using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessagePack;

namespace ClansClientLib
{
    public interface IClanDB
    {
        Task<ClanRecord> GetClanByPlayer(Guid playerId);
        Task<ClanRecord> GetClan(Guid id);
        Task<GetClanConsumablesResult> GetClanConsumables(Guid clanId);
        // amount > 0
        Task<GiveConsumableResult> GiveConsumable(Guid clanId, Guid callerId, int consumableId, int amount, int reason);
        // amount > 0
        Task<TakeConsumableResult> TakeConsumable(Guid clanId, Guid callerId, int consumableId, int amount, int reason);
        Task<List<ClanRecord>> GetTopClans();
        Task<List<ClanRecord>> FindByNameOrTag(string searchText);
        /// <summary>
        /// Ищем кланы
        /// </summary>
        /// <param name="level">-1 если не ищем по уровню</param>
        /// <param name="requirements">данные об игроке, т.к. необходимо вернуть только те кланы в которые потенциально можно вступить</param>
        /// <returns></returns>
        Task<List<ClanRecord>> FindByParamsExact(int level, params ClanRequirement[] requirements);
        Task<List<ClanRecord>> FindByMaxParams(int maxLevel, ClanRequirement[] maxRequirements, bool includeClansWithoutRequirements, bool joinOpenOnly);
        Task<CreateClanResult> CreateClan(ClanDesc desc, Guid owner);
        Task<ClanJoinRequestResult> JoinClanRequest(Guid clanId, Guid playerId, TimeSpan requestTTL, int clanMembersLimit);
        Task<ClanRequestRecord[]> GetMyRequests(Guid playerId);
        Task<ClanRequestRecord[]> GetClanRequests(Guid clanId);
        Task<AcceptRejectClanRequestResult> AcceptClanRequest(Guid requestId, Guid callerId, string role, int clanMembersLimit);
        Task<AcceptRejectClanRequestResult> RejectClanRequest(Guid requestId, Guid callerId);
        Task<CancelClanRequestResult> CancelClanRequest(Guid requestId, Guid playerId);
        Task<CancelClanRequestResult[]> CancelClanRequests(Guid[] requestIds, Guid playerId);
        Task<TransferOwnershipResult> TransferOwnership(Guid clanId, Guid fromPlayerId, Guid toPlayerId);
        Task<LeaveClanResult> LeaveClan(Guid clanId, Guid playerId);
        Task<LeaveClanResult> KickClan(Guid clanId, Guid callerId, Guid playerIdToKick);
        Task<bool> SetLevel(Guid clanId, Guid callerId, int currLevel, int newLevel, int cost);
        Task<SetClanRatingResult> SetRating(Guid clanId, Guid callerId, int currRating, int newRating);
        Task<bool> SetPlayerRating(Guid clanId, Guid playerId, int currentRating, int newRating);
        Task<ActivateBoosterResult> ActivateBooster(Guid clanId, Guid callerId, Guid boosterId, TimeSpan ttl, int price);

        Task<ActivateBoosterResult> ActivateBooster(Guid clanId, Guid callerId, Guid boosterId, TimeSpan ttl, ClanCost cost);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clanId"></param>
        /// <param name="booster"></param>
        /// <returns>false если клан не существует</returns>
        Task<bool> AddBooster(Guid clanId, Guid callerId, ClanBooster booster);
        Task<bool> AddBoosters(Guid clanId, Guid callerId, ClanBooster[] boosters);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clanId"></param>
        /// <param name="playerId"></param>
        /// <param name="amount"></param>
        /// <returns>false если пользователь не в клане в который пытается задонатить</returns>
        Task<bool> Donate(Guid clanId, Guid playerId, int amount, int reason);
        Task<SetClanDescriptionResult> SetClanDescription(Guid clanId, Guid callerId, string name = null, string tag = null, string description = null, string emblem = null, bool? joinOpen = null, bool? joinAfterApprove = null, ClanRequirement[] requirements = null);
        Task<PeriodicClanDonationByPlayer> GetDonationsForPeriod(Guid clanId, Guid callerId, TimeSpan period);
    }

    [MessagePackObject()]
    public class ClanRequestRecordArray
    {
        [Key(0)]
        public ClanRequestRecord[] Array;
        
    }

    [MessagePackObject()]
    public class ClanRecordList
    {
        [Key(0)]
        public List<ClanRecord> Array2;
    }

    [MessagePackObject()]
    public class ClanDbSerializers
    {
        [Key(0)]
        public ActivateBoosterResult ActivateBoosterResult;
        [Key(1)]
        public SetClanRatingResult SetClanRatingResult;
        [Key(2)]
        public LeaveClanResult LeaveClanResult;
        [Key(3)]
        public TransferOwnershipResult TransferOwnershipResult;
        [Key(4)]
        public CancelClanRequestResult[] CancelClanRequestResultArray;
        [Key(5)]
        public CancelClanRequestResult CancelClanRequestResult;
        [Key(6)]
        public AcceptRejectClanRequestResult AcceptRejectClanRequestResult;
    }
}