using MessageServer.Sources;

namespace ProxyClientLib.Sources
{
    interface IProxyClientTransportFactory
    {
        IMessageProviderEvents CreateMessageProvider();
        IMessageSender CreateMessageSender(IMessageProviderEvents messageProvider);
    }
}
