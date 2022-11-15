using System;
using System.Diagnostics;
using log4net;
using MessageServer.Sources;

namespace MessageClient.Sources
{
    public class StMessageDispatcher : BaseMessageDispatcher
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(StMessageDispatcher));

        protected override void Process(MessageFrom message, IMessageHandler handler)
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                handler.Handle(message.Sender, message.Message.Data, message.Message.Tag, message.AnswerContext);
            }
            catch (Exception e)
            {
                ++Stats._handleErrors;
                _log.Error($"Error while handling message type {message.Message.Type}. sender={message.Sender}.", e);
                _error(message);
            }
            finally
            {
                ++Stats._handleCount;
                Stats._handleTime += sw.ElapsedMilliseconds;
            }
        }
    }
}
