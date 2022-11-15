using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Backend.Code.Sync;
using BackendCommon.Code.Data.PersistentStorage;
using log4net;
using PestelLib.ServerCommon.Db;
using PestelLib.ServerCommon.Extensions;
using ServerLib;
using UnityDI;

namespace BackendCommon.Code.Data
{
    public class UserStorage : IUserStorage
    {
        public const string DeviceIdToUserId = "DeviceIdToUserId";
        public const string UserStates = "UserStates";
        public const string UserStatesTimestamps = "UserStatesTimestamps";
        public const string FacebookIdsKeys = "facebookIds";
        public const string NameToId = "NameToId";
        public const string LastUsedDeviceIdKey = "LastUsedDeviceId";

        private static readonly ILog Log = LogManager.GetLogger(typeof(UserStorage));
        private static readonly UserLock userLock = new UserLock();

        private IPersistentStorage _deviceIdToUser;
        private IPersistentStorage _userStates;
        private IPersistentStorage _facebookIdKeys;
        private IPersistentStorage _nameBindings;
        private IPersistentStorage _lastUsedDeviceIds;
        private UserStorageEventsListener _userStorageEventsListener;

        private PersistentStoragePool _persistentStoragePool = new PersistentStoragePool();
        private IUserMaintenancePrivate _userMaintenancePrivate;
        public long UsersCount => _userStates.Count();

        public UserStorage()
        {
            _userMaintenancePrivate = ContainerHolder.Container.Resolve<IUserMaintenancePrivate>();
            _deviceIdToUser = _persistentStoragePool.GetStorage(AppSettings.Default.PersistentStorageSettings.StorageConnectionString, DeviceIdToUserId);
            _userStates = _persistentStoragePool.GetStorage(AppSettings.Default.PersistentStorageSettings.StorageConnectionString, UserStates);
            _facebookIdKeys = _persistentStoragePool.GetStorage(AppSettings.Default.PersistentStorageSettings.StorageConnectionString, FacebookIdsKeys);
            _nameBindings = _persistentStoragePool.GetStorage(AppSettings.Default.PersistentStorageSettings.StorageConnectionString, NameToId);
            _lastUsedDeviceIds = _persistentStoragePool.GetStorage(AppSettings.Default.PersistentStorageSettings.StorageConnectionString, LastUsedDeviceIdKey);
            _userStorageEventsListener = ContainerHolder.Container.Resolve<UserStorageEventsListener>();
        }

        public Guid GetPlayerIdByFacebookId(string facebookId)
        {
            var friendGameIdPersistent = _facebookIdKeys?.LoadRawData(facebookId);
            if (friendGameIdPersistent != null)
            {
                return new Guid(friendGameIdPersistent);
            }

            return Guid.Empty;
        }

        public long CountRecentUsers()
        {
            var period = AppSettings.Default.PersistentStorageSettings.PersistUserTimeout;
            var count = _userStates?.CountBySaveTime(DateTime.UtcNow - period) ?? 0;
            return count;
        }

        public Guid[] GetRecentUsers(int count, int minStateSize = int.MaxValue)
        {
            var result = new List<Guid>(count);
            if (_userStates != null)
            {
                var maxAge = AppSettings.Default.PersistentStorageSettings.PersistUserTimeout;
                var keys = _userStates.GetKeys(maxAge, minStateSize, count);
                result.AddRange(keys.Select(Guid.Parse));
            }

            return result.ToArray();
        }

        public Guid? TryToGetUserIdByDeviceId(string deviceId)
        {
            var resultPersistent = _deviceIdToUser?.LoadRawData(deviceId);
            if (resultPersistent != null)
            {
                return new Guid(resultPersistent);
            }

            return null;
        }

        public bool UserExist(Guid playerId)
        {
            return _userStates?.Exists(playerId.ToString()) ?? false;
        }

        public byte[] GetUserState(Guid playerId)
        {
            var k = playerId.ToString();

            var data = _userStates?.LoadRawData(k);
            if (data == null)
            {
                return null;
            }

            return data;
        }

