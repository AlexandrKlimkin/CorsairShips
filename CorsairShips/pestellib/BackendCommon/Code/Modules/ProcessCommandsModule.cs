using System;
using System.IO;
using Backend.Code.Utils;
using BackendCommon.Code;
using BackendCommon.Code.Data;
using BackendCommon.Services;
using log4net;
using PestelLib.ServerShared;
using PestelLib.SharedLogicBase;
using S;
using Newtonsoft.Json;
using PestelLib.UniversalSerializer;
using ServerShared;
using UnityDI;
using System.Linq;

namespace ServerLib.Modules
{
    public class ProcessCommandsModule : IModule
    {
        private readonly IFeature _feature;
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProcessCommandsModule));

        public ProcessCommandsModule(IFeature featuresCollection)
        {
            _feature = featuresCollection;
        }

        public ServerResponse ProcessCommand(ServerRequest cmd)
        {
            try
            {
                return InternalProcessCommand(cmd, true, new byte[0]);
            }
            catch (Exception e)
            {
                if (e is SharedLogicException || e is ResponseException)
                {
                    if (!AppSettings.Default.SharedLogicEnabled)
                    {
                        var request = cmd.Request;
                        Guid userIdOnServer;

                        //получаем существующий userId
                        StateLoader.LoadBytes(MainHandlerBase.ConcreteGame, new Guid(request.UserId), request.DeviceUniqueId, (int)request.NetworkId, out userIdOnServer);

                        //заменяем сохранённое состояние
                        StateLoader.Save(userIdOnServer, cmd.State, request.DeviceUniqueId);

                        return new ServerResponse
                        {
                            ResponseCode = ResponseCode.OK,
                            PlayerId = userIdOnServer
                        };
                    }
                    else
                    {
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        public ServerResponse InternalProcessCommand(ServerRequest cmd, bool useStateLoader, byte[] stateBytes)
        {
            var request = cmd.Request;

            var concreteRequest = request.ProcessCommandsBatchRequest;
            var userIdOnServer = Guid.Empty;

            var clientUserId = new Guid(request.UserId);

            if (useStateLoader)
            {
                stateBytes = StateLoader.LoadBytes(MainHandlerBase.ConcreteGame, clientUserId,
                    request.DeviceUniqueId, (int) request.NetworkId, out userIdOnServer);
            }

            var batch = Serializer.Deserialize<CommandBatch>(cmd.Data);

            var config = SharedConfigWatcher.Instance.Config;

            if (AppSettings.Default.OfflineSharedLogicEnabled)
            {
                if (batch.DefinitionsVersion != config.DefinitionsVersion ||
                    batch.SharedLogicCrc != config.SharedLogicCrc)
                {
                    //пришли команды из новой версии клиента, которые были сохранены ещё в старой версии клиента;
                    //в этом случае мы пытаемся выполнить эти команды на соответсвующем бэкэнде
                    Log.Info(
                        $"Received commands with different version: cmd defs: {batch.DefinitionsVersion} cfg defs: {config.DefinitionsVersion} cmd SL: {batch.SharedLogicCrc} cfg SL: {config.SharedLogicCrc}");

                    var hive = MainHandlerBase.ServiceProvider.GetService(typeof(IBackendHive)) as IBackendHive;
                    if (hive == null)
                    {
                        throw new ResponseException(ResponseCode.UNSUPPORTED_COMMANDS, "Can't process commands");
                    }
                    var oldBackends = hive.GetByVersion(batch.SharedLogicCrc, batch.DefinitionsVersion);
                    if (!oldBackends.Any())
                    {
                        throw new ResponseException(ResponseCode.UNSUPPORTED_COMMANDS, $"Appropriate backend not found. cmd SL: {batch.SharedLogicCrc} cfg SL: {config.SharedLogicCrc}.");
                    }
                    ServerResponse result = null;
                    IBackendService service = null;
                    foreach (var back in oldBackends)
                    {
                        result = back.ProcessRequest(cmd);
                        if (result.ResponseCode == ResponseCode.WRONG_CLIENT_VERSION)
                        {
                            Log.Info($"Request forwarding result {result.ResponseCode}. backend={back}.");
                            continue;
                        }
                        service = back;
                        break;
                    }
                    Log.Info($"Request forwarding result {result.ResponseCode}. backend={service}.");
                    return result;
                }
            }
            
            var logic = MainHandlerBase.ConcreteGame.SharedLogic(stateBytes, _feature);
            if (logic.SharedLogicVersion > config.SharedLogicVersion && !batch.IsEditor)
            {
                throw new ResponseException(ResponseCode.WRONG_CLIENT_VERSION, string.Format("logic.SharedLogicVersion: {0} config.SharedLogicVersion: {1}", logic.SharedLogicVersion, config.SharedLogicVersion));
            }
            logic.SharedLogicVersion = config.SharedLogicVersion;

            var commands = batch.commandsList;

            if (request.ProcessCommandsBatchRequest.CommandCount != commands.Count)
            {
                Log.Error("Wrong commands amount: ProcessCommandsBatchRequest.CommandCount = "
                    + request.ProcessCommandsBatchRequest.CommandCount
                    + " but commands.Count = "
                    + commands.Count
                );

                throw new ResponseException(ResponseCode.INVALID_COMMAND_COUNTER);
            }

            try
            {
                var firstCommand = commands[0];
                if (logic.CommandSerialNumber >= firstCommand.SerialNumber)
                {
                    /*
                     * Если первая команда из пакета уже была обработана сервером И deviceId был другой,
                     * то у нас с большой вероятностью произошел двойной вход под одним аккаунтом,
                     * поэтому нужно вернуть ошибку неверной сессии
                     */
                    var lastSession = StateLoader.GetLastUsedDeviceId(userIdOnServer);
                    if (!string.IsNullOrEmpty(lastSession) &&
                        lastSession != request.DeviceUniqueId)
                    {
                        return new ServerResponse
                        {
                            ResponseCode = ResponseCode.WRONG_SESSION,
                            PlayerId = userIdOnServer,
                            ActualUserProfile = stateBytes
                        };
                    }
                }

                var lastCommand = commands[commands.Count - 1];
                var sharedLogicCmd = (Command)lastCommand;
                if (logic.CommandSerialNumber >= sharedLogicCmd.SerialNumber)
                {
                    /*
                     * Если все команды уже были обработаны сервером, значит можно просто послать в ответ что
                     * все ок. Это может случаться, если сервер применил изменения к бд, а клиент при этом не дождался ответа.
                     */
                    return new ServerResponse
                    {
                        ResponseCode = ResponseCode.OK,
                        PlayerId = userIdOnServer
                    };
                }

                logic.ApplyCommands(commands);
            }
            catch (SharedLogicException sharedLogicException)
            {
                Log.Error("client id: " + clientUserId + " server id: " + userIdOnServer + " " + sharedLogicException.Message);

                if (sharedLogicException.ExceptionType ==
                    SharedLogicException.SharedLogicExceptionType.WRONG_COMMANDS_ORDER)
                {
                    throw new ResponseException(ResponseCode.WRONG_COMMAND_ORDER);
                }
                else if (sharedLogicException.ExceptionType ==
                         SharedLogicException.SharedLogicExceptionType.LOST_COMMANDS)
                {
                    return new ServerResponse
                    {
                        ResponseCode = ResponseCode.LOST_COMMANDS,
                        PlayerId = userIdOnServer,
                        ActualUserProfile = stateBytes
                    };
                }
                else
                {
                    throw new ResponseException(ResponseCode.UNKNOWN_SHARED_LOGIC_EXCEPTION);
                }
            }
            catch (Exception e)
            {
                Log.Error("Shared logic processing exception: " + e);
                throw;
            }

            ResponseCode responseCode = (logic.GetStateControlHash() == concreteRequest.hashCode)
                ? ResponseCode.OK
                : ResponseCode.HASH_CHECK_FAILED;

            if (responseCode == ResponseCode.HASH_CHECK_FAILED)
            {
                var msg = "";
                if (cmd.State != null)
                {
                    var originalStateLogic = MainHandlerBase.ConcreteGame.SharedLogic(stateBytes, _feature);
                    var serverOriginalState = originalStateLogic.StateInJson();

                    var commandsJson = logic.CommandsInJson(commands);
                    var serverState = commandsJson + "\r\n" + logic.StateInJson();
                    var l = MainHandlerBase.ConcreteGame.SharedLogic(cmd.State, _feature);
                    var clientState = commandsJson + "\r\n" + l.StateInJson();

                    var logDir = PestelLib.ServerCommon.Log.LogDirectory;
                    var timestamp = DateTime.UtcNow.ToString("_yyyy-MM-dd.hh-mm-ss_");

                    var namePrefix = concreteRequest.IsEditor ? "EDITOR_" : string.Empty;
                    if (!concreteRequest.Integrity)
                    {
                        namePrefix = "HACKER_" + namePrefix;
                    }
                    File.WriteAllText(logDir + namePrefix + userIdOnServer + timestamp + ".server.json", serverState);
                    File.WriteAllText(logDir + namePrefix + userIdOnServer + timestamp + ".client.json", clientState);
                    File.WriteAllText(logDir + namePrefix + userIdOnServer + timestamp + ".server.before.json", serverOriginalState);
                    File.WriteAllBytes(logDir + namePrefix + userIdOnServer + timestamp + ".request.bin", Serializer.Serialize(cmd));
                    File.WriteAllBytes(logDir + namePrefix + userIdOnServer + timestamp + ".state.bin", stateBytes);

                    var info = new HashMismatchInfo
                    {
                        Client = clientState,
                        Server = serverState,
                        Commands = JsonConvert.SerializeObject(commands)
                    };

                    Log.Error("HASH_CHECK_FAILED: clientId: " + clientUserId + " serverId: " + userIdOnServer);
                    msg = JsonConvert.SerializeObject(info);
                }

                return new ServerResponse
                {
                    ResponseCode = ResponseCode.HASH_CHECK_FAILED,
                    PlayerId = userIdOnServer,
                    ActualUserProfile = stateBytes,
                    DebugInfo = msg
                };
            }
            if (responseCode == ResponseCode.OK)
            {
                if (useStateLoader)
                {
                    StateLoader.Save(userIdOnServer, logic.SerializedState, request.DeviceUniqueId);
                }
            }

            return new ServerResponse
            {
                ResponseCode = responseCode,
                PlayerId = userIdOnServer
            };
        }
    }
}