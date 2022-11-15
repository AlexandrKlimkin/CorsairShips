using System;
using MessagePack;

namespace ClansClientLib
{
    public enum ClanMessageType
    {
        GetClanByPlayer = 50,
        GetClan,
        GetTopClans,
        FindByNameOrTag,
        FindByParamsExact,
        FindByMaxParams,
        CreateClan,
        JoinClanRequest,
        GetMyRequests,
        GetClanRequests,
        AcceptClanRequest,
        RejectClanRequest,
        CancelClanRequest,
        CancelClanRequests,
        TransferOwnership,
        LeaveClan,
        KickClan,
        SetLevel,
        SetRating,
        SetPlayerRating,
        ActivateBooster,
        AddBooster,
        AddBoosters,
        Donate,
        SetClanDescription,
        GetDonationsForPeriod,

        // server
        AskUpdateRequests,
        AskUpdateBoosters,
        AskUpdateClan,
        AskUpdateConsumables,

        GetClanConsumables,
        GiveConsumable,
        TakeConsumable,
        ActivateBoosterWithConsumbles
    }

    [MessagePackObject()]
    public class GetClanByPlayerRequest
    {
        [Key(0)]
        public Guid PlayerId;
        [Key(1)]
        public Guid Token;
    }

    [MessagePackObject()]
    public class GetClanRequest
    {
        [Key(0)]
        public Guid ClanId;
    }

    [MessagePackObject()]
    public class GetTopClansRequest
    {
    }

    [MessagePackObject()]
    public class FindByNameOrTagRequest
    {
        [Key(0)]
        public string SearchText;
    }

    [MessagePackObject()]
    public class FindByParamsExactRequest
    {
        [Key(0)]
        public int Level;

        [Key(1)] 
        public ClanRequirement[] Requirements;
    }

    [MessagePackObject()]
    public class FindByMaxParamsRequest
    {
        [Key(0)]
        public int MaxLevel;
        [Key(1)]
        public ClanRequirement[] MaxRequirements;
        [Key(3)]
        public bool IncludeClansWithoutRequirements;
        [Key(4)]
        public bool JoinOpenOnly;
    }

    [MessagePackObject()]
    public class CreateClanRequest
    {
        [Key(0)]
        public ClanDesc Description;
        [Key(1)]
        public Guid Owner;
    }

    [MessagePackObject()]
    public class JoinClanRequestRequest
    {
        [Key(0)]
        public Guid ClanId;
        [Key(1)]
        public Guid PlayerId;
        [Key(2)]
        public TimeSpan RequestTTL;
        [Key(3)]
        public int ClanMembersLimit;
    }

    [MessagePackObject()]
    public class GetMyRequestsRequest
    {
        [Key(0)]
        public Guid PlayerId;
    }

    [MessagePackObject()]
    public class GetClanRequestsRequest
    {
        [Key(0)]
        public Guid ClanId;
    }

    [MessagePackObject()]
    public class AcceptClanRequestRequest
    {
        [Key(0)]
        public Guid RequestId;
        [Key(1)]
        public Guid CallerId;
        [Key(2)]
        public string Role;
        [Key(3)]
        public int MembersLimit;
    }

    [MessagePackObject()]
    public class RejectClanRequestRequest
    {
        [Key(0)]
        public Guid RequestId;
        [Key(1)]
        public Guid CallerId;
    }

    [MessagePackObject()]
    public class CancelClanRequestRequest
    {
        [Key(0)]
        public Guid RequestId;
        [Key(1)]
        public Guid PlayerId;
    }

    [MessagePackObject()]
    public class CancelClanRequestsRequest
    {
        [Key(0)]
        public Guid[] RequestIds;
        [Key(1)]
        public Guid PlayerId;
    }

    [MessagePackObject()]
    public class TransferOwnershipRequest
    {
        [Key(0)]
        public Guid ClanId;
        [Key(1)]
        public Guid FromPlayerId;
        [Key(2)]
        public Guid ToPlayerId;
    }

