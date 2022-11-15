using System;
using System.Collections.Generic;
using FriendsClient.Sources;
using FriendsClient.Sources.Serialization;
using MessageClient.Sources;
using MessageServer.Sources;
using UnityDI;

namespace FriendsClient.Private
{
    public partial class FriendsClient
    {
        private List<RoomInviteEventMessage> _postponedInvites = new List<RoomInviteEventMessage>();
        public void IncomingMessage(FriendEvent evt, object data)
        {
            switch (evt)
            {
                case FriendEvent.FriendInvite: OnInvite((FriendsInviteEventMessage) data); break;
                case FriendEvent.FriendAccept: OnInviteAccepted((FriendsInviteEventMessage) data); break;
                case FriendEvent.FriendReject: OnInviteRejected((FriendsInviteEventMessage) data); break;
                case FriendEvent.FriendInviteCancel: OnInviteCanceled((FriendsInviteEventMessage)data); break;
                case FriendEvent.FriendInviteExpired: OnInviteExpired((FriendsInviteEventMessage)data); break;
                case FriendEvent.FriendRemoved: OnFriendRemoved((FriendEventMessage) data); break;
                case FriendEvent.NewFriend: OnNewFriend((FriendEventMessage)data); break;
                case FriendEvent.StatusChanged: OnFriendStatus((FriendStatusChangedMessage)data); break;
                case FriendEvent.Gift: OnFriendGift((FriendGiftEventMessage) data); break;
                case FriendEvent.GiftClaimed: OnFriendGiftClaimed((FriendGiftEventMessage) data); break;
                case FriendEvent.RoomInvite:
                    if (!MuteBattleInvitations)
                    {
                        var invite = (RoomInviteEventMessage) data;
                        if (SaveNewInvite(invite))
                            OnRoomInvite(invite);
                    }
                    else
                    {
                        _postponedInvites.Add((RoomInviteEventMessage)data);
                    }
                    break;
                case FriendEvent.RoomAccept: OnRoomAccept((RoomInviteEventMessage) data); break;
                case FriendEvent.RoomReject: RoomInviteReject((RoomInviteEventMessage) data); break;
                case FriendEvent.RoomAutoReject: OnRoomAutoReject((RoomInviteEventMessage) data); break;
                case FriendEvent.RoomHostChange: OnRoomNewHost((RoomChangeHostEventMessage) data); break;
                case FriendEvent.RoomJoin: OnRoomJoin((RoomJoinEventMessage) data); break;
                case FriendEvent.RoomLeave: OnRoomLeave((RoomLeaveKickEventMessage) data); break;
                case FriendEvent.RoomKick: OnRoomKick((RoomLeaveKickEventMessage) data); break;
                case FriendEvent.RoomStartBattle: OnRoomStartBattle((RoomInfoEventMessage) data); break;
                case FriendEvent.RoomInfo: OnRoomInfo((RoomInfoEventMessage) data); break;
                case FriendEvent.RoomCountdown: OnRoomCountdown((RoomCountdownEventMessage) data); break;
                case FriendEvent.RoomGameData: OnRoomGameData((RoomGameDataMessage) data); break;
                case FriendEvent.ProfileUpdate: OnProfileUpdate((FriendsProfileUpdateMessage)data); break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(evt), evt, null);
            }
        }

        public void IncomingInvite(FriendsInviteEventMessage evt)
        {
            var a = OnInvite;
            OnInvite(evt);
        }

        class FriendsClientMessageHandler<T> : IMessageHandler
        {
            private readonly FriendsClient _client;
            private readonly FriendEvent _eventType;

            public FriendsClientMessageHandler(FriendsClient client, FriendEvent eventType)
            {
                _client = client;
                _eventType = eventType;
            }

            public void Handle(int sender, byte[] data, int tag, IAnswerContext answerContext)
            {
                var evt = FriendServerSerializer.Deserialize<T>(data);
                _client.IncomingMessage(_eventType, evt);
            }

            public void Error(int tag)
            { }
        }

        public class FriendsMessageClientDispatcher : StMessageDispatcher
        {
            private readonly FriendsClient _client;

