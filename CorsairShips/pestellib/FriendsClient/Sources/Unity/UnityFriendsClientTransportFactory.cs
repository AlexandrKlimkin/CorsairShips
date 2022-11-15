using System;
using MessageServer.Sources;
using MessageServer.Sources.Tcp;
using PestelLib.ClientConfig;
using ServerShared;
using UnityDI;

namespace FriendsClient.Sources.Unity
{
    public class UnityFriendsClientTransportFactory : IFriendsClientTransportFactory
    {
#pragma warning disable 649
        [Dependency]
        private IUpdateProvider _updateProvider;

        [Dependency] private Config _config;
#pragma warning restore 649
        private Uri _serverUrl;
        public UnityFriendsClientTransportFactory()
        {
            ContainerHolder.Container.BuildUp(this);
            
            _serverUrl = new Uri(_config.FriendsServerUrl);
        }

        public IMessageProviderEvents CreateMessageProvider()
        {
            return new TcpClientMessageProvider(_serverUrl.Host, _serverUrl.Port, _updateProvider);
        }

        public IMessageSender CreateMessageSender(IMessageProviderEvents messageProvider)
        {
            var myProvider = messageProvider as TcpClientMessageProvider;
            if (myProvider == null)
                return null;
            return myProvider;
        }
    }
}
