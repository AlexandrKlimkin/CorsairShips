using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ServerLib
{
    public class PersistentStorageSettings
    {
#if DEPLOY_IOS
        public string StorageConnectionString = "mongodb://localhost/database_ios?connectTimeoutMS=3000&socketTimeoutMS=3000&serverSelectionTimeoutMS=3000";
#else
        public string StorageConnectionString = "mongodb://localhost/database_android?connectTimeoutMS=3000&socketTimeoutMS=3000&serverSelectionTimeoutMS=3000";
#endif

        public string BroadcastMessageCollectionName = "broadcast_messages";
        public string BroadcastMessagePlayerChacheName = "broadcast_messages_player_cache";
        public int PersistUserTimeoutSeconds = 120;

        [JsonIgnore]
        public TimeSpan PersistUserTimeout => new TimeSpan(0, 0, 0, PersistUserTimeoutSeconds);
    }

    public class GlobalConflictSettings
    {
#if DEPLOY_IOS
        public string ConnectionString = "mongodb://localhost/global_conflict_ios?connectTimeoutMS=3000&socketTimeoutMS=3000&serverSelectionTimeoutMS=3000";
#else
        public string ConnectionString = "mongodb://localhost/global_conflict_android?connectTimeoutMS=3000&socketTimeoutMS=3000&serverSelectionTimeoutMS=3000";
#endif
        public TimeSpan UpdateCooldown = TimeSpan.FromSeconds(1); // минимальная задержка между соседними пересчетами стейта конфликта (в апи есть способ инициировать апдейт сколько угодно раз, мы не хотим лишать апи такой возможности но и не хотим давать инструмент для инзлишнего повышения загрузки ЦПУ)
        public bool EnableProfiler = true;
        public bool DebugEnabled = false;
        // время которое запросы живут в памяти для быстрых ответов на идентичные запросы
        public TimeSpan LeaderboardCacheTTL = TimeSpan.FromSeconds(5);
        public TimeSpan NameCacheTTL = TimeSpan.FromMinutes(1);
        // имя сборки должно соответствовать имени файла
        public string PoiNodePicker = "BackendCommon.Code.GlobalConflict.Server.Stages.DefaultPointOfInterestNodePicker, BackendCommon";
    }

    public class PlayerProfileSettings
    {
        public bool PlayerProfileActivated = false;
        public bool EnableProfiler = true;
        public TimeSpan PutExpire = TimeSpan.Zero;
        public string ConcreteProfileGenerator = "PestelLib.SharedLogic.Extensions.SubmarinesPlayerProfileProvider, PestelLib.ConcreteSharedLogic";
    }

    public class GeoIpService
    {
        public string ConnectionString = "mongodb://localhost";
        public string DbName = "geo";
    }

    public class AppSettings
    {
        public static AppSettings LoadConfig(string configPath)
        {
            var config = File.ReadAllText(configPath);
            var configInstance = new AppSettings();
            JsonConvert.PopulateObject(config, configInstance);
            Default = configInstance;
            return configInstance;
        }

        public static AppSettings Default;

        public bool UseTableStorage = true;
        public bool CheckVersion = true;
        public bool SearchByDeviceId = true;
        public string SupportEmail = "fencingsup2016@gmail.com";
        public string StorageConnectionString = "UseDevelopmentStorage=true;";
        public string RedisConnectionString = "localhost,abortConnect=false,ssl=false";
        public bool FakePayments = true;
        public bool Leaderboard10 = true;
        public bool Leaderboard20 = true;
        public string MatchmakerVersion = "";
        public bool GloabalConflict = false;
        public string GraphiteHost = "pestelcrew.com";
        public int GraphitePort = 2003;
        public string ServerId = "";
        public bool SkipMissingDefs = false;
        public bool LeagueDebug = false;
        public bool SharedLogicEnabled = true;
        public bool SharedLogicDebug = false;
        public int CacheNameSizeLimit = 369; // 10 guid records per name
        public bool OfflineSharedLogicEnabled = false;
        public string MachineNameForWipeDB = "";
        public int LeagueJobPeriod = 30; // seconds
        public TimeSpan MatchInfoTTL = TimeSpan.MaxValue;
        public bool ProfileQuartzJobs = false;
        public bool ProfileMainHandler = false;
        public string ChatDbConnectionString = "mongodb://localhost/chat";
        public string MessageQueueConnectionString = "rabbitmq,localhost";
        public string ClansMessageQueueAppId = "ClansInternalServer";
        public string AndroidInAppValidatorCredFileOverride = ""; // если пустая строка то ищет конфиг в редисе, как раньше. если указан файл то открывает его.

        public bool UseJsonSerializationForSharedLogic = false;

        public bool MinimizeRedisUsage = false;

        /*
        public bool UseTableStorage => _configuration.GetValue<bool>("UseTableStorage");
        public bool CheckVersion => _configuration.GetValue<bool>("CheckVersion");
        public bool SearchByDeviceId => _configuration.GetValue<bool>("SearchByDeviceId");
        public string SupportEmail => _configuration.GetValue<string>("SupportEmail");
        public string StorageConnectionString => _configuration.GetValue<string>("StorageConnectionString");
        public string RedisConnectionString => _configuration.GetValue<string>("RedisConnectionString");
        public string Leaderboards => _configuration.GetValue<string>("Leaderboards");
        public string SharedLogicLib => _configuration.GetValue<string>("SharedLogicLib");
        public bool FakePayments => _configuration.GetValue<bool>("FakePayments", false);
        public bool Leaderboard10 => _configuration.GetValue("Leaderboard10", true);
        public bool Leaderboard20 => _configuration.GetValue("Leaderboard20", true);
        */

        public PersistentStorageSettings PersistentStorageSettings = new PersistentStorageSettings();
        public GlobalConflictSettings GlobalConflictSettings = new GlobalConflictSettings();
        public PlayerProfileSettings PlayerProfileSettings = new PlayerProfileSettings();
        public GeoIpService GeoIpService = new GeoIpService();
    }
}
