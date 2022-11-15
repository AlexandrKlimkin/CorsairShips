using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using S;

namespace FriendsServer.Db.Cache
{
    class CachedRoomInviteStorage : IRoomInviteStorage
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CachedRoomInviteStorage));
        private ConcurrentDictionary<string, RoomInvite> _storage = new ConcurrentDictionary<string, RoomInvite>();
        public Task<bool> Create(long roomId, MadId friend, MadId invitedBy, DateTime expiry)
        {
            var key = Key(roomId, friend);
            if (_storage.TryGetValue(key, out var invite))
            {
                if (invite.Expiry < DateTime.UtcNow)
                {
                    invite.Expiry = expiry;
                    if (!_storage.ContainsKey(key))
                        _storage[key] = invite;
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }

            _storage[key] = new RoomInvite() {RoomId = roomId, Invited = friend, Expiry = expiry, InvitedBy = invitedBy};
            return Task.FromResult(true);
        }

        public Task<bool> Remove(long roomId, MadId friend, out RoomInvite invite)
        {
            var key = Key(roomId, friend);
            if (!_storage.TryRemove(key, out invite))
                return Task.FromResult(false);

            return Task.FromResult(invite.Expiry > DateTime.UtcNow);
        }

        public Task<RoomInvite[]> GetRoomInvites(long roomId, bool includeExpired)
        {
            var result = _storage
                .Where(_ => _.Value.RoomId == roomId && (includeExpired || _.Value.Expiry > DateTime.UtcNow))
                .Select(_ => _.Value).ToArray();

            return Task.FromResult(result);
        }

        public long Count()
        {
            return _storage.Count;
        }

        public bool HasExpired()
        {
            foreach (var storageValue in _storage.Values)
            {
                if (storageValue.Expiry <= DateTime.UtcNow)
                    return true;
            }

            return false;
        }

        public List<RoomInvite> GetExpired()
        {
            var result = new List<RoomInvite>();

            foreach (var invite in _storage.Values)
            {
                if (invite.Expiry <= DateTime.UtcNow)
                    result.Add(invite);
            }

            return result;
        }

        private static string Key(long roomId, MadId friend)
        {
            return $"{roomId}_{friend}";
        }

        private void Dump()
        {
            foreach (var item in _storage.Values)
            {
                Log.Debug($"RoomId={item.RoomId} Expiry={item.Expiry} Invited={item.Invited} InvitedBy={item.InvitedBy} dt={DateTime.UtcNow}");
            }
        }
    }
}
