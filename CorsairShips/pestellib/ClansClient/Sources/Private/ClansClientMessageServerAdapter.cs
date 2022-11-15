using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using MessageClient;
using MessageServer.Sources;
using Newtonsoft.Json;

namespace ClansClientLib.Private
{
    public partial class ClansClientMessageServerAdapter : IClansClient ,IDisposable
    {
        private static ILog Log = LogManager.GetLogger(typeof(ClansClientMessageServerAdapter));
        public ClansClientMessageServerAdapter(IMessageSender messageSender, IMessageProviderEvents messageProvider)
        {
            _messageSender = messageSender;
            _messageProvider = messageProvider;

            _dispatcher = new Dispatcher(this);
            _messageClient = new MessageServer.Sources.MessageClient(messageProvider, _dispatcher);

            _messageProvider.OnConnected += OnConnected;
            _messageProvider.OnDisconnected += OnDisconnected;
        }

        private void OnDisconnected()
        {
            AvailabilityChange(false);
        }

        private void OnConnected()
        {
            AvailabilityChange(true);
        }

        private static readonly int Server = 1;

        private Task<ResponseT> SendRequest<ResponseT, RequestT>(ClanMessageType type, RequestT request)
        {
            return OutgoingMessageRequestHandler<ResponseT>.SendRequest(_messageSender, (int) type, request);
        }

        public Task<ClanRecord> GetClanByPlayer(Guid playerId)
        {
            var request = new GetClanByPlayerRequest()
            {
                PlayerId = playerId,
                Token = AuthToken
            };

            return SendRequest<ClanRecord, GetClanByPlayerRequest>(ClanMessageType.GetClanByPlayer, request);
        }

        public Task<ClanRecord> GetClan(Guid id)
        {
            var request = new GetClanRequest()
            {
                ClanId = id
            };

            return SendRequest<ClanRecord, GetClanRequest>(ClanMessageType.GetClan, request);
        }

        public Task<GetClanConsumablesResult> GetClanConsumables(Guid clanId)
        {
            var request = new GetClanConsumablesRequest()
            {
                ClanId = clanId
            };

            return SendRequest<GetClanConsumablesResult, GetClanConsumablesRequest>(ClanMessageType.GetClanConsumables, request);
        }

        public Task<GiveConsumableResult> GiveConsumable(Guid clanId, Guid callerId, int consumableId, int amount, int reason)
        {
            var request = new GiveConsumableRequest()
            {
                ClanId = clanId,
                CallerId = callerId,
                ConsumableId = consumableId,
                Amount = amount
            };

            return SendRequest<GiveConsumableResult, GiveConsumableRequest>(ClanMessageType.GiveConsumable, request);
        }

        public Task<TakeConsumableResult> TakeConsumable(Guid clanId, Guid callerId, int consumableId, int amount, int reason)
        {
            var request = new TakeConsumableRequest()
            {
                ClanId = clanId,
                CallerId = callerId,
                ConsumableId = consumableId,
                Amount = amount
            };

            return SendRequest<TakeConsumableResult, TakeConsumableRequest>(ClanMessageType.TakeConsumable, request);
        }

        public Task<List<ClanRecord>> GetTopClans()
        {
            var request = new GetTopClansRequest()
            {
            };
            return SendRequest<List<ClanRecord>, GetTopClansRequest>(ClanMessageType.GetTopClans, request);
        }

        public Task<List<ClanRecord>> FindByNameOrTag(string searchText)
        {
            var request = new FindByNameOrTagRequest()
            {
                SearchText = searchText
            };
            return SendRequest<List<ClanRecord>, FindByNameOrTagRequest>(ClanMessageType.FindByNameOrTag, request);
        }

        public Task<List<ClanRecord>> FindByParamsExact(int level, params ClanRequirement[] requirements)
        {
            var request = new FindByParamsExactRequest()
            {
                Level = level,
                Requirements = requirements
            };
            return SendRequest<List<ClanRecord>, FindByParamsExactRequest>(ClanMessageType.FindByParamsExact, request);
        }

        public Task<List<ClanRecord>> FindByMaxParams(int maxLevel, ClanRequirement[] maxRequirements, bool includeClansWithoutRequirements, bool joinOpenOnly)
        {
            var request = new FindByMaxParamsRequest()
            {
                MaxLevel = maxLevel,
                MaxRequirements = maxRequirements,
                IncludeClansWithoutRequirements = includeClansWithoutRequirements,
                JoinOpenOnly = joinOpenOnly
            };
            return SendRequest<List<ClanRecord>, FindByMaxParamsRequest>(ClanMessageType.FindByMaxParams, request);
        }

        public Task<CreateClanResult> CreateClan(ClanDesc desc, Guid owner)
        {
            var request = new CreateClanRequest()
            {
                Owner = owner,
                Description = desc
            };
            return SendRequest<CreateClanResult, CreateClanRequest>(ClanMessageType.CreateClan, request);
        }

