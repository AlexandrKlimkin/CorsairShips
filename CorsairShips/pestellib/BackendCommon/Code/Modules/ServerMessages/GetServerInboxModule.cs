using System;
using log4net;
using PestelLib.ServerCommon.Messaging;
using PestelLib.ServerShared;
using S;
using ServerLib.Modules.Messages;
using UnityDI;
using MessagePack;

namespace ServerLib.Modules.ServerMessages
{
    public class GetServerInboxModule : IModule
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GetServerInboxModule));

        public ServerResponse ProcessCommand(ServerRequest serverRequest)
        {
            var userId = new Guid(serverRequest.Request.UserId);

            var inboxRaw = ServerMessageUtils.GetMessages(userId);
            try
            {
                inboxRaw = MixinBroadcasts(inboxRaw, userId, serverRequest);
            }
            catch(Exception e)
            {
                Log.Warn("Broadcast storage error.", e);
            }

            ServerMessageUtils.ClearInbox(userId);

            return new ServerResponse
            {
                Data = inboxRaw,
                PlayerId = new Guid(serverRequest.Request.UserId),
                ResponseCode = ResponseCode.OK
            };
        }

        private byte[] MixinBroadcasts(byte[] rawInbox, Guid userId, ServerRequest serverRequest)
        {
            var request = serverRequest.Request.GetServerMessagesInbox;
            if (!request.GetBroadcasts) return rawInbox;

            var broadcastMessageStorage = ContainerHolder.Container.Resolve<BroadcastMessageStorage>();
            var filterMatcher = ContainerHolder.Container.Resolve<PlayerFilterMatcher>();
            if (broadcastMessageStorage != null && filterMatcher != null)
            {
                var playerFilter = filterMatcher.PlayerFilters(serverRequest.Request);
                var broadcastMessages = broadcastMessageStorage.GetMessages(request.LastSeenBroadcastSerialId, userId.ToString(), playerFilter, request.Sequence > 0, request.StateBirthday);
                if (broadcastMessages.Length > 0)
                {
                    var inbox = rawInbox != null ? MessagePackSerializer.Deserialize<ServerMessagesInbox>(rawInbox) : new ServerMessagesInbox();
                    inbox.Messages.AddRange(broadcastMessages);
                    rawInbox = MessagePackSerializer.Serialize(inbox);
                }
            }

            return rawInbox;
        }
    }
}