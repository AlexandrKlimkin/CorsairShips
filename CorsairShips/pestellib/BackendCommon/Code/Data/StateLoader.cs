using System;
using System.Collections.Generic;
using log4net;

namespace BackendCommon.Code.Data
{
    public class StateLoader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(StateLoader));
        private static readonly Lazy<UserStorage> _storage = new Lazy<UserStorage>(() => new UserStorage());
        public static UserStorage Storage => _storage.Value;

        public static byte[] LoadBytes(IGameDependent game, Guid? userId, string deviceUniqueId, int networkType, out Guid usedUserId, string facebookId = null)
        {
            if (facebookId != null)
            {
                userId = Storage.GetPlayerIdByFacebookId(facebookId);
            }

            if (userId == null || userId.Value == Guid.Empty)
            {
                userId = Storage.TryToGetUserIdByDeviceId(deviceUniqueId);
            }

            if (userId != null && userId.Value != Guid.Empty && Storage.UserExist(userId.Value))
            {
                var data = Storage.GetUserState(userId.Value);
                usedUserId = userId.Value;
                return data;
            }
            
            //TODO: add search in separate table by facebookId

            //user id not found, device id not found, creating new player
            return MakeNewProfile(game, userId, deviceUniqueId, networkType, out usedUserId, facebookId);
        }

        private static byte[] MakeNewProfile(IGameDependent game, Guid? userId, string deviceUniqueId, int networkType, out Guid usedUserId,
            string facebookId)
        {
//make new profile

            var id = (userId.HasValue && userId.Value != Guid.Empty) ? userId.Value : Guid.NewGuid();
            usedUserId = id;
            Storage.SaveDeviceId(deviceUniqueId, usedUserId);

            var defaultState = game.DefaultState(usedUserId);
            Storage.SaveRawState(id, defaultState);
            return defaultState;
        }
        
        public static void Save(Guid userId, byte[] data, string deviceId)
        {
            SetLastUsedDeviceId(userId, deviceId);
            Storage.SaveRawState(userId, data);
        }

        public static List<int> GetRandomUserIds(Guid playerId, int networkType, List<string> ignoreFacebookId, List<Guid> ignorePlayerId)
        {
            throw new NotImplementedException();
        }

        private static void SetLastUsedDeviceId(Guid playerId, string deviceId)
        {
            Storage.SetLastUsedDeviceId(playerId, deviceId);
        }

        public static string GetLastUsedDeviceId(Guid playerId)
        {
            return Storage.GetLastUsedDeviceId(playerId);
        }
    }
}