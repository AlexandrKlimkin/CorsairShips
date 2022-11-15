using System;
using PestelLib.ServerCommon.Config;

namespace FriendsServer
{
    class ServerConfig
    {
        public int Port = 9001;
        public int MaxFriends = 30;
        public int MaxInvites = 100;
        public int MaxRoomParty = 5;
        public TimeSpan InviteTTL = TimeSpan.FromDays(7);
        public TimeSpan RoomInviteTTL = TimeSpan.FromSeconds(10);
        public string ConnectionString = "mongodb://localhost/friends";
        /// <summary>
        /// If UnlockGiftsAtMidnight is true when this parameter is ignored.
        /// </summary>
        public TimeSpan GiftCooldown = TimeSpan.FromDays(1);
        public bool UnlockGiftsAtMidnight = false;
        public string ProfileServiceUrl = string.Empty;
        public bool ProfileServiceEnabled = false;
        public bool MadIdMixed = false;
        public int GiftsStackSize = 0; // 0 == no limit
        /// <summary>
        /// Invited players will get room updates (party change, kick, leave, gamespecificdata etc.) while invite not answered.
        /// </summary>
        public bool RoomUpdatesForInvited = false;
        /// <summary>
        /// true - if you want to start battle on same room more than once. Consider to pick large enough room autostart delay (1 day or bigger) to prevent room host kicks.
        /// </summary>
        public bool DontCloseRoomOnBattleStart = false;
        /// <summary>
        /// How often server checks profile updates.
        /// </summary>
        public TimeSpan ProfileUpdateFrequency = TimeSpan.FromMinutes(1);
        /// <summary>
        /// Valid period for party to start battle since host's StartBattle request reached server.
        /// </summary>
        public TimeSpan StartBattleDelay = TimeSpan.FromSeconds(15);

        public string MessageQueueConnectionString = string.Empty;
        public bool EnableStats = true;
        public string StatsServerAddr = "pestelcrew.com:2003";
        public bool RoomInviteAnybody = false;
    }

    static class ServerConfigCache
    {
        public static ServerConfig Get()
        {
            if (_inst == null)
                _inst = SimpleJsonConfigLoader.LoadConfigFromFile<ServerConfig>("ServerConfig.json", false);
            return _inst;
        }

        private static ServerConfig _inst;
    }
}