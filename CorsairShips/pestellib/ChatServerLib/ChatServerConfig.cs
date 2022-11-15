using PestelLib.ChatCommon;
using PestelLib.ServerCommon.Config;

namespace PestelLib.ChatServer
{
    public enum ChatServerMessageProvider
    {
        Lidgren,
        MessageServerTcp
    }

    public class ChatServerConfig
    {
        public string GraphiteHost = "pestelcrew.com";
        public int GraphitePort = 2003;
        public string ChatServerName = "dummy";
        public bool MultiChannel = Consts.AllowMultichannel;
        public string Secret = "pestel_chat_Udm7Kzhj30iH";
        public int ChatChannelHistorySize = Consts.ChatChannelHistorySize;
        public int BadWordCount = 3;
        public int BadWordBanTime = 600; // seconds
        public int BadMessageLimit = Consts.BadMessageLimit;
        public string MongoConnectionString = string.Empty;
        public int Port = Consts.ChatPort;
        public bool UseMessageEncryption = false;
        public string TokenStorageConnectionString = string.Empty;
        public bool TokenIpCheck = false;
        public ChatServerMessageProvider ChatServerMessageProvider;
        public string[] BannedChannels;
        public string[] AdminInteropIPs;
        public string SuperuserSecret;
    }

    public static class ChatServerConfigCache
    {
        public static ChatServerConfig Get()
        {
            return SimpleJsonConfigLoader.LoadConfigFromFile<ChatServerConfig>("chat_server_config.json", false);
        }
    }
}
