using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Backend.Code.Modules.PlayerProfile;
using Backend.Code.PlayerProfile;
using Backend.Code.Reflection;
using Backend.Code.Statistics;
using Backend.Code.Utils;
using BackendCommon.Code;
using BackendCommon.Code.Auth;
using BackendCommon.Code.GeoIP;
using BackendCommon.Code.GlobalConflict;
using BackendCommon.Code.GlobalConflict.Db.Mongo;
using BackendCommon.Code.GlobalConflict.Server;
using BackendCommon.Code.IapValidator;
using BackendCommon.Code.Jobs;
using BackendCommon.Code.Leagues;
using BackendCommon.Code.Modules;
using BackendCommon.Code.Modules.ClassicLeaderboards;
using BackendCommon.Code.Modules.GlobalConflict;
using BackendCommon.Services;
using BackendCommon.Services.Concrete;
using GoogleSpreadsheet;
using log4net;
using MongoDB.Driver;
using PestelLib.ChatServer;
using PestelLib.ChatServer.Mongo;
using PestelLib.ServerCommon;
using PestelLib.ServerCommon.Config;
using PestelLib.ServerCommon.Db;
using PestelLib.ServerCommon.Db.Auth;
using PestelLib.ServerCommon.Messaging;
using PestelLib.ServerCommon.Redis;
using PestelLib.ServerCommon.Utils;
using PestelLib.ServerProtocol;
using PestelLib.ServerShared;
using PestelLib.SharedLogicBase;
using PestelLib.UniversalSerializer;
using ReportPlayers;
using Server;
using ServerExtension;
using ServerLib;
using ServerLib.Code.PromoValidator;
using ServerLib.Modules;
using ServerLib.Modules.Messages;
using ServerLib.PlayerProfile;
using ServerShared;
using ServerShared.GlobalConflict;
using ServerShared.Leagues;
using SharedLogicBase.Server.PlayerProfile;
using ShortPlayerId;
using ShortPlayerId.Storage;
using StackExchange.Redis;
using UnityDI;
using System.Diagnostics;
using PestelLib.ServerCommon.Extensions;
using BackendCommon.Code.Data;
using ClansClientLib;

namespace BackendCommon
{
    public static class BackendInitializer
    {
        public static ILeagueDefProvider LeagueDefProvider;
        private static IapValidatorHub _iapValidator;
        private static ServiceContainer _serviceContainer = new ServiceContainer();
        private static readonly ILog log = LogManager.GetLogger(typeof(BackendInitializer));
        public static bool Initialized { get; private set; }

