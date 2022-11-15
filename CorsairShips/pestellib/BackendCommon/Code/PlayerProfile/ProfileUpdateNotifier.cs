using System;
using System.Collections.Concurrent;
using ServerShared.PlayerProfile;
using FriendsServer.Bus;
using log4net;

namespace ServerLib.PlayerProfile
{
    class ProfileUpdateNotifier
    {
        public void RegisterSource(IProfileStorage storage)
        {
            storage.OnProfileUpdated += OnProfileUpdated; 
        }

        public void RegisterDest(FriendsBusBackendClient dest)
        {
            _actions.Add((p) => dest.NotifyProfileChange(p));
        }

        private void OnProfileUpdated(ProfileDTO profile)
        {
            Log.Debug($"Profile updated. nick={profile.Nick}.");
            foreach (var a in _actions)
            {
                a(profile);
            }
        }

        private ConcurrentBag<Action<ProfileDTO>> _actions = new ConcurrentBag<Action<ProfileDTO>>();
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProfileUpdateNotifier));
    }
}
