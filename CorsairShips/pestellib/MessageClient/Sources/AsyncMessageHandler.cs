using System;
using System.Threading.Tasks;
using log4net;
using MessageServer.Sources;
using PestelLib.UniversalSerializer;

namespace MessageClient
{
    public class AsyncMessageHandler<RequestT, ResponseT> : IMessageHandler
    {
        private static ILog Log = LogManager.GetLogger(typeof(AsyncMessageHandler<RequestT, ResponseT>));
        private readonly Func<int, RequestT, Task<ResponseT>> _callback;
        private readonly ISerializer _serializer;

        public AsyncMessageHandler(ISerializer serializer, Func<int, RequestT, Task<ResponseT>> callback)
        {
            _callback = callback;
            _serializer = serializer;
        }

        public void Handle(int sender, byte[] data, int tag, IAnswerContext answerContext)
        {
            var msg = _serializer.Deserialize<RequestT>(data);
            _callback(sender, msg).ContinueWith(_ =>
                {
                    if (answerContext == null) return;
                    var ansData = _serializer.Serialize(_.Result);
                    if (ansData.Length > 500000)
                        Log.Warn($"Big answer size {ansData.Length}.");
                    var r = answerContext.Answer(0, ansData);
                    if (!r)
                        Log.Error($"Answer not sent.");
                })
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public void Error(int tag)
        { }
    }
}
