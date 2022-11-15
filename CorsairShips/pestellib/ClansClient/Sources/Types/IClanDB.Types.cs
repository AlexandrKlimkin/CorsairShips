using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using MessagePack;

namespace ClansClientLib
{
    public enum AcceptRejectClanRequestResult
    {
        Success,
        NoRequest,
        NotOwner,
        InvalidRequestState,
        Expired,
        Error,
        MembersLimit
    }

    public enum CancelClanRequestResult
    {
        Success,
        NoRequest,
        NotAuthorized,
        Error
    }

    public enum TransferOwnershipResult
    {
        Success,
        NoClan,
        NotClanMember,
        NotOwner,
        AlreadyOwner
    }

    public enum LeaveClanResult
    {
        Success,
        OwnerCantLeave,
        NotMember,
        NoClan,
        NotOwner,
        SelfKick,
        Error
    }

    public enum AddPlayerToClanResult
    {
        Success,
        NoClan,
        AlreadyInThisClan,
        AlreadyInOtherClan,
        MembersLimit
    }

    public enum ActivateBoosterResult
    {
        Success,
        NoClan,
        NoBooster,
        AlreadyActive,
        Expiried,
        NoMoney,
        NotOwner,
        InvalidPrice,
        Locked,
        Error
    }

    public enum SetClanRatingResult
    {
        Success,
        NoClan,
        NotMember,
        OldCurrentValue,
        Error
    }

    public enum ClanJoinRequestResultCode
    {
        Success,
        NoClan,
        NotEligible,
        AlreadyInOtherClan,
        AlreadyInThisClan,
        MembersLimit,
        JoinClosed,
        AlreadySent,
        Error
    }

    [MessagePackObject()]
    public struct ClanJoinRequestResult
    {
        [Key(0)]
        public ClanJoinRequestResultCode Code;
        [Key(1)]
        public ClanRequestRecord Request;
    }

    public enum SetTagCode
    {
        Success,
        NoClan,
        NoAllowed,
        AlreadySet,
        NotOwner,
        TagAlreadyTaken,
        BadWord
    }

    public enum SetNameTagCode
    {
        None,
        Success,
        AlreadyTaken,
        InvalidValue,
        NotOwner,
        NotAllowed,
        NoClan,
        Error
    }

    public enum SetClanDescriptionCode
    {
        None,
        Success,
        NotOwner,
        NoClan,
        Error
    }

    [MessagePackObject()]
    public struct SetClanDescriptionResult
    {
        [Key(0)]
        public SetNameTagCode SetNameResult;
        [Key(1)]
        public SetNameTagCode SetTagResult;
        [Key(2)]
        public SetClanDescriptionCode SetDescriptionResult;
        [Key(3)]
        public SetClanDescriptionCode SetEmblemResult;
        [Key(4)]
        public SetClanDescriptionCode SetJoinTypeResult;
        [Key(5)]
        public SetClanDescriptionCode SetRequirementsResult;

    }

    public static class SetClanDescriptionResultExtensions
    {
        public static bool HasChanges(this SetClanDescriptionResult result)
        {
            return result.SetDescriptionResult == SetClanDescriptionCode.Success
                   || result.SetEmblemResult == SetClanDescriptionCode.Success
                   || result.SetJoinTypeResult == SetClanDescriptionCode.Success
                   || result.SetNameResult == SetNameTagCode.Success
                   || result.SetRequirementsResult == SetClanDescriptionCode.Success
                   || result.SetTagResult == SetNameTagCode.Success;
        }
    }

    [MessagePackObject()]
    public class ClanPlayerConsumableDonation
    {
        [Key(0)]
        public int CurrencyType;
        [Key(1)]
        public int TotalDonation;
    }

    [MessagePackObject()]
    public class ClanPlayerTotalDonations
    {
        [Key(0)]
        public Guid PlayerId;
        [Key(1)]
        public int TotalDonation;
        [Key(2)]
        public ClanPlayerConsumableDonation[] CurrencyDonations;
    }

    [MessagePackObject()]
    public class PeriodicClanDonationByPlayer
    {
        [Key(0)]
        public TimeSpan Period;
        [Key(1)]
        public ClanPlayerTotalDonations[] DonationsByPlayer;
    }

    public enum AddConsumableCode
    {
        Success,
        NoClan,
        NotClanMember,
        InvalidAmount // в команде 0 или отрицательный amount
    }

    [MessagePackObject()]
    public class ClanCostInConsumable
    {
        [Key(0)]
        public int ConsumableId;
        [Key(1)]
        public int Amount;
    }

    [MessagePackObject()]
    public class ClanCost
    {
        [Key(0)]
        public int TreasuryCost;
        [Key(1)]
        public ClanCostInConsumable[] ConsumableCost;
    }

    [MessagePackObject()]
    public class GiveConsumableResult
    {
        [Key(0)]
        public AddConsumableCode Code;
        [Key(1)]
        public int AmountAfter;
    }

    public enum TakeConsumableCode
    {
        Success,
        NoClan,
        NotClanMember,
        NotOwner,
        InvalidAmount,
        NoMoney
    }

    [MessagePackObject()]
    public class TakeConsumableResult
    {
        [Key(0)]
        public TakeConsumableCode Code;
        [Key(1)]
        public int AmountAfter;
    }

    public enum GetClanConsumablesCode
    {
        Success,
        NoClan
    }

    [MessagePackObject()]
    public class GetClanConsumablesResult
    {
        [Key(0)]
        public GetClanConsumablesCode Code;
        [Key(1)]
        public ClanConsumables[] Consumables;
    }

}
