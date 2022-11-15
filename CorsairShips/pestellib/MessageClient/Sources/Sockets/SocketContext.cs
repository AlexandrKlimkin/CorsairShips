using System;
using System.Net.Sockets;
using System.Threading;
using log4net;
using MessagePack;
using ServerShared.Sources;

namespace MessageServer.Sources.Sockets
{
    public class SocketContext : IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SocketContext));
        public Socket Socket { get; }
        public DateTime LastReadTime { get; private set; }
        private GrowingBuffer<byte> _readBuffer = new GrowingBuffer<byte>(3);
        private GrowingBuffer<byte> _writeBuffer = new GrowingBuffer<byte>(3);
        private int _pos;
        private object _readSync = new object();
        private object _writeSync = new object();
        private int _messageSize;

        public bool IsValid { get; private set; }
        public int Id { get; private set; }

        private const int MaxSize = (1 << 24) - 1;

        public class Statistics
        {
            internal long _bytesReceived;
            internal long _bytesSent;
            internal long _errors;

            public long BytesReceived
            {
                get { return _bytesReceived; }
            }
            public long BytesSent
            {
                get { return _bytesSent; }
            }
            public long Errors
            {
                get { return _errors; }
            }
        }

        public Statistics Stats { get; }

        public SocketContext(Socket socket)
        {
            Stats = new Statistics();
            Socket = socket;
            IsValid = true;
            Id = socket.GetHashCode();
            LastReadTime = DateTime.UtcNow;
        }

        public int SendMessage(Message message)
        {
            if (!IsValid || !Socket.Connected)
            {
                IsValid = false;
                return 0;
            }

            try
            {
                var dataRaw = MessagePackSerializer.Serialize(message);
                var sz = dataRaw.Length;
                if (dataRaw.Length > MaxSize)
                    throw new InvalidOperationException(string.Format("Message is to big. sz={0}, max_sz={1}.", sz, MaxSize));
                lock (_writeSync)
                {
                    _writeBuffer.EnsureSize(dataRaw.Length + 3);
                    _writeBuffer.Array[0] = (byte)(sz >> 16);
                    _writeBuffer.Array[1] = (byte)(sz >> 8);
                    _writeBuffer.Array[2] = (byte) sz;
                    Array.Copy((Array) dataRaw, (int) 0, (Array) _writeBuffer.Array, (int) 3, (int) sz);
                    Socket.Send(_writeBuffer.Array, 0, sz + 3, SocketFlags.None);
                    Interlocked.Add(ref Stats._bytesSent, sz + 3);
                    return sz + 3;
                }
            }
            catch (Exception e)
            {
                Interlocked.Increment(ref Stats._errors);
                log.Error($"Write error. target={Id}, message type={message.Type}, stack={Environment.StackTrace}.", e);
                IsValid = false;
            }

            return 0;
        }

        public Message ReadMessage(out int bytesSent)
        {
            bytesSent = 0;
            try
            {
                lock (_readSync)
                {
                    return ReadMessageInt(out bytesSent);
                }
            }
            catch (Exception e)
            {
                Interlocked.Increment(ref Stats._errors);
                log.Error("Read error.", e);
                IsValid = false;
            }
            return new Message();
        }

        public void Close()
        {
            try
            {
                Socket.Close();
            }
            catch (Exception e)
            {
                log.Error("Error on close.", e);
            }
            IsValid = false;
        }

        private Message ReadMessageInt(out int bytesSent)
        {
            bytesSent = 0;
            if (!IsValid)
                return new Message();
            if (_messageSize == 0)
            {
                if (Socket.Available >= 3)
                {
                    var r = Socket.Receive(_readBuffer.Array, 0, 3, SocketFlags.None);
                    if (r == 3)
                    {
                        _messageSize = _readBuffer.Array[0] << 16;
                        _messageSize += _readBuffer.Array[1] << 8;
                        _messageSize += _readBuffer.Array[2];
                        _pos = 0;
                        _readBuffer.EnsureSize(_messageSize);
                    }
                    else if (r == 0)
                    {
                        // gracefully shuted down
                        IsValid = false;
                        return new Message();
                    }
                }
                else
                {
                    // end of stream
                    IsValid = false;
                }
            }

            if (_messageSize > 0)
            {
                var available = Socket.Available;
                if (available > 0)
                {
                    var left = _messageSize - _pos;
                    var toRead = available;
                    if (toRead > left) toRead = left;

                    var r = Socket.Receive(_readBuffer.Array, _pos, toRead, SocketFlags.None);
                    if (r == 0)
                    {
                        // gracefully shuted down
                        IsValid = false;
                        return new Message();
                    }

                    _pos += r;
                    if (_pos == _messageSize)
                    {
                        var result = MessagePackSerializer.Deserialize<Message>(_readBuffer.GetSegment(0, _messageSize));
                        Interlocked.Add(ref Stats._bytesReceived, _messageSize + 3);
                        bytesSent = _messageSize + 3;
                        _messageSize = 0;
                        LastReadTime = DateTime.UtcNow;
                        return result;
                    }
                }
                else
                {
                    // end of stream
                    IsValid = false;
                }
            }

            return new Message();
        }

        public void Dispose()
        {
            if(Socket != null)
                Socket.Dispose();
        }
    }
}
