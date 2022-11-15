using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using log4net;
using MasterServerProtocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[assembly: InternalsVisibleTo("BoltMasterServerTest")]
namespace BoltMasterServer
{
    internal partial class AgonesGameServerLauncher : IGameServerLauncher
    {
        private Settings settings;
        private static ILog _log = LogManager.GetLogger(typeof(GameServerLauncher));

        private GameServersCollection gameServersCollection;
        public AgonesGameServerLauncher(GameServersCollection gameServersCollection, Settings settings)
        {
            this.gameServersCollection = gameServersCollection;
            this.settings = settings;
        }

        private static readonly HttpClient client = new HttpClient();

        public bool KillProcess(Guid gameServerId) 
        {
            /*
             * Остановкой процесса игрового сервера занимается Agones, нет необходимости делать что-то здесь
             */
            return true;
        }

        public async Task<JoinInfo> StartNewServerInstance(string matchmakingData)
        {
            while (true)
            {
                //TODO: добавить передачу matchmakingData
                var connectionData = await AllocateGameServer(matchmakingData);

                _log.Info("created server id: " + connectionData.GameServerId);

                IGameServer server;
                Stopwatch stopwatch = Stopwatch.StartNew();
                while (!gameServersCollection.TryGetValue(connectionData.GameServerId, out server))
                {
                    await Task.Delay(30);
                    if (stopwatch.Elapsed > TimeSpan.FromSeconds(10))
                    {
                        _log.Error("Can't receive information from recently started server");
                        break;
                    }
                }

                if (!gameServersCollection.TryGetValue(connectionData.GameServerId, out server))
                    continue;

                _log.Info("server connected, id = " + connectionData.GameServerId);

                server.Port = connectionData.Port;
                server.IPAddress = connectionData.IPAddress;

                _log.Info("reserve slot begin");
                var reserveSlotResponse = await server.ReserveSlot(new ReserveSlotRequest());

                _log.Info("reserve slot finish");

                return new JoinInfo
                {
                    Port = server.Port,
                    IpAddress = server.IPAddress,
                    ReservedSlot = reserveSlotResponse.JoinInfo.ReservedSlot,
                    Map = reserveSlotResponse.JoinInfo.Map
                };
            }
        }

        public void StopAllProcesses()
        {
            /*
             * При падении или закрытии мастер сервера игровые сервера под его управлением останутся аллоцированными
             * Нужно добавить в логику самого игрового сервера, чтобы он вызывал метод Shutdown в AgnoesSDK
             * В том случае, если у него нет связи с мастер сервером.
             * Либо, когда нет связи и закончился раунд - что бы игроки, которые уже в игре, могли доиграть матч.
             */
            return;
        }

        private async Task<ConnectionData> AllocateGameServer(string matchmakingData)
        {
            var mm = JsonConvert.DeserializeObject<Dictionary<string, string>>(matchmakingData);
            StringBuilder mmDataStr = new StringBuilder();
            foreach (var kv in mm)
            { 
                if (mmDataStr.Length > 0)
                {
                    mmDataStr.AppendLine(",");
                }

                mmDataStr.Append($"\"matchmaking_{kv.Key}\" : \"{kv.Value}\"");
            }
            mmDataStr.AppendLine("");
            
            var gameServerInstanceId = Guid.NewGuid();

            var requestTemplate = @"
{
    ""apiVersion"":""allocation.agones.dev/v1"",
    ""kind"":""GameServerAllocation"",
    ""spec"":{
        ""required"":{
            ""matchLabels"":{
                ""agones.dev/fleet"":""%FLEET%""
            }
        },
        ""metadata"":{
            ""labels"": {
                ""gameServerId"" : ""%GAME_SERVER_ID%"",
                ""masterAddress"" : ""%MASTER_ADDRESS%"",
                ""masterport"" : ""%MASTER_PORT%"",
                %MATCHMAKING%
            }
        }
    }
}";

            var requestString = requestTemplate
                .Replace("%FLEET%", settings.AgonesFleetName)
                .Replace("%GAME_SERVER_ID%", gameServerInstanceId.ToString())
                .Replace("%MASTER_PORT%", settings.MasterListenerPort.ToString())
                .Replace("%MASTER_ADDRESS%", settings.MasterServerIp)
                .Replace("%MATCHMAKING%", mmDataStr.ToString());
                        
            try
            {
                _log.Debug("Request: " + requestString);

                var content = new StringContent(requestString, Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var resp = await client.PostAsync("http://localhost:8001/apis/allocation.agones.dev/v1/namespaces/default/gameserverallocations", content);
                if (resp.IsSuccessStatusCode)
                {
                    var responseText = await resp.Content.ReadAsStringAsync();

                    _log.Debug("Response: " + responseText);

                    JObject results = JsonConvert.DeserializeObject<JObject>(responseText);
                    var state = (string)results["status"]["state"];
                    if (state == "Allocated")
                    {
                        var port = (int)results["status"]["ports"][0]["port"];
                        var address = (string)results["status"]["address"];
                        _log.Info($"You can join to {address}:{port}");
                        
                        return new ConnectionData { 
                            GameServerId = gameServerInstanceId,
                            IPAddress = address,
                            Port = port
                        };
                    }
                    else
                    {
                        var error = "Wrong response state: " + state;
                        _log.Error(error);
                        throw new Exception(error);
                    }
                }
                else
                {
                    var error = $"Bad HTTP response code: {resp}";                    
                    _log.Error(error);
                    try
                    {
                        var respText = await resp.Content.ReadAsStringAsync();
                        _log.Error(respText);
                    } 
                    catch(Exception e)
                    {
                        _log.Error("cant get content from response " + e.Message);
                    }
                    throw new Exception(error);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception during game server allocation: " + e.Message + "\n" + e.StackTrace);
                throw e;
            }
        }
    }
}