using System;
using System.Collections.Generic;

namespace BackendCommon.Code.Data
{
    public interface IUserStorage
    {
        long UsersCount { get; }

        IEnumerable<byte[]> AllUsers(int skip, int count, DateTime lastLogin = default(DateTime));
        void BindUserIdToName(Guid uid, string name, int maxSize);
        long CountRecentUsers();
        void Delete(Guid playerId);
        bool DeleteDeviceId(string deviceId);
        bool DeleteFacebookId(string facebookId);
        string GetLastUsedDeviceId(Guid playerId);
        Guid GetPlayerIdByFacebookId(string facebookId);
        DateTime GetProfileModDate(Guid userId);
        Guid[] GetRecentUsers(int count, int minStateSize = int.MaxValue);
        byte[] GetUserState(Guid playerId);
        void SaveDeviceId(string deviceId, Guid userId);
        void SaveRawState(Guid? userId, byte[] data);
        void SaveNameToUidBindings(string name, string currentBindings);
        void SetFacebookId(Guid userId, string facebookId);
        void SetLastUsedDeviceId(Guid playerId, string deviceId);
        Guid? TryToGetUserIdByDeviceId(string deviceId);
        void LockPlayer(Guid playerId, TimeSpan period);
        void UnlockPlayer(Guid playerId);
        bool UserExist(Guid playerId);
    }
}