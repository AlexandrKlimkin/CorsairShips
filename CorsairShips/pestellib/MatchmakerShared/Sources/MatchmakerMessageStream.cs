using System;
using System.IO;
using System.Net.Sockets;
using log4net;
using ServerShared;
using System.Threading;
using System.Net;

namespace PestelLib.MatchmakerShared
{
    class ReadContext : IDisposable
    {
        public byte[] array;
        public int len;
        public int read;
        public volatile bool closed;
        public ManualResetEvent waitEvent;
        public Exception exception;
        public EndPoint remote;
        public DateTime LastRead;

        public void Dispose()
        {
            if(waitEvent != null)
                waitEvent.Close();
            waitEvent = null;
        }
    }

    public class MatchmakerMessageStream : IDisposable
    {
        private readonly TimeSpan InactivityTimeout = TimeSpan.FromSeconds(300);
        static readonly ILog Log = LogManager.GetLogger(typeof(MatchmakerMessageStream));
        static readonly BufferManager<byte> BufferManager = new BufferManager<byte>(1000000);
        private object _readSync = new object();
        private object _writeSync = new object();
        private readonly Stream _stream;
        private Socket _socket;
        private volatile bool _disposing;
        private object _sync = new object();
        private ReadContext _readContext;

        public MatchmakerMessageStream(TcpClient c)
            :this(c.GetStream())
        {
            _socket = c.Client;
        }

        public MatchmakerMessageStream(Stream s)
        {
            _stream = s;
        }

        public void Close()
        {
            Log.InfoFormat("Closing stream");
            Dispose();
        }

        private bool WrapSocketErrors(Action action)
        {
            EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
            Exception exception;
            try
            {
                ep = _socket.RemoteEndPoint;
            }
            catch { }
            var r = WrapSocketErrors(action, ep, out exception);
            if (exception != null)
            {
                Log.Error(exception);
            }
            return r;
        }

        private bool WrapSocketErrors(Action action, EndPoint remote, out Exception exception)
        {
            exception = null;
            try
            {
                action();
                return true;
            }
            catch (ObjectDisposedException)
            {
                Log.DebugFormat("Connection {0} closed by us", remote.ToString());
            }
            catch (IOException e)
            {
                var socketError = e.InnerException as SocketException;
                var disposedException = e.InnerException as ObjectDisposedException;
                if (socketError != null)
                {
                    if (socketError.SocketErrorCode == SocketError.ConnectionReset)
                        Log.DebugFormat("Connection {0} reset by remote host", remote.ToString());
                    else
                        Log.DebugFormat("Socket error: " + socketError.SocketErrorCode);
                }
                else if (disposedException != null)
                {
                    Log.DebugFormat("Connection {0} closed by us", remote.ToString());
                }
                else
                    exception = e;
            }
            catch (Exception e)
            {
                exception = e;
            }
            return false;
        }

        private void ReadCallback(IAsyncResult r)
        {
            var ctx = (ReadContext)r.AsyncState;
            var before = ctx.read;
            var success =
            WrapSocketErrors(
                () => ctx.read += _stream.EndRead(r),
                ctx.remote, 
                out ctx.exception);
            if (success && ctx.read - before == 0)
            {
                ctx.closed = true;
                Log.Debug("0 bytes read. Closing stream");
            }
            ctx.LastRead = DateTime.UtcNow;
            try
            {
                if (ctx.waitEvent != null)
                    ctx.waitEvent.Set();
            }
            catch {}
        }

        private bool ReadBytes(byte[] array, int len)
        {
            EndPoint remote;
            try
            {
                remote = _socket.RemoteEndPoint;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            lock (_sync)
            {
                var ctx = _readContext = new ReadContext {
                    array = array,
                    len = len,
                    read = 0,
                    waitEvent = new ManualResetEvent(true),
                    remote = remote,
                    LastRead = DateTime.UtcNow
                };
                do
                {
                    if (!ctx.waitEvent.WaitOne(100))
                    {
                        // inactivity timeout check
                        var d = DateTime.UtcNow - ctx.LastRead;
                        if (d >= InactivityTimeout)
                        {
                            Log.InfoFormat("{0} is inactive for {1} secs. Connection closed.", remote.ToString(), d.TotalSeconds);
                            return false;
                        }

                        continue;
                    }

                    if (ctx.read == ctx.len)
                        return true;
                    if (ctx.exception != null)
                        throw ctx.exception;
                    if (!_stream.CanRead || !_socket.Connected)
                        return false;
                    if (ctx.closed)
                        return false;
                    ctx.waitEvent.Reset();

                    var r =
                    WrapSocketErrors(
                        () => _stream.BeginRead(ctx.array, ctx.read, ctx.len - ctx.read, ReadCallback, ctx),
                        ctx.remote, 
                        out ctx.exception);
                    if (ctx.exception != null)
                        throw ctx.exception;
                    if (!r)
                        return r;
                } while (!_disposing);
            }

            try
            {
                if (_readContext.waitEvent != null)
                    _readContext.waitEvent.Set();
            }
            catch { }
            return false;
        }

        public MatchmakerMessage Read()
        {
            MatchmakerMessage result;
            var sz = 0;
            lock (_readSync)
            {
                using (var sizeBytes = BufferManager.AllocBufferGuard(4))
                {
                    if (!ReadBytes(sizeBytes.Obj, 4))
                        return null;
                    sz = GetSize(sizeBytes.Obj, sizeBytes.Obj.Length);
                }

                using (var buffer = BufferManager.AllocBufferGuard(sz))
                {
                    if (!ReadBytes(buffer.Obj, sz))
                        return null;
                    result = MatchmakerMessage.Deserialize<MatchmakerMessage>(buffer.Obj, sz);
                }
            }

            return result;
        }

        public void Write(MatchmakerMessage message)
        {
            var data = message.Serialize();
            var sz = data.Length;
            using (var buffer = BufferManager.AllocBufferGuard(sz + 4))
            {
                WriteSize(buffer.Obj, 0, sz);
                Array.Copy(data, 0, buffer.Obj, 4, sz);
                lock (_writeSync)
                {
                    WrapSocketErrors(
                        () => _stream.Write(buffer.Obj, 0, sz + 4));
                }
            }
        }

        public void Dispose()
        {
            if (_disposing)
            {
                return;
            }
            _disposing = true;
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            } catch { }
            if (_readContext != null)
            {
                try
                {
                    _readContext.waitEvent.WaitOne(5000);
                    _readContext.Dispose();
                }
                catch {}
                _readContext = null;
            }
            try
            {
                _stream.Close();
            } catch {}
        }

        private int GetSize(byte[] bytes, int len)
        {
            if (len < 4)
                return 0;

            return bytes[0]
                   | bytes[1] << 8
                   | bytes[2] << 16
                   | bytes[3] << 24;
        }

        private void WriteSize(byte[] bytes, int offset, int size)
        {
            if (bytes.Length < offset + 4)
                return;
            bytes[offset + 0] = (byte)size;
            bytes[offset + 1] = (byte)(size >> 8);
            bytes[offset + 2] = (byte)(size >> 16);
            bytes[offset + 3] = (byte)(size >> 24);
        }
    }
}
