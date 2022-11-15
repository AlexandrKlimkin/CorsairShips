using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BoltLoadBalancing.MasterServer;
using BoltLoadBalancing.MatchMaking;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("BoltLoadBalancingTest")]
namespace BoltLoadBalancing.Logic
{
    public class Matchmaking : IMatchmaking
    {
        /*
         * Матч мейкинг как в фотоне - должны совпадать все параметры между запросом клиента
         * RequestServer.MatchmakingData и созданной комнатой GameServerStateReport.MatchmakingData
         */
        public virtual List<MatchMakingGame> GetPossibleMatches(string matchmakingString, IEnumerable<IMasterServer> masters)
        {
            var result = new List<MatchMakingGame>();
            var matchmakingParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(matchmakingString);

            foreach (var m in masters)
            {
                foreach (var gameServer in m.GameServers)
                {
                    if (gameServer.Players >= gameServer.MaxPlayers) continue;

                    var gameServerMatchmakingParams = JsonConvert.DeserializeObject<Dictionary<string, string>>(gameServer.MatchmakingData);
                    if (IsParamsTheSame(matchmakingParams, gameServerMatchmakingParams))
                    {
                        result.Add(new MatchMakingGame {
                            MasterServer = m,
                            GameServerStateReport = gameServer
                        });
                    }
                }
            }
                        
            return result;
        }

        /*
         * Каждое значение из словаря 'a' должно быть точно таким же в словаре 'b'
         * При этом в словаре 'b' может быть больше пар ключ-значение, чем в 'a'
         */
        private bool IsParamsTheSame(Dictionary<string,string> a, Dictionary<string,string> b)
        {
            foreach (var pair in a)
            {
                var aValue = pair.Key;
                if (!b.ContainsKey(pair.Key) || b[pair.Key] != pair.Value)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
