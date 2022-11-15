using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using ServerLib;
using log4net;
using PestelLib.ServerCommon.Db;

namespace BackendCommon.Code.Auth
{
    [BsonIgnoreExtraElements]
    class BanRecord
    {
        [BsonId]
        public Guid PlayerId;
        public bool Banned;
        public string Reason;
        public DateTime Time = DateTime.UtcNow;
    }

    // Забыл [BsonIgnoreExtraElements] для BanRecord поэтому создаю новую коллекцию
    [BsonIgnoreExtraElements]
    class BanDeviceRecord
    {
        [BsonId]
        public Guid PlayerId;
        public string[] Devices;
    }

    public class MongoGameBanStorage : IGameBanStorage
    {
        private IMongoCollection<BanRecord> _banCollection;
        private IMongoCollection<BanDeviceRecord> _banDeviceCollection;
        private MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private static readonly ILog Log = LogManager.GetLogger(typeof(MongoGameBanStorage));

        public MongoGameBanStorage()
        {
            var url = new MongoUrl(AppSettings.Default.PersistentStorageSettings.StorageConnectionString);
            var c = url.GetServer();
            var db = c.GetDatabase(url.DatabaseName);
            _banCollection = db.GetCollection<BanRecord>("game_bans");
            _banDeviceCollection = db.GetCollection<BanDeviceRecord>("game_device_bans");
            _banDeviceCollection.Indexes.CreateOne(Builders<BanDeviceRecord>.IndexKeys.Ascending(_ => _.Devices));
        }

        private bool IsDeviceBanned(Guid userId, string deviceId, out string reason)
        {
            reason = null;
            if (string.IsNullOrEmpty(deviceId) || userId == Guid.Empty)
                return false;
            var filter = Builders<BanDeviceRecord>.Filter.AnyEq(_ => _.Devices, deviceId);
            var records = _banDeviceCollection.FindSync(filter).ToList();
            if (records.Count() < 1)
                return false;
            var cur = _banCollection.FindSync(Builders<BanRecord>.Filter.And(
                Builders<BanRecord>.Filter.Eq(_ => _.Banned, true),
                Builders<BanRecord>.Filter.In(_ => _.PlayerId, records.Select(_ => _.PlayerId))));
            var banItem = cur.FirstOrDefault();
            if (banItem == null)
                return false;
            Log.Warn($"Banned user changed his player id from {banItem.PlayerId} to {userId}. deviceId={deviceId}. {userId} will be banned.");
            Ban(userId, banItem.Reason);
            banItem.PlayerId = userId;
            SaveDeviceId(banItem, deviceId);
            reason = banItem.Reason;
            return true;
        }

        private void SaveDeviceId(BanRecord banRecord, string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                return;
            var filter = Builders<BanDeviceRecord>.Filter.Eq(_ => _.PlayerId, banRecord.PlayerId);
            var result = _banDeviceCollection.FindSync(filter).SingleOrDefault();
            if (result == null)
            {
                result = new BanDeviceRecord()
                {
                    PlayerId = banRecord.PlayerId,
                    Devices = new string[] { deviceId }
                };
                _banDeviceCollection.InsertOne(result);
                Log.Debug($"Banned player {banRecord.PlayerId} device {deviceId}.");
            }
            else
            {
                if (result.Devices.Contains(deviceId))
                    return;
                var update = Builders<BanDeviceRecord>.Update.Push(_ => _.Devices, deviceId);
                result = _banDeviceCollection.FindOneAndUpdate(filter, update);
                if (result != null)
                {
                    Log.Debug($"Banned player {banRecord.PlayerId} logs in with new device {deviceId}.");
                }
            }
        }

        public bool IsBanned(Guid userId, string deviceId, out string reason)
        {
            if (userId == Guid.Empty)
            {
                reason = "";
                return false;
            }

            var key = GetKey(userId);
            var banItem = _cache.Get(key) as BanRecord;
            if (banItem != null)
            {
                reason = banItem.Reason;
                if(banItem.Banned)
                    SaveDeviceId(banItem, deviceId);
                return banItem.Banned;
            }

            var filter = Builders<BanRecord>.Filter.Eq(_ => _.PlayerId, userId);
            var r = _banCollection.FindSync(filter);
            banItem = r.SingleOrDefault();
            if (banItem == null)
            {
                if (IsDeviceBanned(userId, deviceId, out reason))
                {
                    return true;
                }
                banItem = new BanRecord()
                {
                    Banned = false,
                    PlayerId = userId,
                };
            }
            if (banItem.Banned)
                SaveDeviceId(banItem, deviceId);

            _cache.Set(key, banItem, TimeSpan.FromMinutes(30));
            reason = banItem.Reason;
            return banItem.Banned;
        }

        public bool Unban(Guid userId)
        {
            var filter = Builders<BanRecord>.Filter.Eq(_ => _.PlayerId, userId);
            var update = Builders<BanRecord>.Update.Set(_ => _.Banned, false);
            var r = _banCollection.FindOneAndUpdate(filter, update);
            _cache.Remove(GetKey(userId));
            return r != null;
        }

        public IEnumerable<GameBanStorageItem> ListBans()
        {
            var filter = Builders<BanRecord>.Filter.Eq(_ => _.Banned, true);
            var r = _banCollection.FindSync(filter);
            while (r.MoveNext())
            {
                foreach (var record in r.Current)
                {
                    yield return new GameBanStorageItem {
                        PlayerId = record.PlayerId,
                        Reason = record.Reason
                    };
                }
            }
        }

        public bool Ban(Guid userId, string reason)
        {
            if (userId == Guid.Empty)
            {
                Log.Error($"Cant ban UserId={userId}. reason={reason}.");
                return false;
            }

            var key = GetKey(userId);
            var br = new BanRecord()
            {
                PlayerId = userId,
                Reason = reason,
                Banned = true,
            };
            _cache.Set(key, br, DateTimeOffset.UtcNow.AddMinutes(30));
            var filter = Builders<BanRecord>.Filter.Eq(_ => _.PlayerId, userId);
            if (_banCollection.Count(filter) > 0)
            {
                Log.Debug($"Ban update for {userId}.");
                var r = _banCollection.FindOneAndReplace(filter, br);
                return r != null;
            }
            else
            {
                Log.Debug($"New ban for {userId}.");
                _banCollection.InsertOne(br);
                return true;
            }
        }

        private static string GetKey(Guid playerId)
        {
            return $"ban:{playerId}";
        }
    }
}