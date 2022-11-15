using MessageClient.Sources;

namespace ClansClientLib.Private
{
    public partial class ClansClientMessageServerAdapter
    {
        class Dispatcher : StMessageDispatcher
        {
            public Dispatcher(ClansClientMessageServerAdapter client)
            {
                RegisterHandler((int)ClanMessageType.AskUpdateClan, new IncomingMessageNotifyHandler<byte>(client.AskUpdateClanMessage));
                RegisterHandler((int)ClanMessageType.AskUpdateBoosters, new IncomingMessageNotifyHandler<byte>(client.AskUpdateBoostersMessage));
                RegisterHandler((int)ClanMessageType.AskUpdateRequests, new IncomingMessageNotifyHandler<byte>(client.AskUpdateRequestsMessage));
                RegisterHandler((int)ClanMessageType.AskUpdateConsumables, new IncomingMessageNotifyHandler<byte>(client.AskUpdateConsumablesMessage));
            }
        }
    }
}
