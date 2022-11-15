using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FriendsClient.Lobby;
using log4net;
using S;

namespace FriendsServer.Db.Cache
{
    class CachedRoomStorage : IRoomStorage
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CachedRoomStorage));
        private long _id;
        ConcurrentDictionary<long, Room> _rooms = new ConcurrentDictionary<long, Room>();
        ConcurrentDictionary<uint, long> _madIdToRoomId = new ConcurrentDictionary<uint, long>();

        public CachedRoomStorage()
        {
        }

        public Task<Room> Get(long roomId)
        {
            _rooms.TryGetValue(roomId, out var room);
            return Task.FromResult(room);
        }

        public Task<Room[]> GetExpired()
        {
            return Task.FromResult(_rooms.Values.Where(_ => _.BattleStart <= DateTime.UtcNow && _.RoomStatus == RoomStatus.Party).ToArray());
        }

        public Task<Room> Get(MadId id, bool hostOnly)
        {
            long roomId = 0;
            if (hostOnly && !_madIdToRoomId.TryGetValue(id, out roomId))
                return Task.FromResult((Room) null);
            if(roomId != 0)
                return Get(roomId);

            var result = _rooms.Values.FirstOrDefault(_ => _.Party.Contains(id));
            return Task.FromResult(result);
        }

        public Task<Room> Create(MadId host, TimeSpan autoStart, string gameData)
        {
            var id = Interlocked.Increment(ref _id);
            var room = new Room()
            {
                Id = id,
                RoomStatus = RoomStatus.Party,
                GameSpecificData = gameData,
                BattleStart = DateTime.UtcNow + autoStart,
                AutoStartDelay = autoStart,
                Party = new List<MadId>() {host},
                Host = host,
            };
            _rooms[id] = room;
            _madIdToRoomId[host] = id;

            return Task.FromResult(room);
        }

        public async Task<Room> ChangeHost(long roomId, MadId newHost)
        {
            var room = await Get(roomId);
            if (room == null) return null;
            lock (room)
            {
                room.BattleStart = DateTime.UtcNow + room.AutoStartDelay;
                room.Host = newHost;
            }
            return room;
        }

        public async Task<bool> StartBattle(long roomId, string gameData)
        {
            var room = await Get(roomId);
            if (room == null) return false;
            lock (room)
            {
                room.RoomStatus = RoomStatus.Battle;
                room.GameSpecificData = gameData;
            }
            return true;
        }

        public async Task<bool> JoinRoom(long roomId, MadId friend)
        {
            var room = await Get(roomId);
            if (room == null) return false;
            lock (room)
            {
                if (room.Party.Contains(friend))
                {
                    Log.Error($"{friend} already joined {roomId}.");
                    return false;
                }
                room.Party.Add(friend);
            }
            return true;
        }

        public async Task<bool> LeaveRoom(long roomId, MadId friend)
        {
            var room = await Get(roomId);
            if (room == null) return false;
            lock (room)
            {
                if (room.Host == friend)
                {
                    room.Host = MadId.Zero;
                    _madIdToRoomId.TryRemove(friend, out var r);
                }

                return room.Party.Remove(friend);
            }
        }

        public async Task<bool> Remove(long roomId)
        {
            var room = await Get(roomId);
            if (room == null) return false;
            lock (room)
            {
                _madIdToRoomId.TryRemove(room.Host, out var r);
                _rooms.TryRemove(roomId, out var rm);
            }
            return true;
        }

        public async Task<Room> Update(long roomId, string data, RoomStatus status)
        {
            var room = await Get(roomId);
            if (room == null) return null;
            lock (room)
            {
                room.GameSpecificData = data;
                room.RoomStatus = status;
            }

            return room;
        }

        public Task<Room> GetCloseToExpireRoom()
        {
            return Task.FromResult(_rooms.Values.OrderByDescending(_ => _.BattleStart).FirstOrDefault());
        }

        public Task<long> Count()
        {
            return Task.FromResult((long)_rooms.Count);
        }
    }
}