        public static IServiceProvider ServiceProvider => _serviceContainer;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static bool Init()
        {
            if(Initialized)
                return false;

            Log.Init();

            InitializeAppSettings();
            InitQuartz();
            WipeDatabaseOnDeveloperMachine();
            if (AppSettings.Default != null && string.IsNullOrEmpty(AppSettings.Default.ServerId))
            {
                var parts = RuntimeSettings.AppRoot.Split(new[] {"\\", "/"}, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                    AppSettings.Default.ServerId = parts.Last();
                else
                    AppSettings.Default.ServerId = "_";

                if (AppSettings.Default.UseJsonSerializationForSharedLogic)
                {
                    Serializer.SetTextMode();
                }

                SharedLogicSettings.IsDebug = AppSettings.Default.SharedLogicDebug;
            }

            MakeConcreteGame();

            MainHandlerBase.ServiceProvider = _serviceContainer;

            DynamicLoader.LoadServerExtensions();
            PrepareFeatures();
            Migrate();

            var scheduler = BackgroundProcesses.OnAppStart(AppSettings.Default);

            return Initialized = true;
        }

        private static void InitQuartz()
        {
            BackgroundProcesses.Configure(_serviceContainer);
            var quartzDb = DefaultDbFactory<IQuartzConfig>.Create(AppSettings.Default.PersistentStorageSettings.StorageConnectionString);
            ContainerHolder.Container.RegisterInstance(quartzDb);

            //migrate from redis
            var sw = Stopwatch.StartNew();
            var migrator = new PestelLib.ServerCommon.Db.Mongo.Temp.QuartzMigrateRedis(ContainerHolder.Container.Resolve<IQuartzConfig>(), false);
            migrator.Migrate();
            log.Debug($"QuartzMigrateRedis took {sw.ElapsedMilliseconds}ms.");
        }

        // временный код, удалить когда все проекты слезут с редиса
        private static void Migrate()
        {
            var container = ContainerHolder.Container;
            var sw = Stopwatch.StartNew();
            {
                log.Debug($"MaintenanceMigrateRedis start");
                var maintenanceMigrator = new PestelLib.ServerCommon.Db.Mongo.Temp.MaintenanceMigrateRedis(container.Resolve<IUserMaintenancePrivate>(), false);
                maintenanceMigrator.Migrate();
                log.Debug($"MaintenanceMigrateRedis took {sw.ElapsedMilliseconds}ms.");
            }

            {
                log.Debug($"PromoMigrateRedis start");
                sw.Restart();
                var promoMigrator = new PestelLib.ServerCommon.Db.Mongo.Temp.PromoMigrateRedis(_serviceContainer.GetService(typeof(IPromoStorage)) as IPromoStorage, false);
                promoMigrator.Migrate();
                log.Debug($"PromoMigrateRedis took {sw.ElapsedMilliseconds}ms.");
            }

            {
                log.Debug($"ServerMessageMigrateRedis start");
                var migrator = new PestelLib.ServerCommon.Db.Mongo.Temp.ServerMessageMigrateRedis(_serviceContainer.GetService(typeof(IServerMessageStorage)) as IServerMessageStorage, false);
                var migratorSw = Stopwatch.StartNew();
                migrator.Migrate().ReportOnFail().ContinueWith(_ => log.Debug($"ServerMessageMigrateRedis took {migratorSw.ElapsedMilliseconds}."));
            }

            {
                log.Debug($"FeedbackMigrateRedis start");
                sw.Restart();
                var migrator = new PestelLib.ServerCommon.Db.Mongo.Temp.FeedbackMigrateRedis(_serviceContainer.GetService(typeof(IFeedbackStorage)) as IFeedbackStorage, false);
                migrator.Migrate();
                log.Debug($"FeedbackMigrateRedis took {sw.ElapsedMilliseconds}.");
            }
        }

        private static void WipeDatabaseOnDeveloperMachine()
        {
#if DEBUG
            if (string.IsNullOrEmpty(AppSettings.Default.MachineNameForWipeDB)) return;

            if (AppSettings.Default.MachineNameForWipeDB == Environment.MachineName)
            {
                WipeRedis();
                WipeMongo();
            }

            void WipeMongo()
            {
                var url = new MongoUrl(AppSettings.Default.PersistentStorageSettings.StorageConnectionString);
                var client = url.GetServer();
                client.Client.DropDatabase(url.DatabaseName);
            }

            void WipeRedis()
            {
                using (var connection = GetConnectionMultiplexerAdmin())
                {
                    var ep = RedisUtils.Connection.GetEndPoints()[0];
                    var server = connection.GetServer(ep);
                    server.FlushAllDatabases();
                }
            }

            ConnectionMultiplexer GetConnectionMultiplexerAdmin()
            {
                var options = ConfigurationOptions.Parse(AppSettings.Default.RedisConnectionString);
                options.AllowAdmin = true;
                return ConnectionMultiplexer.Connect(options);
            }
#endif
        }

        private static void InitializeAppSettings()
        {
            string configPath = AppDomain.CurrentDomain.BaseDirectory + "App_Data" + Path.DirectorySeparatorChar + "appsettings.json";
            AppSettings.LoadConfig(configPath);
            ContainerHolder.Container.OnErrorLog += m => log.Error(m);
            ContainerHolder.Container.OnInjectedException += m => log.Error(m);
        }

        private static void PrepareFeatures()
        {
            var _featuresCollection = MainHandlerBase.FeaturesCollection = new FeaturesCollection();
            var container = ContainerHolder.Container;
            var userStorageEventsListener = new UserStorageEventsListener();
            container.RegisterCustomSingleton<IReportsStorage>(() => new MongoReportsStorage(AppSettings.Default.PersistentStorageSettings.StorageConnectionString));
            container.RegisterCustomSingleton(() => new ReportPlayerModule(container.Resolve<IReportsStorage>()));
            container.RegisterInstance(userStorageEventsListener);

            container.RegisterCustomSingleton(() =>
            {
                var serverExtensions = new ServerExtensions();
                var extensions = ServerReflectionUtils.GetAllInterfaceImplementations(typeof(IExtension));
                foreach (var extensionType in extensions)
                {
                    serverExtensions.RegisterExtension((IExtension)container.Resolve(extensionType));
                }
                return serverExtensions;
            });

            container.RegisterCustomSingleton(() => new ShortPlayerIdStorage(AppSettings.Default.PersistentStorageSettings.StorageConnectionString));
            container.RegisterCustomSingleton(() => new ShortPlayerIdModule(container.Resolve<ShortPlayerIdStorage>()));
            container.RegisterCustomSingleton(() =>
            {
                var serverExtensions = new ServerExtensionsAsync();
                var extensions = ServerReflectionUtils.GetAllInterfaceImplementations(typeof(IAsyncExtension));
                foreach (var extensionType in extensions)
                {
                    log.Info("Tying to load extension " + extensionType);
                    var extension = container.Resolve(extensionType);
                    if (extension == null)
                    {
                        log.Error("Can't load server extension: " + extensionType);
                        continue;
                    }
                    serverExtensions.RegisterExtension((IAsyncExtension)extension);
                }
                return serverExtensions;
            });
            container.Resolve<ServerExtensionsAsync>();

            var playerIpTable = DefaultDbFactory<IPlayerIpResolver>.Create(AppSettings.Default.PersistentStorageSettings.StorageConnectionString);
            var playerIdToVersion = new PlayerIdToVersion();
            ContainerHolder.Container.RegisterInstance(playerIpTable);
            ContainerHolder.Container.RegisterInstance(playerIdToVersion);
            ContainerHolder.Container.RegisterInstance<IPlayerIpResolver>(playerIpTable);

            _serviceContainer.AddService(typeof(IGameBanStorage), new MongoGameBanStorage());
            try
            {
                var banStorage = new BanStorageMongo(new MongoUrl(AppSettings.Default.ChatDbConnectionString));
                _serviceContainer.AddService(typeof(IBanRequestStorage), banStorage);
            }
            catch(Exception e)
            {
                log.Error(e);
            }



            //var _iapValidator = new IapValidatorRouteHandler(AppSettings.Default.FakePayments, true);
            if (LeagueDefProvider != null)
            {
                var config = LeagueLeaderboardConfigCache.Get();
                var leagueDefs = new LeagueDefHelper(LeagueDefProvider);
                var leagueState = new LeagueStateCache();
                ILeagueStorage storage = new LeagueStorageMongo(new MongoUrl(AppSettings.Default.PersistentStorageSettings.StorageConnectionString), "Leagues_", config, leagueState, leagueDefs);
                if (storage == null)
                    throw null;

                var _leagueServer = new LeagueServer(storage, config, leagueState, leagueDefs);
                _serviceContainer.AddService(typeof(ILeagueStorage), storage);
                _featuresCollection.AddFeature(new Feature<ILeagueServerModuleApi>(_leagueServer));
                _serviceContainer.AddService(typeof(LeagueServer), _leagueServer);
                _serviceContainer.AddService(typeof(LeagueStateCache), leagueState);
                ContainerHolder.Container.RegisterInstance(_leagueServer);
            }

            var _classicLeaderboards = DefaultDbFactory<ClassicLeaderboards.ILeaderboards>.Create(AppSettings.Default.PersistentStorageSettings.StorageConnectionString);
            var promoStorage = DefaultDbFactory<IPromoStorage>.Create(AppSettings.Default.PersistentStorageSettings.StorageConnectionString);
            _serviceContainer.AddService(typeof(IPromoStorage), promoStorage);
            _iapValidator = new IapValidatorHub(AppSettings.Default.FakePayments);
            _featuresCollection.AddFeature(new Feature<IIapValidator>(_iapValidator));
            _featuresCollection.AddFeature(new Feature<IPromoValidator>(new PromoValidator(promoStorage)));
            _featuresCollection.AddFeature(new Feature<ClassicLeaderboards.ILeaderboards>(_classicLeaderboards));
            _serviceContainer.AddService(typeof(IIapValidator), _iapValidator);
            ContainerHolder.Container.RegisterCustom(ApiFactory.GetMatchInfoApi);
            ContainerHolder.Container.RegisterInstance<ClassicLeaderboards.ILeaderboards>(_classicLeaderboards);

            var classicLeaderboardConfig = SimpleJsonConfigLoader.LoadConfig<LeaderboardConfig>();
            ContainerHolder.Container.RegisterInstance(classicLeaderboardConfig);

            var gc = new GraphiteClient(AppSettings.Default.GraphiteHost, AppSettings.Default.GraphitePort);
            var statsClient = new DefaultStatisticsClient(gc);
            ContainerHolder.Container.RegisterInstance(statsClient);
            ContainerHolder.Container.RegisterSingleton<RedisLockManager>();
            ContainerHolder.Container.RegisterCustom<ILockManager>(() => ContainerHolder.Container.Resolve<RedisLockManager>());
            _serviceContainer.AddService(typeof(DefaultStatisticsClient), statsClient);

            var geoService = GeoIPFactory.Create();
            if (geoService != null)
            {
                ContainerHolder.Container.RegisterInstance(geoService);
            }

            if (AppSettings.Default.GloabalConflict)
            {
                var nodePickerInstance = DynamicLoader.CreateInstance<IPointOfInterestNodePicker>(AppSettings.Default.GlobalConflictSettings.PoiNodePicker);
                ContainerHolder.Container.RegisterInstance(nodePickerInstance);

                GlobalConflictMongoInitializer.Init();
                var globalConflict = new GlobalConflictServer();
                ContainerHolder.Container.RegisterCustom<GlobalConflictApi>(() => globalConflict);
                ContainerHolder.Container.RegisterCustom<GlobalConflictPrivateApi>(() => globalConflict);
                ContainerHolder.Container.RegisterSingleton<GlobalConflictApiCallHandler>();
            }

            if (AppSettings.Default.PlayerProfileSettings.PlayerProfileActivated)
            {
                var playerProfileProvider =
                    DynamicLoader.CreateInstance<IPlayerProfileProvider>(AppSettings.Default.PlayerProfileSettings
                        .ConcreteProfileGenerator);
                if (playerProfileProvider != null)
                {
                    playerProfileProvider.Init(MainHandlerBase.ConcreteGame.Definitions);
                    ContainerHolder.Container.RegisterInstance<IPlayerProfileProvider>(playerProfileProvider);
                }
            }

            var profileUpdateNotifier = new ProfileUpdateNotifier();
            ContainerHolder.Container.RegisterInstance(profileUpdateNotifier);

            if (!string.IsNullOrEmpty(AppSettings.Default.MessageQueueConnectionString))
            {
                var friendsBus = new FriendsServer.Bus.FriendsBusBackendClient(AppSettings.Default.MessageQueueConnectionString);
                if (friendsBus.Enabled)
                {
                    profileUpdateNotifier.RegisterDest(friendsBus);
                    userStorageEventsListener.Register(friendsBus);
                }
            }

            IProfileStorage profileStorage = new MongoProfileStorage(new MongoUrl(AppSettings.Default.PersistentStorageSettings.StorageConnectionString), "player_profile");
            profileUpdateNotifier.RegisterSource(profileStorage);
            if (AppSettings.Default.PlayerProfileSettings.EnableProfiler)
            {
                profileStorage = new ProfileStorageBenchmark(profileStorage);
            }

            _serviceContainer.AddService(typeof(IProfileStorage), profileStorage);
            ContainerHolder.Container.RegisterInstance<IProfileStorage>(profileStorage);
            ContainerHolder.Container.RegisterSingleton<PlayerProfileApiCallHandler>();
            ContainerHolder.Container.RegisterSingleton<PlayerFilterMatcher>();
            ContainerHolder.Container.RegisterSingleton<BroadcastMessageStorage>();

            var tokenStorage = TokenStorageFactory.GetStorage(AppSettings.Default);
            if (tokenStorage != null)
            {
                var tokenStorageWriter = TokenStorageFactory.GetStoreWriter(AppSettings.Default);
                _serviceContainer.AddService(typeof(ITokenStore), tokenStorage);
                _serviceContainer.AddService(typeof(ITokenStoreWriter), tokenStorageWriter);
            }

            var hive = new BackendHiveMongo(AppSettings.Default.PersistentStorageSettings.StorageConnectionString);
            _serviceContainer.AddService(typeof(IBackendHive), hive);
            _serviceContainer.AddService(typeof(IBackendHivePrivate), hive);

            hive.RegisterSharedConfig(new Uri("http://localhost/" + GetBackendQuery()), SharedConfigWatcher.Instance.Config);

            var userMaintenancePrivate = DefaultDbFactory<IUserMaintenancePrivate>.Create(AppSettings.Default.PersistentStorageSettings.StorageConnectionString);
            var userMaintenance = DefaultDbFactory<IUserMaintenance>.Create(AppSettings.Default.PersistentStorageSettings.StorageConnectionString);
            _serviceContainer.AddService(typeof(IUserMaintenancePrivate), userMaintenancePrivate);
            _serviceContainer.AddService(typeof(IUserMaintenance), userMaintenance);
            container.RegisterInstance(userMaintenancePrivate);
            container.RegisterInstance(userMaintenance);

            var serverMessageStorage = DefaultDbFactory<IServerMessageStorage>.Create(AppSettings.Default.PersistentStorageSettings.StorageConnectionString);
            _serviceContainer.AddService(typeof(IServerMessageStorage), serverMessageStorage);
            var feedbackStorage = DefaultDbFactory<IFeedbackStorage>.Create(AppSettings.Default.PersistentStorageSettings.StorageConnectionString);
            _serviceContainer.AddService(typeof(IFeedbackStorage), feedbackStorage);
            if (!string.IsNullOrEmpty(AppSettings.Default.MessageQueueConnectionString))
            {
                var clansClient = new ClansBackendClient(AppSettings.Default.MessageQueueConnectionString, AppSettings.Default.ClansMessageQueueAppId);
                _serviceContainer.AddService(typeof(ClansBackendClient), clansClient);
            }
        }

        private static string GetBackendQuery()
        {
            var parts = RuntimeSettings.AppRoot.Split(new[] {'\\', '/'}, StringSplitOptions.RemoveEmptyEntries);
            return parts.Last();
        }

        private static void MakeConcreteGame()
        {
            var lib = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\PestelLib.ConcreteSharedLogic.dll";
            var assembly = Assembly.LoadFrom(lib);

            var definitionsType = ServerReflectionUtils.GetTheMostDerivedType(assembly, typeof(IGameDefinitions));
            var sharedLogicType = ServerReflectionUtils.GetTheMostDerivedType(assembly, typeof(ISharedLogic));
            var defaultStateFactory = ServerReflectionUtils.GetTheMostDerivedType(assembly, typeof(DefaultStateFactory));
            if (defaultStateFactory == null)
            {
                /*
                 * DefaultStateFactory может быть и не переопределён в игре, это ОК
                 * В этом случае используется реализация по-умолчанию
                 */
                defaultStateFactory = typeof(DefaultStateFactory);
            }

            Type[] sharedLogicConstructorArgTypes = { typeof(S.UserProfile), definitionsType, typeof(IFeature) };

            var slConstructor = sharedLogicType.GetConstructor(sharedLogicConstructorArgTypes);
            var stateFactory = (DefaultStateFactory)Activator.CreateInstance(defaultStateFactory);

            DynamicGameTemplate gameTemplate = new DynamicGameTemplate(
                slConstructor,
                definitionsType,
                (d) => InitDefinitions(assembly, definitionsType, d),
                stateFactory
            );

            ContainerHolder.Container.RegisterInstance(stateFactory);

            MainHandlerBase.ConcreteGame = gameTemplate;
        }

        private static void InitDefinitions(Assembly assembly, Type defType, IGameDefinitions defs)
        {
            var leagueDefProviderImpl = assembly.GetType("PestelLib.Leagues.SourcesSL.LeagueDefProvider");
            var leagueDefType = assembly.GetType("PestelLib.SharedLogic.Modules.LeagueDef");
            var settingDefType = assembly.GetType("PestelLib.SharedLogic.Modules.SettingDef");
            if (leagueDefProviderImpl == null || leagueDefType == null || settingDefType == null)
            {
                log.WarnFormat("LeagueDefProvider: {0}, LeagueDef: {1}, SettingDef: {2}", leagueDefProviderImpl != null, leagueDefType != null, settingDefType != null);
                return;
            }

            var listType = typeof(List<>);
            var dictType = typeof(Dictionary<,>);
            var leagueDefListType = listType.MakeGenericType(leagueDefType);
            var settingsDictType = dictType.MakeGenericType(typeof(string), settingDefType);
            var leagueDefProviderCtor = leagueDefProviderImpl.GetConstructor(new[] { leagueDefListType, settingsDictType });
            object leagueDefList = null;
            object settingsDict = null;
            foreach (var field in defType.GetFields())
            {
                if (field.FieldType == leagueDefListType)
                {
                    leagueDefList = field.GetValue(defs);
                }
                else if (field.FieldType == settingsDictType)
                {
                    settingsDict = field.GetValue(defs);
                }
            }
            if (leagueDefList == null || settingsDict == null)
            {
                log.WarnFormat("leagueDefList: {0}, settingsDict: {1}", leagueDefList != null, settingsDict != null);
                return;
            }
            LeagueDefProvider = leagueDefProviderCtor.Invoke(new[] { leagueDefList, settingsDict }) as ILeagueDefProvider;
        }
    }
}