    [MessagePackObject()]
    public class LeaveClanRequest
    {
        [Key(0)]
        public Guid ClanId;
        [Key(1)]
        public Guid PlayerId;
    }

    [MessagePackObject()]
    public class KickClanRequest
    {
        [Key(0)]
        public Guid ClanId;
        [Key(1)]
        public Guid CallerId;
        [Key(2)]
        public Guid PlayerIdToKick;
    }

    [MessagePackObject()]
    public class SetLevelRequest
    {
        [Key(0)]
        public Guid ClanId;
        [Key(1)]
        public int CurrLevel;
        [Key(2)]
        public int NewLevel;
        [Key(3)]
        public int Cost;
        [Key(4)]
        public Guid CallerId;
    }

    [MessagePackObject()]
    public class SetRatingRequest
    {
        [Key(0)]
        public Guid ClanId;
        [Key(1)]
        public int Rating;
        [Key(2)]
        public Guid CallerId;
        [Key(3)]
        public int CurrentRating;
    }

    [MessagePackObject()]
    public class SetPlayerRatingRequest
    {
        [Key(0)]
        public Guid ClanId;
        [Key(1)]
        public Guid PlayerId;
        [Key(2)]
        public int CurrentRating;
        [Key(3)]
        public int NewRating;
    }

    [MessagePackObject()]
    public class ActivateBoosterRequest
    {
        [Key(0)]
        public Guid ClanId;
        [Key(1)]
        public Guid CallerId;
        [Key(2)]
        public Guid BoosterId;
        [Key(3)]
        public TimeSpan TTL;
        [Key(4)]
        public int Price;
        [Key(5)]
        public ClanCost Cost;
    }

    [MessagePackObject()]
    public class AddBoosterRequest
    {
        [Key(0)]
        public Guid ClanId;
        [Key(1)]
        public ClanBooster Booster;
        [Key(2)]
        public Guid CallerId;
    }

    [MessagePackObject()]
    public class AddBoostersRequest
    {
        [Key(0)]
        public Guid ClanId;
        [Key(1)]
        public ClanBooster[] Boosters;
        [Key(2)]
        public Guid CallerId;
    }

    [MessagePackObject()]
    public class DonateRequest
    {
        [Key(0)]
        public Guid ClanId;
        [Key(1)]
        public Guid PlayerId;
        [Key(2)]
        public int Amount;
        [Key(3)]
        public int Reason;
    }

    [MessagePackObject()]
    public class SetClanDescriptionRequest
    {
        [Key(0)]
        public Guid ClanId;
        [Key(1)]
        public Guid CallerId;
        [Key(2)]
        public string Name;
        [Key(3)]
        public string Tag;
        [Key(4)]
        public string Description;
        [Key(5)]
        public string Emblem;
        [Key(6)]
        public bool? JoinOpen;
        [Key(7)]
        public bool? JoinAfterApprove;
        [Key(8)]
        public ClanRequirement[] Requirements;
    }

    [MessagePackObject()]
    public class GetDonationsForPeriodRequest
    {
        [Key(0)]
        public Guid ClanId;
        [Key(1)]
        public TimeSpan Period;
        [Key(2)]
        public Guid CallerId;
    }

    [MessagePackObject()]
    public class GetClanConsumablesRequest
    {
        [Key(0)]
        public Guid ClanId;
    }

    [MessagePackObject()]
    public class GiveConsumableRequest
    {
        [Key(0)]
        public Guid ClanId;
        [Key(1)]
        public Guid CallerId;
        [Key(2)]
        public int ConsumableId;
        [Key(3)]
        public int Amount;
        [Key(4)]
        public int Reason;
    }

    [MessagePackObject()]
    public class TakeConsumableRequest
    {
        [Key(0)]
        public Guid ClanId;
        [Key(1)]
        public Guid CallerId;
        [Key(2)]
        public int ConsumableId;
        [Key(3)]
        public int Amount;
        [Key(4)]
        public int Reason;
    }
}