        // USE IT WISELY
        public IEnumerable<byte[]> AllUsers(int skip, int count, DateTime lastLogin = default(DateTime))
        {
            foreach (var key in _userStates.GetKeys(skip, count, lastLogin))
            {
                yield return GetUserState(new Guid(key));
            }
        }

        public void LockPlayer(Guid playerId, TimeSpan period)
        {
            _userMaintenancePrivate.SetUserMaintenance(playerId, DateTime.UtcNow + period);
        }

        public void UnlockPlayer(Guid playerId)
        {
            _userMaintenancePrivate.Remove(playerId);
        }

        public void SaveDeviceId(string deviceId, Guid userId)
        {
            _deviceIdToUser?.SaveRawDataAsync(deviceId, userId.ToByteArray());
        }

        public bool DeleteDeviceId(string deviceId)
        {
            return _deviceIdToUser?.Remove(deviceId) ?? false;
        }

        public void SetFacebookId(Guid userId, string facebookId)
        {
            _facebookIdKeys?.SaveRawDataAsync(facebookId, userId.ToByteArray());
        }

        public bool DeleteFacebookId(string facebookId)
        {
            return _facebookIdKeys?.Remove(facebookId) ?? false;
        }

        public void Delete(Guid playerId)
        {
            var k = playerId.ToString();

            _userStates?
                .RemoveAsync(k)
                .ReportOnFail()
                .ResultToCallback(b => 
                {
                    Log.Debug($"{k} removed from persistent {b}");
                    if (b)
                        _userStorageEventsListener?.PlayerDeleted(playerId);
                });
        }

        public void SaveRawState(Guid? userId, byte[] data)
        {
            if (_userStates == null)
                throw new UserStorageException(UserStorageException.ErrorEnum.STORAGE_NOT_AVAILABLE, userId);

            if (!_userStates.SaveRawData(userId.Value.ToString(), data))
                throw new UserStorageException(UserStorageException.ErrorEnum.SAVE_RAW_DATA_FAILED, userId);
        }

        private string GetString(IPersistentStorage collection, string key)
        {
            var nameBytes = collection?.LoadRawData(key);
            if (nameBytes != null && nameBytes.Length > 0)
            {
                return Encoding.UTF8.GetString(nameBytes);
            }

            return null;
        }

        private void SetString(IPersistentStorage collection, string key, string val)
        {
            collection.SaveRawData(key, Encoding.UTF8.GetBytes(val));
        }

        public void BindUserIdToName(Guid uid, string name, int maxSize)
        {
            if (string.IsNullOrEmpty(name))
                return;
            lock (userLock.GetStringLockObject(name))
            {
                var val = GetString(_nameBindings, name);
                var uidS = uid.ToString();

                string currentBindings = val ?? string.Empty;

                if (currentBindings.Length + uidS.Length + 1 > maxSize)
                {
                    return;
                }
                if (!string.IsNullOrEmpty(currentBindings))
                {
                    if(currentBindings.Contains(uidS))
                        return;
                    currentBindings += " ";
                }

                currentBindings += uidS;

                SaveNameToUidBindings(name, currentBindings);
            }
        }

        public void SaveNameToUidBindings(string name, string currentBindings)
        {
            SetString(_nameBindings, name, currentBindings);
        }

        public DateTime GetProfileModDate(Guid userId)
        {
            return _userStates.GetTime(userId.ToString());
        }

        public void SetLastUsedDeviceId(Guid playerId, string deviceId)
        {
            var playerIdStr = playerId.ToString();

            SetString(_lastUsedDeviceIds, playerIdStr, deviceId);
        }

        public string GetLastUsedDeviceId(Guid playerId)
        { 
            var resultPersistent = GetString(_lastUsedDeviceIds, playerId.ToString());
            if (resultPersistent != null)
            {
                return resultPersistent;
            }

            return null;
        }
    }
}
