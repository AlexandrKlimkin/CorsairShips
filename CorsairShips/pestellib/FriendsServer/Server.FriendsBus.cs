using FriendsClient.Private;
using FriendsClient.Sources.Serialization;
using FriendsServer.Bus;
using PestelLib.ServerCommon.Extensions;
using System;
using System.Threading.Tasks;

namespace FriendsServer
{
    partial class Server
    {
        private FriendsBus _friendsBus = FriendsBus.Instance;

        private void InitBus()
        {
            if(!_friendsBus.Enabled)
                return;
            _friendsBus.OnStatusChanged += BusOnStatusChanged;
            _friendsBus.OnFriendInvite += BusOnFriendInvite;
            _friendsBus.OnFriendInviteCanceled += BusOnFriendInviteCanceled;
            _friendsBus.OnFriendInviteAnswered += BusOnFriendInviteAnswered;
            _friendsBus.OnFriendRemoved += BusOnFriendRemoved;
            _friendsBus.OnGiftSent += BusOnGiftSent;
            _friendsBus.OnGiftClaim += BusOnGiftClaim;
            _friendsBus.OnPlayerDelete += BusOnPlayerDelete;
        }

        private void DeinitBus()
        {
            _friendsBus.OnStatusChanged -= BusOnStatusChanged;
            _friendsBus.OnFriendInvite -= BusOnFriendInvite;
            _friendsBus.OnFriendInviteCanceled -= BusOnFriendInviteCanceled;
            _friendsBus.OnFriendInviteAnswered -= BusOnFriendInviteAnswered;
            _friendsBus.OnFriendRemoved -= BusOnFriendRemoved;
            _friendsBus.OnGiftSent -= BusOnGiftSent;
            _friendsBus.OnGiftClaim -= BusOnGiftClaim;
            _friendsBus.OnPlayerDelete -= BusOnPlayerDelete;
        }

        private void BusOnGiftClaim(FriendGiftEventMessageGlobal evt)
        {
            if (_madIdToSenderMap.TryGetValue(evt.Target, out var target))
            {
                var data = FriendServerSerializer.Serialize(evt.Message);
                _messageSender.Notify(target, (int)evt.Message.Event, data);
            }
        }

        private void BusOnGiftSent(FriendGiftEventMessageGlobal evt)
        {
            if (_madIdToSenderMap.TryGetValue(evt.Target, out var target))
            {
                var data = FriendServerSerializer.Serialize(evt.Message);
                _messageSender.Notify(target, (int)evt.Message.Event, data);
            }
        }

        private void BusOnFriendRemoved(FriendEventMessageGlobal evt)
        {
            if (_madIdToSenderMap.TryGetValue(evt.Target, out var target))
            {
                var data = FriendServerSerializer.Serialize(evt.Message);
                _messageSender.Notify(target, (int)evt.Message.Event, data);
            }
        }

        private void BusOnFriendInviteAnswered(FriendsInviteEventGlobal evt)
        {
            if (_madIdToSenderMap.TryGetValue(evt.Target, out var target))
            {
                var data = FriendServerSerializer.Serialize(evt.Message);
                _messageSender.Notify(target, (int)evt.Message.Event, data);
            }
        }

        private void BusOnFriendInviteCanceled(FriendsInviteEventGlobal evt)
        {
            if (_madIdToSenderMap.TryGetValue(evt.Target, out var target))
            {
                var data = FriendServerSerializer.Serialize(evt.Message);
                _messageSender.Notify(target, (int)evt.Message.Event, data);
            }
        }

        private void BusOnFriendInvite(FriendsInviteEventGlobal evt)
        {
            if (_madIdToSenderMap.TryGetValue(evt.Target, out var target))
            {
                var data = FriendServerSerializer.Serialize(evt.Message);
                _messageSender.Notify(target, (int)evt.Message.Event, data);
            }
        }

        private void BusOnStatusChanged(FriendStatusChangedMessage evt)
        {
            GetNotifyListForStatusChange(evt.From).ContinueWith(t =>
            {
                var data = FriendServerSerializer.Serialize(evt);
                _messageSender.Notify(t.Result, (int)evt.Event, data);
            }).ReportOnFail();
        }

        private void BusOnPlayerDelete(Guid playerId)
        {
            BusOnPlayerDeleteAsync(playerId).ReportOnFail();
        }

        private async Task BusOnPlayerDeleteAsync(Guid playerId)
        {
            var friends = await _friendStorage.GetMany(playerId);
            if (friends.Length == 0)
                return;
            var deletedPlayer = friends[0];

            string report = $"Deleted player {playerId}:{deletedPlayer.Id} friend-list ({deletedPlayer.Friends.Length}) ";
            bool r;
            foreach (var f in deletedPlayer.Friends)
            {
                r = await RemoveFriendInt(deletedPlayer.Id, f);
                report += $"{f}={r} ";
            }
            r = await _friendStorage.Remove(deletedPlayer.Id);
            report += $"friend item remove={r}";
            Log.Debug(report);
        }
    }
}
