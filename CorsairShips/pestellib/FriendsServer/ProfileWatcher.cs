using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using FriendsClient;
using FriendsServer.Bus;
using FriendsServer.Db;
using log4net;
using S;
using ServerShared.PlayerProfile;

namespace FriendsServer
{
    class ProfileWatcher : IDisposable
    {
        private const int BatchSize = 10;
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProfileWatcher));
        private readonly IProfileStorage _profileStorage;
        private readonly Dictionary<uint,ProfileWatchItem> _watchItems = new Dictionary<uint, ProfileWatchItem>();
        private readonly List<ProfileWatchItem> _updateList = new List<ProfileWatchItem>(BatchSize);
        private readonly Dictionary<Guid, ProfileDTO> _updatesFromBus = new Dictionary<Guid, ProfileDTO>();
        private ServerConfig _config;
        private FriendsBus _bus;

        public ProfileWatcher(IProfileStorage profileStorage, ServerConfig config)
        {
            _profileStorage = profileStorage;
            _config = config;
            _bus = FriendsBus.Instance;
            if(_bus.Enabled)
                _bus.OnProfileUpdate += BusOnProfileUpdate;
        }

        private void BusOnProfileUpdate(ProfileDTO profileDto)
        {
            Log.Debug("BusOnProfileUpdate " + profileDto.Nick);
            lock (_updatesFromBus)
            {
                _updatesFromBus[profileDto.PlayerId] = profileDto;
            }
        }

        public void WatchProfile(FriendBase profile)
        {
            lock (_watchItems)
            {
                _watchItems[profile.Id] = new ProfileWatchItem()
                {
                    FriendBase = profile,
                    LastCheck = DateTime.UtcNow
                };
                Log.Debug($"Watched profiles: {_watchItems.Count}.");
            }
        }

        public void UnwatchProfile(MadId id)
        {
            lock (_watchItems)
            {
                _watchItems.Remove(id);
                Log.Debug($"Watched profiles: {_watchItems.Count}.");
            }
        }

        public async Task<List<FriendBase>> Update()
        {
            if (_bus.Enabled)
                return await UpdateFromBus();
            return await UpdateFromStorage();
        }

        public Task<List<FriendBase>> UpdateFromBus()
        {
            var result = new List<FriendBase>();
            lock (_updatesFromBus)
            {
                if (_updatesFromBus.Count < 1)
                    return Task.FromResult(result);

                lock (_watchItems)
                {
                    foreach (var item in _watchItems)
                    {
                        if(!_updatesFromBus.TryGetValue(item.Value.FriendBase.Profile.PlayerId, out var profile))
                            continue;

                        item.Value.FriendBase.Profile = profile;
                        Log.Debug($"Profile changed. Id={item.Value.FriendBase.Id}.");
                        result.Add(item.Value.FriendBase);
                    }
                }
                _updatesFromBus.Clear();
            }

            return Task.FromResult(result);
        }

        public async Task<List<FriendBase>> UpdateFromStorage()
        {
            _updateList.Clear();
            lock (_watchItems)
            {
                foreach (var item in _watchItems)
                {
                    if (DateTime.UtcNow - item.Value.LastCheck < _config.ProfileUpdateFrequency)
                        continue;
                    _updateList.Add(item.Value);
                    if (_updateList.Count == BatchSize)
                        break;
                }
            }

            if (_updateList.Count == 0)
                return null;
            var profiles = await _profileStorage.GetMany(_updateList.Select(_ => _.FriendBase.Profile.PlayerId).ToArray());
            var updatedProfilesMap = profiles.ToDictionary(_ => _.PlayerId);
            var result = new List<FriendBase>();
            foreach (var profile in _updateList)
            {
                profile.LastCheck = DateTime.UtcNow;
                if (!updatedProfilesMap.TryGetValue(profile.FriendBase.Profile.PlayerId, out var updatedProfile))
                    continue;
                if (updatedProfile.UpdateTime <= profile.FriendBase.Profile.UpdateTime)
                    continue;
                Log.Debug($"Profile changes detected. Id={profile.FriendBase.Id}.");
                result.Add(profile.FriendBase);
                profile.FriendBase.Profile = updatedProfile;
            }

            return result;
        }

        class ProfileWatchItem
        {
            public FriendBase FriendBase;
            public DateTime LastCheck;
        }

        public void Dispose()
        {
            if (_bus.Enabled)
                _bus.OnProfileUpdate -= BusOnProfileUpdate;
        }
    }
}
