using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClansClientLib;
using ClansClientLib.DefsProxy;
using log4net;
using MessageServer.Server;
using MessageServer.Sources;
using Newtonsoft.Json;
using PestelLib.ServerCommon.Db.Auth;
using UnityDI;

namespace ClansServerLib
{
    partial class ClansServer : IDisposable
    {
        public ClansServer(IMessageProvider messageProvider)
        {
            ContainerHolder.Container.BuildUp(this);
            _config = ClansConfigCache.Get();
            _messageProvider = messageProvider;
            _messageProvider.OnSenderDisconnect += _senderDisconnect;
            _messageProvider = messageProvider;
            if(!string.IsNullOrEmpty(_config.TokenStorageConnectionString))
                _tokenStore = TokenStorageFactory.GetStorage(_config.TokenStorageConnectionString);

            _dispatcher = new Dispatcher(this);
            _messageServer = new MessageServer.Server.MessageServer(_messageProvider, _dispatcher);
        }

        private void _senderDisconnect(int sender)
        {
            _userBinder.Unbind(sender);
        }

        private Task<CreateClanResult> CreateClan(int sender, CreateClanRequest request)
        {
            return _db.CreateClan(request.Description, request.Owner);
        }

        private Task<ClanRecord> GetClanByPlayer(int sender, GetClanByPlayerRequest request)
        {
            if (_tokenStore != null)
            {
                var token = _tokenStore.GetByTokenId(request.Token);
                if (token == null || token.PlayerId != request.PlayerId || token.ExpiryTime <= DateTime.UtcNow)
                {
                    Log.Error($"Auth failed. Invalid token {JsonConvert.SerializeObject(token)}. request={JsonConvert.SerializeObject(request)}.");
                    return null;
                }
            }
            _userBinder.Bind(sender, request.PlayerId);
            return _db.GetClanByPlayer(request.PlayerId);
        }

        private bool SecurityCheck(int sender, Func<Guid> playerIdGetter)
        {
            var boundPlayer = _userBinder.GetPlayer(sender);
            var playerId = playerIdGetter();
            var r = boundPlayer == playerId;
            if (!r)
                Log.Error($"Security check failed. Sender id {sender} bound to player {boundPlayer}. Player id in request {playerId}.");

            return r;
        }

        private Task<ClanRecord> GetClan(int arg1, GetClanRequest arg2)
        {
            return _db.GetClan(arg2.ClanId);
        }

        private Task<List<ClanRecord>> GetTopClans(int arg1, GetTopClansRequest arg2)
        {
            return _db.GetTopClans();
        }

        private Task<List<ClanRecord>> FindByNameOrTag(int arg1, FindByNameOrTagRequest arg2)
        {
            return _db.FindByNameOrTag(arg2.SearchText);
        }

        private Task<List<ClanRecord>> FindByParamsExact(int arg1, FindByParamsExactRequest arg2)
        {
            return _db.FindByParamsExact(arg2.Level, arg2.Requirements);
        }

        private Task<List<ClanRecord>> FindByMaxParams(int arg1, FindByMaxParamsRequest arg2)
        {
            return _db.FindByMaxParams(arg2.MaxLevel, arg2.MaxRequirements, arg2.IncludeClansWithoutRequirements, arg2.JoinOpenOnly);
        }

        private Task<ClanJoinRequestResult> JoinClanRequest(int arg1, JoinClanRequestRequest arg2)
        {
            return _db.JoinClanRequest(arg2.ClanId, arg2.PlayerId, arg2.RequestTTL, arg2.ClanMembersLimit);
        }

        private Task<ClanRequestRecord[]> GetMyRequests(int arg1, GetMyRequestsRequest arg2)
        {
            if (!SecurityCheck(arg1, () => arg2.PlayerId))
                return null;
            return _db.GetMyRequests(arg2.PlayerId);
        }

        private Task<ClanRequestRecord[]> GetClanRequests(int arg1, GetClanRequestsRequest arg2)
        {
            return _db.GetClanRequests(arg2.ClanId);
        }

        private Task<AcceptRejectClanRequestResult> AcceptClanRequest(int arg1, AcceptClanRequestRequest arg2)
        {
            if (!SecurityCheck(arg1, () => arg2.CallerId))
                return null;
            return _db.AcceptClanRequest(arg2.RequestId, arg2.CallerId, arg2.Role, arg2.MembersLimit);
        }

        private Task<AcceptRejectClanRequestResult> RejectClanRequest(int arg1, RejectClanRequestRequest arg2)
        {
            if (!SecurityCheck(arg1, () => arg2.CallerId))
                return null;
            return _db.RejectClanRequest(arg2.RequestId, arg2.CallerId);
        }

        private Task<CancelClanRequestResult> CancelClanRequest(int arg1, CancelClanRequestRequest arg2)
        {
            if (!SecurityCheck(arg1, () => arg2.PlayerId))
                return null;
            return _db.CancelClanRequest(arg2.RequestId, arg2.PlayerId);
        }

        private Task<CancelClanRequestResult[]> CancelClanRequests(int arg1, CancelClanRequestsRequest arg2)
        {
            if (!SecurityCheck(arg1, () => arg2.PlayerId))
                return null;
            return _db.CancelClanRequests(arg2.RequestIds, arg2.PlayerId);
        }

        private Task<TransferOwnershipResult> TransferOwnership(int arg1, TransferOwnershipRequest arg2)
        {
            if (!SecurityCheck(arg1, () => arg2.FromPlayerId))
                return null;
            return _db.TransferOwnership(arg2.ClanId, arg2.FromPlayerId, arg2.ToPlayerId);
        }

