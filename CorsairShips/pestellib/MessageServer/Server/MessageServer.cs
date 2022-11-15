using System;
using System.Threading;
using MessageClient.Sources;
using MessageServer.Sources;
using PestelLib.ServerCommon.Threading;

namespace MessageServer.Server
{
    public class MessageServer : DisposableLoop
    {
        private readonly IMessageProvider _provider;
        private readonly BaseMessageDispatcher _dispatcher;

        public MessageServerStats Stats => _provider.Stats;

        public MessageServer(IMessageProvider provider, BaseMessageDispatcher dispatcher)
        {
            _provider = provider;
            _dispatcher = dispatcher;
            _dispatcher.OnError += _error;
        }

        private void _error(MessageFrom messageFrom)
        {
            _provider.DisconnectSender(messageFrom.Sender);
        }

        protected override void Update(CancellationToken cancellationToken)
        {
            if(_provider == null) return;
            var msg = _provider.GetMessage();
            if(msg.Sender == 0)
                return;

            _dispatcher.Dispatch(msg);
        }

        public override void Dispose()
        {
            _dispatcher.OnError -= _error;
            _dispatcher.Dispose();
            base.Dispose();
        }
    }
}
