using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Backend.Code.Modules;
using Backend.Code.Modules.PlayerProfile;
using Backend.Code.Statistics;
using Backend.Code.Utils;
using BackendCommon.Code.Auth;
using BackendCommon.Code.Modules;
using BackendCommon.Code.Modules.ClassicLeaderboards;
using BackendCommon.Code.Modules.GlobalConflict;
using BackendCommon.Services;
using log4net;
using MessagePack;
using PestelLib.ClientConfig;
using PestelLib.ServerShared;
using S;
using ServerLib;
using ServerLib.Modules;
using ServerLib.Modules.Leagues;
using ServerLib.Modules.ServerMessages;
using UnityDI;
using PestelLib.ServerCommon.Db;
using BackendCommon.Code.Leagues;

namespace BackendCommon.Code
{
    public class MainHandlerBase : IRequestHandler
    {
        private static ILog Log = LogManager.GetLogger(typeof(MainHandlerBase));
        public static IGameDependent ConcreteGame;
        public static FeaturesCollection FeaturesCollection;

        public static IServiceProvider ServiceProvider;

        private readonly ThreadLocal<Stopwatch> _stopwatch = new ThreadLocal<Stopwatch>(() => new Stopwatch());
        public async Task<byte[]> ProcessData(byte[] signedData, string hostAddr)
        {
            _stopwatch.Value.Restart();
            var hive = ServiceProvider.GetService(typeof(IBackendHive)) as IBackendHive;

            Response processResponse;
            byte[] additionalData = null;
            var sessionId = 0L;

            SharedConfig config = null;
            ServerRequest serverRequest = null;
            Guid serverRequestUserId;
            IModule processingModule= null;

            try
            {
                if (hive?.SelfService?.Maintenance == true)
                {
                    throw new ResponseException(ResponseCode.SERVER_MAINTENANCE);
                }
                if (hive?.SelfService?.Public == false)
                {
                    throw new ResponseException(ResponseCode.WRONG_CLIENT_VERSION);
                }

                CheckSignature(signedData);
                var serializedContainerData = MessageCoder.GetData(signedData);

                var requestContainer = MessagePackSerializer.Deserialize<DataCollection>(serializedContainerData);
                var requestBytes = requestContainer.Request;
                
                serverRequest = new ServerRequest
                {
                    Request = MessagePackSerializer.Deserialize<Request>(requestBytes),
                    Data = requestContainer.Data,
                    State = requestContainer.State,
                    HostAddr = hostAddr
                };
                
                serverRequestUserId = new Guid(serverRequest.Request.UserId);
                MainHandlerBaseStats.Instance.NewPlayer(serverRequestUserId);
                var iptab = ContainerHolder.Container.Resolve<MongoPlayerIpTable>();
                var versionTable = ContainerHolder.Container.Resolve<PlayerIdToVersion>();
                iptab?.Set(serverRequestUserId, hostAddr);
                versionTable?.SetVersion(serverRequestUserId, serverRequest.Request.SharedLogicVersion);

                if (serverRequestUserId != Guid.Empty)
                {
                    CheckBan(serverRequestUserId, serverRequest.Request.DeviceUniqueId);
                    CheckMaintenance(serverRequestUserId);
                }

                config = SharedConfigWatcher.Instance.Config;

                if (AppSettings.Default.CheckVersion
                    && (uint) serverRequest.Request.SharedLogicVersion != config.SharedLogicCrc)
                {
                    throw new ResponseException(ResponseCode.WRONG_CLIENT_VERSION);
                }
                
                processingModule = GetProcessingModule(serverRequest.Request);

                ServerResponse resp;
                if (processingModule is IModuleAsync asyncModule)
                {
                    resp = await asyncModule.ProcessCommandAsync(serverRequest);
                }
                else
                {
                    resp = processingModule.ProcessCommand(serverRequest);
                }

                processResponse = new Response
                {
                    ResponseCode = resp.ResponseCode,
                    PlayerId = resp.PlayerId.ToByteArray(),
                    Timestamp = DateTime.UtcNow.Ticks,
                    Banned = LeagueStorage?.HasBan(resp.PlayerId) ?? false,
                    ActualUserProfile = resp.ActualUserProfile,
                    Token = resp.Token,
                    DebugInfo = resp.DebugInfo,
                    ShortId = resp.ShortId
                };
                additionalData = resp.Data;
            }
            catch (ResponseException e)
            {
                processResponse = new Response
                {
                    ResponseCode = e.ResponseCode,
                    DebugInfo = e.DebugMessage
                };
                Log.Error(e.ResponseCode + " " + e + " " + e.DebugMessage);
            }
            catch (Exception e)
            {
                processResponse = new Response
                {
                    ResponseCode = ResponseCode.SERVER_EXCEPTION,
                    ServerStackTrace = e.Message + e.StackTrace
                };
                Log.Error($"From {hostAddr}: {e}");
            }

            if (serverRequest != null && config != null && serverRequest.Request.DefsVersion < config.DefinitionsVersion)
            {
                processResponse.DefsData = DefsLoader.DefsData;
                processResponse.DefsData.Version = (int)config.DefinitionsVersion;
            }

            try
            {
                processResponse.MaintenanceTimestamp = GlobalMaintenanceBeginTimestamp();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            processResponse.SessionId = sessionId;

            var responseData = MessagePackSerializer.Serialize(processResponse);

            var container = new DataCollection
            {
                Response = responseData, 
                Data = additionalData
            };
            MainHandlerBaseStats.Instance.NewResponse(processResponse, _stopwatch.Value, processingModule);
            return MessagePackSerializer.Serialize(container);
        }

        private long GlobalMaintenanceBeginTimestamp()
        {
            if (!BackendService.Maintenance)
                return 0;

            return BackendService.MaintenanceStart.Ticks;
        }

        private void CheckBan(Guid userId, string deviceId)
        {
            var banService = ServiceProvider.GetService(typeof(IGameBanStorage)) as IGameBanStorage;
            if (banService == null) return;

            if(!banService.IsBanned(userId, deviceId, out var reason)) return;

            throw new ResponseException(ResponseCode.BANNED, reason);
        }

        private void CheckMaintenance(Guid userId)
        {
            if (BackendService.Maintenance)
            {
                throw new ResponseException(ResponseCode.SERVER_MAINTENANCE);
            }

            var maintEnd = UserMaintenanceApi.GetUserMaintenanceEnd(userId);
            if (DateTime.UtcNow < maintEnd)
            {
                throw new ResponseException(ResponseCode.SERVER_MAINTENANCE);
            }
        }

        public async Task<byte[]> Process(byte[] data, RequestContext ctx)
        {
            return await ProcessData(data, ctx.RemoteAddr);
        }

        private static void CheckSignature(byte[] signedData)
        {
            if (signedData.Length == 0)
            {
                throw new ResponseException(ResponseCode.EMPTY_REQUEST);
            }
            
            if (signedData.Length <= MessageCoder.SignatureLength)
            {
                throw new ResponseException(ResponseCode.INCOMPLETE_REQUEST);
            }

            if (!MessageCoder.CheckSignature(signedData))
            {
                throw new ResponseException(ResponseCode.BAD_SIGNATURE);
            }
        }

        public static IModule GetProcessingModule(Request request)
        {
            IModule processingModule;
            if (request.ExtensionModuleRequest != null)
            {
                return ContainerHolder.Container.Resolve<ServerExtensions>();
            }
            else if (request.ExtensionModuleAsyncRequest != null)
            {
                return ContainerHolder.Container.Resolve<ServerExtensionsAsync>();
            }
            else if (request.InitRequest != null)
            {
                processingModule = new InitDataModule();
            }
            else if (request.ProcessCommandsBatchRequest != null)
            {
                processingModule = new ProcessCommandsModule(FeaturesCollection);
            }
            else if (request.ResetRequest != null)
            {
                processingModule = new ResetDataModule();
            }
            else if (request.SetFacebookIdRequest != null)
            {
                processingModule = new SetFacebookIdModule();
            }
            else if (request.GetProfileByFacebookIdRequest != null)
            {
                processingModule = new GetProfileModule();
            }
            else if (request.GetRandomUserIds != null)
            {
                processingModule = new GetRandomUserIdsModule();
            }
            else if (request.SyncTime != null)
            {
                processingModule = new SyncTimeModule();
            }
            else if (request.ReplaceStateRequest != null)
            {
                processingModule = new ReplaceStateModule();
            }
            else if (request.RegisterPayment != null)
            {
                processingModule = new RegisterPaymentModule();
            }
            else if (request.SendFeedback != null)
            {
                processingModule = new SendFeedbackModule(ServiceProvider);
            }
            else if (request.LeaderboardRegisterRecord != null)
            {
                processingModule = new LeaderboardRegisterModule();
            }
            else if (request.LeaderboardGetRank != null)
            {
                processingModule = new LeaderboardGetRankModule();
            }
            else if (request.LeaderboardGetRankTop != null)
            {
                processingModule = new LeaderboardGetRankTopModule();
            }
            else if (request.LeaderboardGetRankTopChunk != null)
            {
                processingModule = new LeaderboardGetRankTopChunkModule();
            }
            else if (request.LeaderboardGetFacebookFriendsTop != null)
            {
                processingModule = new LeaderboardGetFacebookFriendsTopModule();
            }
            else if (request.UsePromo != null)
            {
                processingModule = new PromoModule(ServiceProvider);
            }
            else if (request.GetServerMessagesInbox != null)
            {
                processingModule = new GetServerInboxModule();
            }
            else if(request.LeagueRegister != null)
            {
                processingModule = new LeagueRegisterModule(ServiceProvider);
            }
            else if (request.LeaguePlayerGlobalRank != null)
            {
                processingModule = new LeaguePlayerGlobalRankModule(ServiceProvider);
            }
            else if (request.LeaguePlayerLeagueRank != null)
            {
                processingModule = new LeaguePlayerLeagueRankModule(ServiceProvider);
            }
            else if (request.LeagueDivisionRanks != null)
            {
                processingModule = new LeagueDivisionRanksModule(ServiceProvider);
            }
            else if (request.LeagueTop != null)
            {
                processingModule = new LeagueTopModule(ServiceProvider);
            }
            else if (request.LeagueGlobalTop != null)
            {
                processingModule = new LeagueGlobalTopModule(ServiceProvider);
            }
            else if (request.DefsRequest != null)
            {
                processingModule = new DefsModule();
            }
            else if (request.DeleteUserRequest != null)
            {
                processingModule = new DeleteUserModule();
            }
            else if (request.GlobalConflictApiCall != null)
            {
                processingModule = new GlobalConflictApiModule();
            }
            else if(request.PlayerProfile != null)
            {
                processingModule = new PlayerProfileApiModule();
            }
            else if (request.ValidateSessionRequest != null)
            {
                processingModule = new ValidateSessionModule();
            }
            else if (request.LeaderboardGetSeasonInfoRequest != null)
            {
                processingModule = new LeaderboardGetSeasonInfoModule();
            }
            else
            {
                throw new ResponseException(ResponseCode.COMMAND_NOT_FOUND);
            }
            return processingModule;
        }

        public bool IsReusable {
            get {
                return true;
            }
        }

        public static IUserMaintenance UserMaintenanceApi {
            get {
                if (_userMaintenanceApi != null)
                    return _userMaintenanceApi;
                return _userMaintenanceApi = ContainerHolder.Container.Resolve<IUserMaintenance>();
            }
        }

        public static IBackendService BackendService {
            get {
                if (_backendHive == null)
                    _backendHive = ServiceProvider.GetService(typeof(IBackendHive)) as IBackendHive;
                return _backendHive?.SelfService;
            }
        }

        // TODO: вынести логику бана в лиге из интерфейса, чтоб не нужно было использовать ILeagueStorage здесь
        private static ILeagueStorage LeagueStorage
        {
            get {
                if (_leagueStorage == null)
                    _leagueStorage = ServiceProvider.GetService(typeof(ILeagueStorage)) as ILeagueStorage;
                return _leagueStorage;
            }
        }

        private static IUserMaintenance _userMaintenanceApi;
        private static IBackendHive _backendHive;
        private static ILeagueStorage _leagueStorage;
    }
}