        private Task<LeaveClanResult> LeaveClan(int arg1, LeaveClanRequest arg2)
        {
            if (!SecurityCheck(arg1, () => arg2.PlayerId))
                return null;
            return _db.LeaveClan(arg2.ClanId, arg2.PlayerId);
        }

        private Task<LeaveClanResult> KickClan(int arg1, KickClanRequest arg2)
        {
            if (!SecurityCheck(arg1, () => arg2.CallerId))
                return null;
            return _db.KickClan(arg2.ClanId, arg2.CallerId, arg2.PlayerIdToKick);
        }

        private Task<bool> SetLevel(int arg1, SetLevelRequest arg2)
        {
            if (!SecurityCheck(arg1, () => arg2.CallerId))
                return null;
            return _db.SetLevel(arg2.ClanId, arg2.CallerId, arg2.CurrLevel, arg2.NewLevel, arg2.Cost);
        }

        private Task<SetClanRatingResult> SetRating(int arg1, SetRatingRequest arg2)
        {
            if (!SecurityCheck(arg1, () => arg2.CallerId))
                return null;
            return _db.SetRating(arg2.ClanId, arg2.CallerId, arg2.CurrentRating, arg2.Rating);
        }

        private Task<bool> SetPlayerRating(int arg1, SetPlayerRatingRequest arg2)
        {
            if (!SecurityCheck(arg1, () => arg2.PlayerId))
                return null;
            return _db.SetPlayerRating(arg2.ClanId, arg2.PlayerId, arg2.CurrentRating, arg2.NewRating);
        }

        private Task<ActivateBoosterResult> ActivateBooster(int arg1, ActivateBoosterRequest arg2)
        {
            if (!SecurityCheck(arg1, () => arg2.CallerId))
                return null;
            return _db.ActivateBooster(arg2.ClanId, arg2.CallerId, arg2.BoosterId, arg2.TTL, arg2.Price);
        }

        private Task<bool> AddBooster(int arg1, AddBoosterRequest arg2)
        {
            if (!SecurityCheck(arg1, () => arg2.CallerId))
                return null;
            return _db.AddBooster(arg2.ClanId, arg2.CallerId, arg2.Booster);
        }

        private Task<bool> AddBoosters(int arg1, AddBoostersRequest arg2)
        {
            if (!SecurityCheck(arg1, () => arg2.CallerId))
                return null;
            return _db.AddBoosters(arg2.ClanId, arg2.CallerId, arg2.Boosters);
        }

        private Task<bool> Donate(int arg1, DonateRequest arg2)
        {
            if (!SecurityCheck(arg1, () => arg2.PlayerId))
                return null;
            return _db.Donate(arg2.ClanId, arg2.PlayerId, arg2.Amount, arg2.Reason);
        }

        private Task<SetClanDescriptionResult> SetClanDescription(int arg1, SetClanDescriptionRequest arg2)
        {
            if (!SecurityCheck(arg1, () => arg2.CallerId))
                return null;
            return _db.SetClanDescription(arg2.ClanId, arg2.CallerId, arg2.Name, arg2.Tag, arg2.Description,
                arg2.Emblem, arg2.JoinOpen, arg2.JoinAfterApprove, arg2.Requirements);
        }

        private Task<PeriodicClanDonationByPlayer> GetDonationsForPeriod(int arg1, GetDonationsForPeriodRequest arg2)
        {
            if (!SecurityCheck(arg1, () => arg2.CallerId))
                return null;
            return _db.GetDonationsForPeriod(arg2.ClanId, arg2.CallerId, arg2.Period);
        }

        private Task<GetClanConsumablesResult> GetClanConsumables(int arg1, GetClanConsumablesRequest arg2)
        {
            return _db.GetClanConsumables(arg2.ClanId);
        }

        private Task<GiveConsumableResult> GiveConsumable(int arg1, GiveConsumableRequest arg2)
        {
            if (!SecurityCheck(arg1, () => arg2.CallerId))
                return null;
            return _db.GiveConsumable(arg2.ClanId, arg2.CallerId, arg2.ConsumableId, arg2.Amount, arg2.Reason);
        }

        private Task<TakeConsumableResult> TakeConsumable(int arg1, TakeConsumableRequest arg2)
        {
            if (!SecurityCheck(arg1, () => arg2.CallerId))
                return null;
            return _db.TakeConsumable(arg2.ClanId, arg2.CallerId, arg2.ConsumableId, arg2.Amount, arg2.Reason);
        }

        private Task<ActivateBoosterResult> ActivateBoosterWithConsumables(int arg1, ActivateBoosterRequest arg2)
        {
            if (!SecurityCheck(arg1, () => arg2.CallerId))
                return null;
            return _db.ActivateBooster(arg2.ClanId, arg2.CallerId, arg2.BoosterId, arg2.TTL, arg2.Cost);
        }

        private Task<byte> Ping(int arg1, byte arg2)
        {
            return Task.FromResult((byte)0);
        }

        public void Dispose()
        {
        }

        [Dependency]
        private ITokenStore _tokenStore;
        [Dependency]
        private IClanDB _db;
        [Dependency]
        private IMessageSender _messageSender;
        [Dependency]
        private ClanServerUserBinder _userBinder;

        private readonly IMessageProvider _messageProvider;
        private ClansConfig _config;
        private IClanDefsLoader _clanDefsLoader;

        private static readonly ILog Log = LogManager.GetLogger(typeof(ClansServer));
        private Dispatcher _dispatcher;
        private MessageServer.Server.MessageServer _messageServer;
    }
}
