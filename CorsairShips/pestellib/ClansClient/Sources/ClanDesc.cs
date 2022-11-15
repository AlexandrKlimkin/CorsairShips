using System;
using System.Collections.Generic;
using MessagePack;

namespace ClansClientLib
{
    public class Donations7Days
    {
        public string PlayerId;
        public string ClanId;
        public int Amount;
        public DateTime Date;
    }

    [MessagePackObject()]
    public class ClanPlayer
    {
        [Key(0)]
        public Guid Id;
        [Key(1)]
        public int Rating;
        [Key(2)]
        public int Donated;
        [Key(3)]
        public string Role;
    }

    [MessagePackObject()]
    public class ClanBooster
    {
        [Key(0)]
        public Guid Id;
        [Key(1)]
        public string DefId;
        [Key(2)]
        public bool Activated; // проверяй дополнительно ExpiryTime, Activated && ExpiryTime > SharedTime.Now 
        [Key(3)]
        public DateTime ActivateTime;
        [Key(4)]
        public DateTime ExpiryTime;
    }

    public struct ClanState
    {
        public string Id;
        public List<ClanPlayer> Members;
        public ClanDesc Desc;
        public int Level;
        public int Rating;
        public int TreasuryCurrent;
        public int TreasuryTotal;
        public List<ClanBooster> Boosters;
    }

    [MessagePackObject()]
    public class ClanRequirement
    {
        [Key(0)]
        public string Name;
        [Key(1)]
        public int Value;
    }

    [MessagePackObject]
    public class ClanDesc
    {
        [Key(0)]
        public string Name;
        [Key(1)]
        public string Tag;
        [Key(2)]
        public string Description;
        [Key(3)]
        public string Emblem;
        [Key(4)]
        public bool JoinOpen;
        [Key(5)]
        public bool JoinCheckEligibility;
        [Key(6)]
        public bool JoinAfterApprove;
        [Key(7)]
        public List<ClanRequirement> Requirements;
    }

    public enum JoinRequestState
    {
        None,
        Pending,
        Approved,
        Rejected,
        Removed,
        Expiried
    }

    public struct JoinRequest
    {
        public string Id;
        public JoinRequestState Result;
    }

    public class ClanRequests
    {
        public IReadOnlyList<ClanRequestRecord> IncomingRequests => _incomingRequests;
        public IReadOnlyList<ClanRequestRecord> OutgoingRequests => _outgoingRequests;

        public ClanRequests(List<ClanRequestRecord> incoming, List<ClanRequestRecord> outgoing)
        {
            _incomingRequests = incoming;
            _outgoingRequests = outgoing;
        }

        private List<ClanRequestRecord> _incomingRequests;
        private List<ClanRequestRecord> _outgoingRequests;
    }

    public enum CreateClanResultCode
    {
        Success,
        NameAlreadyTaken,
        BadWordName,
        BadWordTag,
        TagAlreadyTaken,
        PlayerAlreadyInClan,
        NoMoney,
        TagIsEmpty,
        NameIsEmpty,
        BadWordDescription
    }

    [MessagePackObject()]
    public struct CreateClanResult
    {
        [Key(0)]
        public CreateClanResultCode Code;
        [Key(1)]
        public Guid Id;
    }
}
