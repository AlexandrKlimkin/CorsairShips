using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FriendsClient.FriendList;
using FriendsClient.Lobby;
using S;
using ServerShared.PlayerProfile;

namespace FriendsServer.Db
{
    class Friend
    {
        public MadId Id;
        public Guid PlayerId;
        public int Status;
        public MadId[] Friends;
        public DateTime LastStatus;
    }

    class Room
    {
        public long Id;
        public MadId Host;
        public List<MadId> Party;
        public string GameSpecificData;
        public RoomStatus RoomStatus;
        public DateTime BattleStart;
        public TimeSpan AutoStartDelay;
    }

    interface IRoomStorage
    {
        Task<Room> Get(long roomId);
        Task<Room[]> GetExpired();
        Task<Room> Get(MadId id, bool hostOnly);
        Task<Room> Create(MadId host, TimeSpan autoStart, string gameData);
        Task<Room> ChangeHost(long roomId, MadId newHost);
        Task<bool> StartBattle(long roomId, string gameData);
        Task<bool> JoinRoom(long roomId, MadId friend);
        Task<bool> LeaveRoom(long roomId, MadId friend);
        Task<bool> Remove(long roomId);
        Task<Room> Update(long room, string data, RoomStatus status);
        Task<Room> GetCloseToExpireRoom();
        Task<long> Count();
    }

    class RoomInvite
    {
        public long RoomId;
        public MadId Invited;
        public MadId InvitedBy;
        public DateTime Expiry;
    }

    interface IRoomInviteStorage
    {
        Task<bool> Create(long roomId, MadId friend, MadId invitedBy, DateTime expiry);
        Task<bool> Remove(long roomId, MadId friend, out RoomInvite invite);
        Task<RoomInvite[]> GetRoomInvites(long roomId, bool includeExpired);
        long Count();
        bool HasExpired();
        List<RoomInvite> GetExpired();
    }

    interface INonFriendsStatusWatch
    {
        Task SetWatch(Guid watcher, Guid[] observables);
        Task<Guid[]> GetWatchersOfObservable(Guid observable);
    }

    interface IFriendStorage
    {
        Task<Friend> GetOrCreate(Guid playerId);
        Task<Friend> Get(MadId id);
        Task<Friend[]> GetMany(params Guid[] playerIds);
        Task<Friend[]> GetMany(params MadId[] ids);
        Task<bool> ChangeStatus(MadId id, int status);
        Task<bool> CheckChangeStatus(MadId id, int fromStatus, int finalStatus);
        Task<bool> AddFriend(MadId friendList, MadId friend);
        Task<bool> RemoveFriend(MadId friendList, MadId friend);
        Task<bool> IsFriends(MadId first, MadId second);
        Task<bool> Remove(MadId id);
    }

    class Invitation
    {
        public long Id;
        public MadId From;
        public MadId To;
        public InviteStatus Status;
        public DateTime Expiry;
    }

    interface IInvitationStorage
    {
        Task<Invitation> Create(MadId from, MadId to, TimeSpan ttl);
        Task<Invitation> Get(long inviteId);
        Task<Invitation[]> GetIncoming(MadId id);
        Task<Invitation[]> GetOutgoing(MadId id);
        Task<long> CountIncoming(MadId id);
        Task<Invitation> HasInvite(MadId f, MadId t);
        Task<Invitation> GetCloseToExpirInvitation();
        Task<Invitation[]> GetExpired(int amount);
        Task<long> CleanExpiried();
        Task<bool> Remove(long id);
        Task<long> RemoveMany(params long[] ids);
        Task<long> Count();
    }

    interface IProfileStorage
    {
        Task<ProfileDTO> Get(Guid playerId);
        Task<ProfileDTO[]> GetMany(params Guid[] playerId);
        Task<bool> IsBot(Guid playerId);
    }

#pragma warning disable 649
    class Gift
    {
        public long Id;
        public MadId From;
        public MadId To;
        public int GameSpecificId;
        public DateTime CreateTime;
        public DateTime ClaimTime;
        public bool IsClaimed;
    }
#pragma warning restore 649

    interface IGiftStorage
    {
        Task<Gift> Create(MadId from, MadId to, int giftId);
        /// <summary>
        /// Get unclaimed in/out gifts.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Gift[]> Get(MadId id);
        Task<Gift[]> GetAll(MadId id);
        Task<Gift> GetLastGift(MadId from, MadId to);
        Task<long> CountGifts(MadId from, MadId to, bool includeClaimed);
        Task<Gift> ClaimGift(long giftId, MadId clamingId);
        Task<List<Gift>> GetById(params long[] giftIds);
    }
}
