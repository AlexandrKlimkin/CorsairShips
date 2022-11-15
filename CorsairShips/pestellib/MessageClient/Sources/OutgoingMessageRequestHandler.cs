using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using MessagePack;
using MessageServer.Sources;
using Newtonsoft.Json;

namespace MessageClient
{
    public class OutgoingMessageRequestHandler<ResponseT> : IMessageHandler
    {
        private static OutgoingMessageRequestHandler<ResponseT> _instance = new OutgoingMessageRequestHandler<ResponseT>();
        public static OutgoingMessageRequestHandler<ResponseT> Instance => _instance;

        public OutgoingMessageRequestHandler()
        {
            _failedTags = new HashSet<int>();
            _awaiters = new Dictionary<int, TaskCompletionSource<ResponseT>>();
        }

        public static Task<ResponseT> SendRequest<RequestT>(IMessageSender messageSender, int type, RequestT request)
        {
            var data = MessagePackSerializer.Serialize(request);
            var tag = messageSender.Request(Server, type, data, _instance);
            return _instance.RegisterAwaiter(tag);
        }

        public Task<ResponseT> RegisterAwaiter(int tag)
        {
            var tcs = new TaskCompletionSource<ResponseT>();
            _awaiters[tag] = tcs;
            if (_failedTags.Contains(tag))
                Error(tag);
            return tcs.Task;
        }

        public void Handle(int sender, byte[] data, int tag, IAnswerContext answerContext)
        {
            var msg = MessagePackSerializer.Deserialize<ResponseT>(data);
            try
            {
                if (_awaiters.TryGetValue(tag, out var tcs))
                    tcs.SetResult(msg);
                else
                    Log.Error($"Nobody awaits message with tag {tag}. message={JsonConvert.SerializeObject(msg)}.");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                _awaiters.Remove(tag);
            }
        }

        public void Error(int tag)
        {
            try
            {
                if (_awaiters.TryGetValue(tag, out var tcs))
                {
                    _failedTags.Remove(tag);
                    Log.Warn($"Handle message with tag {tag} error.");
                    tcs.SetException(new Exception());
                }
                else
                {
                    Log.Warn($"Handle message with tag {tag} error. Pending.");
                    _failedTags.Add(tag);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                _awaiters.Remove(tag);
            }
        }

        private HashSet<int> _failedTags;
        private Dictionary<int, TaskCompletionSource<ResponseT>> _awaiters;
        private static readonly ILog Log = LogManager.GetLogger(typeof(OutgoingMessageRequestHandler<ResponseT>));
        private static readonly int Server = 1;
    }
}