using System;
using System.Linq;
using ClansClientLib;
using log4net;
using MessagePack;
using MessageServer.Sources;
using UnityDI;

namespace ClansServerLib
{
    class NotifyPlayers : INotifyPlayers
    {
        public NotifyPlayers()
        {
            ContainerHolder.Container.BuildUp(this);
            _dummyMessage = MessagePackSerializer.Serialize((byte) 1);
            _db = new Lazy<IClanDB>(() => ContainerHolder.Container.Resolve<IClanDB>());
        }

        public void AskPlayerToUpdateHisClan(Guid playerId)
        {
            var target = _userBinder.GetSender(playerId);
            if (target != 0)
                _messageSender.Notify(target, (int) ClanMessageType.AskUpdateClan, _dummyMessage);
        }

        public void AskPlayerToUpdateRequests(Guid playerId)
        {
            var target = _userBinder.GetSender(playerId);
            if (target != 0)
                _messageSender.Notify(target, (int)ClanMessageType.AskUpdateRequests, _dummyMessage);
        }

        public void AskClanToUpdateBoosters(Guid clanId)
        {
            _db.Value.GetClan(clanId).ContinueWith(_ =>
            {
                if (_.Result == null)
                {
                    Log.Error($"Clan not found {clanId}.");
                    return;
                }

                var members = _.Result.Members;
                var senders = _userBinder.GetSenders(members.Select(_ => _.Id)).ToArray();
                if (senders.Length > 0)
                    _messageSender.Notify(senders, (int)ClanMessageType.AskUpdateBoosters, _dummyMessage);
            });
        }

        public void AskClanToUpdate(Guid clanId)
        {
            _db.Value.GetClan(clanId).ContinueWith(_ =>
            {
                if (_.Result == null)
                {
                    Log.Error($"Clan not found {clanId}.");
                    return;
                }
                var members = _.Result.Members;
                var senders = _userBinder.GetSenders(members.Select(_ => _.Id)).ToArray();
                if (senders.Length > 0)
                    _messageSender.Notify(senders, (int)ClanMessageType.AskUpdateClan, _dummyMessage);
            });
        }

        public void AskClanToUpdateConsumables(Guid clanId)
        {
            _db.Value.GetClan(clanId).ContinueWith(_ =>
            {
                if (_.Result == null)
                {
                    Log.Error($"Clan not found {clanId}.");
                    return;
                }
                var members = _.Result.Members;
                var senders = _userBinder.GetSenders(members.Select(_ => _.Id)).ToArray();
                if (senders.Length > 0)
                    _messageSender.Notify(senders, (int)ClanMessageType.AskUpdateConsumables, _dummyMessage);
            });
        }

        [Dependency]
        private ClanServerUserBinder _userBinder;
        [Dependency]
        private IMessageSender _messageSender;

        private Lazy<IClanDB> _db;
        private readonly byte[] _dummyMessage;

        private static readonly ILog Log = LogManager.GetLogger(typeof(NotifyPlayers));
    }
}
