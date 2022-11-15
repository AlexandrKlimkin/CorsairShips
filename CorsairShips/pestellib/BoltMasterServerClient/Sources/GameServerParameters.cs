using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace BoltGameServerToMasterServerConnector
{
    public class GameServerParameters
    {
        public Guid? GameServerId;
        public int? MasterPort;
        public string MasterIpAddress;
        public int? ServerPort = 7777; //7777 - default, used on all agones game servers. Could be overriden from cmd line args

        public int? MaxConnections = 16;
        public bool ServerOpened = true;

        public string GameServerIpAddress;
        public int? GameServerExternalPort = 7777;

        public string MatchMakingDataStr;
        public bool WorkingWithAgones;
        private Dictionary<string, string> _matchmakingData;

        public GameServerParameters()
        {
            if (Guid.TryParse(GetArg("-gameServerId"), out var gameServerId))
            {
                GameServerId = gameServerId;
            }

            if (!string.IsNullOrEmpty(GetArg("-master")))
            {
                MasterIpAddress = GetArg("-master");
            }

            if (int.TryParse(GetArg("-masterport"), out var port))
            {
                MasterPort = port;
            }

            if (int.TryParse(GetArg("-serverport"), out var serverport))
            {
                ServerPort = serverport;
                GameServerExternalPort = serverport;
            }

            if (!string.IsNullOrEmpty(GetArg("-matchmakingData")))
            {
                var base64Params = GetArg("-matchmakingData");
                var matchmakingDataBytes = Convert.FromBase64String(base64Params);
                MatchMakingDataStr = Encoding.UTF8.GetString(matchmakingDataBytes);
            }

            if (int.TryParse(GetArg("-maxplayers"), out var maxplayers))
            {
                MaxConnections = maxplayers;
            }

            if (!string.IsNullOrEmpty(GetArg("-ip")))
            {
                GameServerIpAddress = GetArg("-ip");
            }
        }


        public string Map => (MatchmakingData != null && MatchmakingData.ContainsKey("map")) ? MatchmakingData["map"] : null;

        public Dictionary<string, string> MatchmakingData
        {
            get
            {
                if (MatchMakingDataStr == null) return null;
                if (_matchmakingData == null)
                {
                    _matchmakingData = JsonConvert.DeserializeObject<Dictionary<string, string>>(MatchMakingDataStr);
                }
                return _matchmakingData;
            }
        }

        public static string GetArg(params string[] names)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                foreach (var name in names)
                {
                    if (args[i] == name && args.Length > i + 1)
                    {
                        return args[i + 1];
                    }
                }
            }

            return null;
        }
    }
}