        public Task<ClanJoinRequestResult> JoinClanRequest(Guid clanId, Guid playerId, TimeSpan requestTTL, int clanMembersLimit)
        {
            var request = new JoinClanRequestRequest()
            {
                ClanId = clanId,
                PlayerId = playerId,
                RequestTTL = requestTTL,
                ClanMembersLimit = clanMembersLimit
            };
            return SendRequest<ClanJoinRequestResult, JoinClanRequestRequest>(ClanMessageType.JoinClanRequest, request);
        }

        public Task<ClanRequestRecord[]> GetMyRequests(Guid playerId)
        {
            var request = new GetMyRequestsRequest()
            {
                PlayerId = playerId
            };
            return SendRequest<ClanRequestRecord[], GetMyRequestsRequest>(ClanMessageType.GetMyRequests, request);
        }

        public Task<ClanRequestRecord[]> GetClanRequests(Guid clanId)
        {
            var request = new GetClanRequestsRequest()
            {
                ClanId = clanId
            };
            return SendRequest<ClanRequestRecord[], GetClanRequestsRequest>(ClanMessageType.GetClanRequests, request);
        }

        public Task<AcceptRejectClanRequestResult> AcceptClanRequest(Guid requestId, Guid callerId, string role, int membersLimit)
        {
            var request = new AcceptClanRequestRequest()
            {
                RequestId = requestId,
                CallerId = callerId,
                Role = role,
                MembersLimit = membersLimit
            };
            return SendRequest<AcceptRejectClanRequestResult, AcceptClanRequestRequest>(ClanMessageType.AcceptClanRequest, request);
        }

        public Task<AcceptRejectClanRequestResult> RejectClanRequest(Guid requestId, Guid callerId)
        {
            var request = new RejectClanRequestRequest()
            {
                RequestId = requestId,
                CallerId = callerId
            };
            return SendRequest<AcceptRejectClanRequestResult, RejectClanRequestRequest>(ClanMessageType.RejectClanRequest, request);
        }

        public Task<CancelClanRequestResult> CancelClanRequest(Guid requestId, Guid playerId)
        {
            var request = new CancelClanRequestRequest()
            {
                RequestId = requestId,
                PlayerId = playerId
            };
            return SendRequest<CancelClanRequestResult, CancelClanRequestRequest>(ClanMessageType.CancelClanRequest, request);
        }

        public Task<CancelClanRequestResult[]> CancelClanRequests(Guid[] requestIds, Guid playerId)
        {
            var request = new CancelClanRequestsRequest()
            {
                RequestIds = requestIds,
                PlayerId = playerId
            };
            return SendRequest<CancelClanRequestResult[], CancelClanRequestsRequest>(ClanMessageType.CancelClanRequests, request);
        }

        public Task<TransferOwnershipResult> TransferOwnership(Guid clanId, Guid fromPlayerId, Guid toPlayerId)
        {
            var request = new TransferOwnershipRequest()
            {
                ClanId = clanId,
                FromPlayerId = fromPlayerId,
                ToPlayerId = toPlayerId
            };
            return SendRequest<TransferOwnershipResult, TransferOwnershipRequest>(ClanMessageType.TransferOwnership, request);
        }

        public Task<LeaveClanResult> LeaveClan(Guid clanId, Guid playerId)
        {
            var request = new LeaveClanRequest()
            {
                ClanId = clanId,
                PlayerId = playerId
            };
            return SendRequest<LeaveClanResult, LeaveClanRequest>(ClanMessageType.LeaveClan, request);
        }

        public Task<LeaveClanResult> KickClan(Guid clanId, Guid callerId, Guid playerIdToKick)
        {
            var request = new KickClanRequest()
            {
                ClanId = clanId,
                CallerId = callerId,
                PlayerIdToKick = playerIdToKick
            };
            return SendRequest<LeaveClanResult, KickClanRequest>(ClanMessageType.KickClan, request);
        }

        public Task<bool> SetLevel(Guid clanId, Guid callerId, int currLevel, int newLevel, int cost)
        {
            var request = new SetLevelRequest()
            {
                CallerId = callerId,
                CurrLevel = currLevel,
                NewLevel = newLevel,
                ClanId = clanId,
                Cost = cost
            };
            return SendRequest<bool, SetLevelRequest>(ClanMessageType.SetLevel, request);
        }

        public Task<SetClanRatingResult> SetRating(Guid clanId, Guid callerId, int currRating, int newRating)
        {
            var request = new SetRatingRequest()
            {
                CallerId = callerId,
                ClanId = clanId,
                Rating = newRating,
                CurrentRating = currRating
            };
            return SendRequest<SetClanRatingResult, SetRatingRequest>(ClanMessageType.SetRating, request);
        }

