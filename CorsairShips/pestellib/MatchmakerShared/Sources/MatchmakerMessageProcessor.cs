using System;
using System.Collections.Generic;
using System.Linq;
using ServerShared;
using log4net;
using System.Threading;
using System.Reflection;

namespace PestelLib.MatchmakerShared
{
    class MatchmakerMessageStreamContext
    {
        private static object _sync = new object();
        private static long _nextId = 1;

        public MatchmakerMessageStream Stream;
        public long Id;

        public MatchmakerMessageStreamContext(MatchmakerMessageStream stream)
        {
            Stream = stream;
            lock (_sync)
            {
                Id = _nextId;
                ++_nextId;
            }
        }

        public override bool Equals(object obj)
        {
            var ctx = obj as MatchmakerMessageStreamContext;
            if (ctx == null)
                return false;
            return ctx.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class MatchmakerMessageProcessor : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MatchmakerMessageProcessor));
        static readonly MatchmakerMessage[] VoidRet = new MatchmakerMessage[] { };
        Dictionary<Type, List<object>> _callbacks = new Dictionary<Type, List<object>>();
        private readonly List<Thread> _threads = new List<Thread>();
        private Dictionary<long, MatchmakerMessageStreamContext> _streams = new Dictionary<long, MatchmakerMessageStreamContext>();
        private Dictionary<Type, MethodInfo> _processMap = new Dictionary<Type, MethodInfo>();

        public bool CanWrite { get { return _streams.Count > 0; } }

        public event Action<long> OnClose = (s) => { };

        public MatchmakerMessageProcessor() {}
        public MatchmakerMessageProcessor(MatchmakerMessageStream messageStream)
        {
            AddSource(messageStream);
        }

        public void AddSource(MatchmakerMessageStream messageStream)
        {
            var thread = new Thread(MessageReadLoop)
            {
                Name = "MatchmakerMessageProcessor",
                IsBackground = true
            };
            thread.Start(new MatchmakerMessageStreamContext(messageStream));
            lock (_threads)
                _threads.Add(thread);
        }

        public IObjectScopeGuard RegisterCallback<T>(Func<T, long, MatchmakerMessage> callback) where T : MatchmakerMessage
        {
            var t = typeof(T);
            List<object> list;
            if (!_callbacks.TryGetValue(t, out list))
            {
                list = new List<object>();
                _callbacks[t] = list;
            }

            lock (list)
                list.Add(callback);
            var result = new ObjectScopeGuard<Func<T, long, MatchmakerMessage>>(callback, (c) =>
            {
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    lock (list) list.Remove(c);
                });
            });
            return result;
        }

        public int SendMessage(MatchmakerMessage message)
        {
            long dest;
            lock (_streams)
                dest = _streams.Count == 1 ? _streams.First().Key : -1L;
            if (dest < 0)
                return 0;
            return SendMessage(message, dest);
        }

        public int SendMessage(MatchmakerMessage message, long dest)
        {
            MatchmakerMessageStreamContext ctx;
            lock (_streams)
                if (!_streams.TryGetValue(dest, out ctx))
                    return 0;
            ctx.Stream.Write(message);
            return 1;
        }

        public int SendMessage(MatchmakerMessage message, params long[] dests)
        {
            var result = 0;
            for (var i = 0; i < dests.Length; ++i)
                result += SendMessage(message, dests[i]);
            return result;
        }

        public void Close(long streamId)
        {
            MatchmakerMessageStreamContext ctx;
            lock (_streams)
                if (_streams.TryGetValue(streamId, out ctx))
                    ctx.Stream.Close();
        }

        private MethodInfo GetProcess(Type t)
        {
            MethodInfo mi;
            lock (_processMap)
                _processMap.TryGetValue(t, out mi);
            if (mi != null)
                return mi;

            var process = GetType().GetMethod("Process", BindingFlags.NonPublic | BindingFlags.Instance);
            var processInst = process.MakeGenericMethod(t);
            lock (_processMap)
                _processMap[t] = processInst;
            return processInst;
        }

        private void MessageReadLoop(object obj)
        {
            var ctx = obj as MatchmakerMessageStreamContext;
            var messageStream = ctx.Stream;
            object message;
            Log.DebugFormat("Stream {0} registered", ctx.Id);
            
            lock (_streams)
                _streams.Add(ctx.Id, ctx);
            try
            {
                do
                {
                    message = messageStream.Read();
                    if (message == null)
                        return;
                    var ans = (MatchmakerMessage[])GetProcess(message.GetType()).Invoke(this, new object[] { message, ctx.Id, true });
                    for (var i = 0; i < ans.Length; i++)
                    {
                        messageStream.Write(ans[i]);
                    }
                } while (message != null);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                Log.DebugFormat("Stream {0} closed", ctx.Id);
                ThreadPool.QueueUserWorkItem((o) => OnClose(ctx.Id));
                lock (_threads)
                {
                    _threads.RemoveAll(_ => _.ManagedThreadId == Thread.CurrentThread.ManagedThreadId);
                }
                messageStream.Dispose();
                lock (_streams)
                    _streams.Remove(ctx.Id);
            }
        }

        private MatchmakerMessage[] FireCallbacks<T>(T message, long from, out bool processed) where T : MatchmakerMessage
        {
            processed = false;
            var t = message.GetType();
            List<object> list;
            if (!_callbacks.TryGetValue(t, out list))
                return VoidRet;

            var results = new List<MatchmakerMessage>();
            var errors = false;
            lock (list)
            {
                foreach (var a in list)
                {
                    MatchmakerMessage r = null;
                    try
                    {
                        r = ((Func<T, long, MatchmakerMessage>)a)(message, from);
                        if (r != null)
                            results.Add(r);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        errors = true;
                    }
                }
            }
            if (errors && !results.Any())
            {
                throw new Exception("There was an error while processing message");
            }

            processed = true;
            return results.ToArray();
        }

        protected MatchmakerMessage[] Process<T>(T message, long from, bool throwIfUnknown) where T: MatchmakerMessage
        {
            bool b;
            var answers = FireCallbacks(message, from, out b);
            if (throwIfUnknown && !b)
                throw new NotSupportedException(string.Format("Message type '{0}' not supported", typeof(T)));
            return answers;
        }

        public void Dispose()
        {
            lock (_streams)
            {
                foreach (var s in _streams.Values)
                {
                    s.Stream.Close();
                }
                _streams.Clear();
            }
            List<Thread> threads;
            lock (_threads)
            {
                threads = _threads.ToList();
            }
            foreach (var thread in threads)
            {
                if (Thread.CurrentThread != thread)
                    thread.Join();
            }
            _threads.Clear();
        }
    }
}
