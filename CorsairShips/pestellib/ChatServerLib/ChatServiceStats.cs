using System;
using System.Collections.Generic;
using ChatServer.Transport;

namespace PestelLib.ChatServer
{
    class ChatServiceStats : ChatServerTransportStats
    {
        public int RoomsCount;
        public TimeSpan Uptime;
        public Dictionary<string, int> UsersInRooms;
        public int FloodBans;
        public int MaxFloodBanLevel;
        public Dictionary<string, int> BansInRooms;
    }
}