        public Task<bool> SetPlayerRating(Guid clanId, Guid playerId, int currentRating, int newRating)
        {
            var request = new SetPlayerRatingRequest()
            {
                ClanId = clanId,
                PlayerId = playerId,
                CurrentRating = currentRating,
                NewRating = newRating
            };
            return SendRequest<bool, SetPlayerRatingRequest>(ClanMessageType.SetPlayerRating, request);
        }

        public Task<ActivateBoosterResult> ActivateBooster(Guid clanId, Guid callerId, Guid boosterId, TimeSpan ttl, int price)
        {
            var request = new ActivateBoosterRequest()
            {
                ClanId = clanId,
                CallerId = callerId,
                BoosterId = boosterId,
                TTL = ttl,
                Price = price
            };
            return SendRequest<ActivateBoosterResult, ActivateBoosterRequest>(ClanMessageType.ActivateBooster, request);
        }

        public Task<ActivateBoosterResult> ActivateBooster(Guid clanId, Guid callerId, Guid boosterId, TimeSpan ttl, ClanCost cost)
        {
            var request = new ActivateBoosterRequest()
            {
                ClanId = clanId,
                CallerId = callerId,
                BoosterId = boosterId,
                TTL = ttl,
                Cost = cost
            };
            return SendRequest<ActivateBoosterResult, ActivateBoosterRequest>(ClanMessageType.ActivateBoosterWithConsumbles, request);
        }

        public Task<bool> AddBooster(Guid clanId, Guid callerId, ClanBooster booster)
        {
            var request = new AddBoosterRequest()
            {
                ClanId = clanId,
                CallerId = callerId,
                Booster = booster
            };
            return SendRequest<bool, AddBoosterRequest>(ClanMessageType.AddBooster, request);
        }

        public Task<bool> AddBoosters(Guid clanId, Guid callerId, ClanBooster[] boosters)
        {
            var request = new AddBoostersRequest()
            {
                ClanId = clanId,
                CallerId = callerId,
                Boosters = boosters
            };
            return SendRequest<bool, AddBoostersRequest>(ClanMessageType.AddBoosters, request);
        }

        public Task<bool> Donate(Guid clanId, Guid playerId, int amount, int reason)
        {
            var request = new DonateRequest()
            {
                ClanId = clanId,
                PlayerId = playerId,
                Amount = amount,
                Reason = reason
            };
            return SendRequest<bool, DonateRequest>(ClanMessageType.Donate, request);
        }

        public Task<SetClanDescriptionResult> SetClanDescription(Guid clanId, Guid callerId, string name = null, string tag = null, string description = null,
            string emblem = null, bool? joinOpen = null, bool? joinAfterApprove = null, ClanRequirement[] requirements = null)
        {
            var request = new SetClanDescriptionRequest()
            {
                ClanId = clanId,
                CallerId = callerId,
                Name = name,
                Tag = tag,
                Description = description,
                Emblem = emblem,
                JoinOpen = joinOpen,
                JoinAfterApprove = joinAfterApprove,
                Requirements = requirements
            };
            return SendRequest<SetClanDescriptionResult, SetClanDescriptionRequest>(ClanMessageType.SetClanDescription, request);
        }

        public async Task<ClanRequests> GetRequests(Guid clanId, Guid playerId, bool clanOwner)
        {
            ClanRequestRecord[] inRequests = new ClanRequestRecord[] { };
            var outRequests = await GetMyRequests(playerId);
            if(clanOwner)
                inRequests = await GetClanRequests(clanId);
            return new ClanRequests(inRequests.ToList(), outRequests.ToList());
        }

        public Task<PeriodicClanDonationByPlayer> GetDonationsForPeriod(Guid clanId, Guid callerId, TimeSpan period)
        {
            var request = new GetDonationsForPeriodRequest()
            {
                ClanId = clanId,
                CallerId = callerId,
                Period = period
            };
            return SendRequest<PeriodicClanDonationByPlayer, GetDonationsForPeriodRequest>(ClanMessageType.GetDonationsForPeriod, request);
        }

        private void AskUpdateRequestsMessage(byte data)
        {
            AskUpdateRequests();
        }

        private void AskUpdateBoostersMessage(byte data)
        {
            AskUpdateBoosters();
        }

        private void AskUpdateClanMessage(byte data)
        {
            AskUpdateClan();
        }

        private void AskUpdateConsumablesMessage(byte data)
        {
            AskUpdateConsumables();
        }

        public void Dispose()
        {
            _dispatcher?.Dispose();
        }

        public event Action<bool> AvailabilityChange = b => { };
        public bool Available => _messageSender.IsValid;

        public event Action AskUpdateRequests = () => { };
        public event Action AskUpdateBoosters = () => { };
        public event Action AskUpdateClan = () => { };
        public event Action AskUpdateConsumables = () => { };

        public Guid AuthToken { get; set; }

        private Dispatcher _dispatcher;
        private MessageServer.Sources.MessageClient _messageClient;
        private IMessageSender _messageSender;
        private readonly IMessageProviderEvents _messageProvider;
    }
}