            public FriendsMessageClientDispatcher(FriendsClient client)
            {
                _client = client;

                RegisterHandler((int)FriendEvent.FriendInvite, new FriendsClientMessageHandler<FriendsInviteEventMessage>(_client,FriendEvent.FriendInvite));
                RegisterHandler((int)FriendEvent.FriendAccept, new FriendsClientMessageHandler<FriendsInviteEventMessage>(_client, FriendEvent.FriendAccept));
                RegisterHandler((int)FriendEvent.FriendReject, new FriendsClientMessageHandler<FriendsInviteEventMessage>(_client, FriendEvent.FriendReject));
                RegisterHandler((int)FriendEvent.FriendInviteCancel, new FriendsClientMessageHandler<FriendsInviteEventMessage>(_client, FriendEvent.FriendInviteCancel));
                RegisterHandler((int)FriendEvent.FriendInviteExpired, new FriendsClientMessageHandler<FriendsInviteEventMessage>(_client, FriendEvent.FriendInviteExpired));
                RegisterHandler((int)FriendEvent.FriendRemoved, new FriendsClientMessageHandler<FriendEventMessage>(_client, FriendEvent.FriendRemoved));
                RegisterHandler((int)FriendEvent.NewFriend, new FriendsClientMessageHandler<FriendEventMessage>(_client, FriendEvent.NewFriend));
                RegisterHandler((int)FriendEvent.StatusChanged, new FriendsClientMessageHandler<FriendStatusChangedMessage>(_client, FriendEvent.StatusChanged));
                RegisterHandler((int)FriendEvent.Gift, new FriendsClientMessageHandler<FriendGiftEventMessage>(_client, FriendEvent.Gift));
                RegisterHandler((int)FriendEvent.GiftClaimed, new FriendsClientMessageHandler<FriendGiftEventMessage>(_client, FriendEvent.GiftClaimed));
                RegisterHandler((int)FriendEvent.RoomInvite, new FriendsClientMessageHandler<RoomInviteEventMessage>(_client, FriendEvent.RoomInvite));
                RegisterHandler((int)FriendEvent.RoomAccept, new FriendsClientMessageHandler<RoomInviteEventMessage>(_client, FriendEvent.RoomAccept));
                RegisterHandler((int)FriendEvent.RoomReject, new FriendsClientMessageHandler<RoomInviteEventMessage>(_client, FriendEvent.RoomReject));
                RegisterHandler((int)FriendEvent.RoomAutoReject, new FriendsClientMessageHandler<RoomInviteEventMessage>(_client, FriendEvent.RoomAutoReject));
                RegisterHandler((int)FriendEvent.RoomHostChange, new FriendsClientMessageHandler<RoomChangeHostEventMessage>(_client, FriendEvent.RoomHostChange));
                RegisterHandler((int)FriendEvent.RoomJoin, new FriendsClientMessageHandler<RoomJoinEventMessage>(_client, FriendEvent.RoomJoin));
                RegisterHandler((int)FriendEvent.RoomLeave, new FriendsClientMessageHandler<RoomLeaveKickEventMessage>(_client, FriendEvent.RoomLeave));
                RegisterHandler((int)FriendEvent.RoomKick, new FriendsClientMessageHandler<RoomLeaveKickEventMessage>(_client, FriendEvent.RoomKick));
                RegisterHandler((int)FriendEvent.RoomStartBattle, new FriendsClientMessageHandler<RoomInfoEventMessage>(_client, FriendEvent.RoomStartBattle));
                RegisterHandler((int)FriendEvent.RoomInfo, new FriendsClientMessageHandler<RoomInfoEventMessage>(_client, FriendEvent.RoomInfo));
                RegisterHandler((int)FriendEvent.RoomCountdown, new FriendsClientMessageHandler<RoomCountdownEventMessage>(_client, FriendEvent.RoomCountdown));
                RegisterHandler((int)FriendEvent.RoomGameData, new FriendsClientMessageHandler<RoomGameDataMessage>(_client, FriendEvent.RoomGameData));
                RegisterHandler((int)FriendEvent.ProfileUpdate, new FriendsClientMessageHandler<FriendsProfileUpdateMessage>(_client, FriendEvent.ProfileUpdate));
            }
        }
    }
}
