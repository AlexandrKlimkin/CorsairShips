using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MessageClient.Sources;
using ServerShared;

namespace MessageServer.Sources
{
    public class MtMessageDispatcher : BaseMessageDispatcher
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(MtMessageDispatcher));
        private SemaphoreSlim _concurrencyGuard = new SemaphoreSlim(Environment.ProcessorCount);

        protected override void Process(MessageFrom message, IMessageHandler handler)
        {
            _concurrencyGuard.Wait();
            Task.Run(() =>
            {
                var sw = new Stopwatch();
                sw.Start();
                try
                {
                    handler.Handle(message.Sender, message.Message.Data, message.Message.Tag,
                        message.AnswerContext);
                }
                catch (Exception e)
                {
                    ++Stats._handleErrors;
                    _log.Error(
                        $"Error while handling message type {message.Message.Type}. sender={message.Sender}. " + e.Flatten());
                    _error(message);
                }
                finally
                {
                    ++Stats._handleCount;
                    Stats._handleTime += sw.ElapsedMilliseconds;
                }
            }).ContinueWith<int>(t => _concurrencyGuard.Release());
        }
    }
}
