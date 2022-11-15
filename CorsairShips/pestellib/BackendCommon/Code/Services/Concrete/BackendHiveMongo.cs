using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Backend.Code.Utils;
using BackendCommon.Code.Services.Concrete;
using log4net;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using PestelLib.ClientConfig;
using PestelLib.ServerCommon.Db;

namespace BackendCommon.Services.Concrete
{
#pragma warning disable 0649
    [BsonIgnoreExtraElements]
    class BackendService
    {
        [BsonId]
        public Uri MainHandlerUrl;

        public long SharedLogicCrc;
        public long SharedLogicVersion;
        public long DefinitionsVersion;
        public DateTime ApproxMaintenanceEnd;
        public DateTime MaintenanceStart;

        public bool Online;
        public bool PublicAccess = true;
        public bool InternalAccess = true;
    }
#pragma warning restore 0649

    class BackendHiveMongo : IBackendHivePrivate, IBackendHive, IDisposable
    {
        private static readonly TimeSpan CacheTime = TimeSpan.FromMinutes(1);
        private static ILog Log = LogManager.GetLogger(typeof(BackendHiveMongo));
        private readonly IMongoCollection<BackendService> _backendsCollection;
        private MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private Uri _selfUri;
        private object _sync = new object();

        public BackendHiveMongo(string dbUrl)
        {
            var url = new MongoUrl(dbUrl);
            var client = url.GetServer();
            var db = client.GetDatabase(url.DatabaseName);
            _backendsCollection = db.GetCollection<BackendService>("hive_backends");
            _backendsCollection.Indexes.CreateOne(
                Builders<BackendService>.IndexKeys.Combine(
                    Builders<BackendService>.IndexKeys.Ascending(_ => _.SharedLogicCrc),
                    Builders<BackendService>.IndexKeys.Ascending(_ => _.DefinitionsVersion))
                );
        }

        public async Task SetOnlineStatus(IBackendService service, bool online)
        {
            if (!(service is HttpBackendService httpService))
            {
                Log.Error($"{service.GetType()} not supported.");
                return;
            }

            var filter = Builders<BackendService>.Filter.Eq(_ => _.MainHandlerUrl, httpService.Url);
            var update = Builders<BackendService>.Update.Set(_ => _.Online, online);
            var updated = await _backendsCollection.FindOneAndUpdateAsync(filter, update);
            if (updated != null)
                Create(updated);
        }

        public IEnumerable<IBackendService> AllServices()
        {
            var filter = Builders<BackendService>.Filter.Empty;
            var cur = _backendsCollection.FindSync(filter);
            while (cur.MoveNext())
            {
                foreach (var service in cur.Current)
                {
                    yield return Create(service);
                }
            }
        }

        public void RegisterSharedConfig(Uri url, SharedConfig config)
        {
            var filter = Builders<BackendService>.Filter.Eq(_ => _.MainHandlerUrl, url);
            var update = Builders<BackendService>.Update.Combine(
                Builders<BackendService>.Update.Set(_ => _.SharedLogicCrc, config.SharedLogicCrc),
                Builders<BackendService>.Update.Set(_ => _.SharedLogicVersion, config.SharedLogicVersion),
                Builders<BackendService>.Update.Set(_ => _.DefinitionsVersion, config.DefinitionsVersion),
                Builders<BackendService>.Update.SetOnInsert(_ => _.PublicAccess, true),
                Builders<BackendService>.Update.SetOnInsert(_ => _.InternalAccess, true)
            );
            var opts = new FindOneAndUpdateOptions<BackendService>()
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };
            _selfUri = url;
            var item = _backendsCollection.FindOneAndUpdate(filter, update, opts);
            Create(item);
        }

        private void SetServiceParam<T>(IBackendService service, T val, Expression<Func<BackendService, T>> getter, UpdateDefinition<BackendService> extraUpdate)
        {
            if (!(service is HttpBackendService httpService))
            {
                Log.Error($"{service.GetType()} not supported.");
                return;
            }

            var filter = Builders<BackendService>.Filter.Eq(_ => _.MainHandlerUrl, httpService.Url);
            var update = Builders<BackendService>.Update.Set(getter, val);
            if (extraUpdate != null)
                update = Builders<BackendService>.Update.Combine(update, extraUpdate);
            var updated = _backendsCollection.FindOneAndUpdate(filter, update);
            if(updated != null)
                Create(updated);
        }

        public void SetMaintenance(IBackendService service, DateTime val)
        {
            SetServiceParam(service, val, _ => _.MaintenanceStart, null);
        }

        public void RemoveMaintenance(IBackendService service)
        {
            SetMaintenance(service, DateTime.MaxValue);
        }

        public void SetPublicAccess(IBackendService service, bool val)
        {
            SetServiceParam(service, val, _ => _.PublicAccess, null);
        }

        public void SetInternalAccess(IBackendService service, bool val)
        {
            SetServiceParam(service, val, _ => _.InternalAccess, null);
        }

        public IBackendService SelfService {
            get
            {
                var config = SharedConfigWatcher.Instance.Config;
                if (_selfUri == null)
                    return null;
                var key = _selfUri.ToString();
                if (_cache.TryGetValue(key, out var item))
                {
                    return item as IBackendService;
                }
                lock (_sync)
                {
                    if (_cache.TryGetValue(key, out item))
                        return item as IBackendService;
                    var sw = System.Diagnostics.Stopwatch.StartNew();

                    var self = GetByIdAndUpdate(_selfUri, config);
                    Log.Debug($"SelfService cache miss. time={sw.ElapsedMilliseconds}ms, slcrc ={config.SharedLogicCrc}, defs={config.DefinitionsVersion}.");
                    return self;
                }
            }
        }

        private IBackendService Create(BackendService item)
        {
            var service = new HttpBackendService(item.MainHandlerUrl, item.PublicAccess, item.InternalAccess, item.Online, item.MaintenanceStart);
            var key = item.MainHandlerUrl.ToString();
            _cache.Set(key, service, DateTimeOffset.UtcNow.Add(CacheTime));
            return service;
        }

        public IEnumerable<IBackendService> GetByVersion(uint slCrc, uint defVersion)
        {
            var filter = Builders<BackendService>.Filter.And(
                Builders<BackendService>.Filter.Eq(_ => _.SharedLogicCrc, slCrc),
                Builders<BackendService>.Filter.Eq(_ => _.DefinitionsVersion, defVersion)
            );
            var opts = new FindOptions<BackendService>();
            opts.Sort = Builders<BackendService>.Sort.Descending(_ => _.SharedLogicVersion);
            var cur = _backendsCollection.FindSync(filter, opts);
            return cur.ToList().Select(_ => Create(_));
        }

        private IBackendService GetByIdAndUpdate(Uri url, SharedConfig config)
        {
            var filter = Builders<BackendService>.Filter.Eq(_ => _.MainHandlerUrl, url);
            var update = Builders<BackendService>.Update.Combine(
                    Builders<BackendService>.Update.Set(_ => _.SharedLogicCrc, config.SharedLogicCrc),
                    Builders<BackendService>.Update.Set(_ => _.DefinitionsVersion, config.DefinitionsVersion)
                );
            var opts = new FindOneAndUpdateOptions<BackendService>();
            opts.ReturnDocument = ReturnDocument.After;
            var doc = _backendsCollection.FindOneAndUpdate(filter, update, opts);
            if (doc == null)
                return null;
            return Create(doc);
        }

        public void Dispose()
        {
            _cache?.Dispose();
        }
    }
}
