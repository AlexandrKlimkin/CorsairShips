using System;
using System.Collections.Generic;
using System.Text;
using MessageServer.Sources;
using MessageServer.Sources.Tcp;
using ServerShared;
using UnityDI;

namespace FriendsClient
{
    public interface IFriendsClientTransportFactory
    {
        IMessageProviderEvents CreateMessageProvider();
        IMessageSender CreateMessageSender(IMessageProviderEvents messageProvider);
    }

    public class DefaultFriendsClientTransportFactory : IFriendsClientTransportFactory
    {
#pragma warning disable 649
        [Dependency]
        private IUpdateProvider _updateProvider;
#pragma warning restore 649

        private string _host = "localhost";
        private int _port = 9001;

        private static readonly DefaultFriendsClientTransportFactory _instance = new DefaultFriendsClientTransportFactory();

        public static DefaultFriendsClientTransportFactory Instance {
            get { return _instance; }
        }

        public DefaultFriendsClientTransportFactory(string host, int port)
        :this()
        {
            _host = host;
            _port = port;
        }

        private DefaultFriendsClientTransportFactory()
        {
            ContainerHolder.Container.BuildUp(this);
        }

        public IMessageProviderEvents CreateMessageProvider()
        {
            return new TcpClientMessageProvider(_host, _port, _updateProvider);
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
