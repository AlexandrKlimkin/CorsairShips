using FriendsServer.Bus;
using System;
using System.Collections.Generic;
using log4net;

namespace BackendCommon.Code.Data
{
    class UserStorageEventsListener
    {
        public void Register(FriendsBusBackendClient client)
        {
            if (_registred.Contains(client))
                return;
            Log.Debug("FriendsBusBackendClient registred.");
            _deleteListeners.Add(_ => client.NotifyPlayerRemoved(_));
        }

        public void PlayerDeleted(Guid playerId)
        {
            foreach (var dl in _deleteListeners)
            {
                dl(playerId);
            }
        }

        private HashSet<object> _registred = new HashSet<object>();
        private List<Action<Guid>> _deleteListeners = new List<Action<Guid>>();
        private static readonly ILog Log = LogManager.GetLogger(typeof(UserStorageEventsListener));
    }
}
