using System;
using ServerLib;
using PestelLib.ServerCommon.Config;

namespace PestelLib.MatchmakerServer.Config
{
    public class MatchmakerConfig
    {
        public const string FILE_NAME = "matchmaker_config.json";

        public TimeSpan MaxAcceptWaitTime = TimeSpan.FromSeconds(1);
        public TimeSpan SendMatchStatePeriod = TimeSpan.FromSeconds(1);
        public TimeSpan SendServerStatsPeriod = TimeSpan.FromSeconds(3);
        public bool UseGrafana = false;
        public string GrafanaHost = "pestelcrew.com";
        public int GrafanaPort = 2003;
        public string ServerId = "dummy";
        public int ServerPort = 8500;
    }

    public static class MatchmakerConfigCache
    {
        public static MatchmakerConfig Get()
        {
            return SimpleJsonConfigLoader.LoadConfigFromFile<MatchmakerConfig>("matchmaker_config.json", true);
        }
    }
}
