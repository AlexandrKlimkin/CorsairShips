using MessageClient.Sources;

namespace MessageServer.Sources
{
    public class MessageClient
    {
        private readonly BaseMessageDispatcher _dispatcher;

        public MessageClient(IMessageProviderEvents messageProvider, BaseMessageDispatcher dispatcher)
        {
            messageProvider.OnMessage += OnMessage;
            _dispatcher = dispatcher;
        }

        private void OnMessage(Message message, IAnswerContext answerContext)
        {
            var fromMsg = new MessageFrom();
            fromMsg.Message = message;
            fromMsg.AnswerContext = answerContext;
            _dispatcher.Dispatch(fromMsg);
        }
    }
}
