using System;
using Lidgren.Network;
using Newtonsoft.Json;

namespace PestelLib.ChatCommon
{
    public static class Consts
    {
        public const string ChatAppId = "PCChat";
        public const int ChatPort = 8001;
        public const int ChatChannelHistorySize = 20;
        public const NetDeliveryMethod DefaultDeliveryMethod = NetDeliveryMethod.ReliableOrdered;
        public const bool AllowMultichannel = false;
        public const bool FloodProtection = false;
        public const int FloodMessagesPerMinute = 60;
        public const int FloodDataPerMinute = 20000;
        public const float FloodMessagesPerc = 0.7f;
        public const int FloodInBanMessageCountLevelUp = 5;
        public const int FloodMaxLevel = 9;
        public const int BadMessageLimit = 4;
        public static readonly JsonSerializerSettings JsonSerializerSettings;

        static Consts()
        {
            JsonSerializerSettings = new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
        }

        public static object Init(object a)
        {
            var peer = a as NetPeer;
            var data = new byte[16];
            data[0] = 0xf9;
            data[1] = 0x19;
            data[2] = 0x0c;
            data[3] = 0xa9;
            data[4] = 0x84;
            data[5] = 0x79;
            data[6] = 0xa4;
            data[7] = 0x4f;
            data[8] = 0x35;
            data[9] = 0xf7;
            data[10] = 0xe4;
            data[11] = 0xc3;
            data[12] = 0x78;
            data[13] = 0x3c;
            data[14] = 0xbb;
            data[15] = 0x09;
            return new NetXtea(peer, data);
        }

        public const string Temp = "bY9n5s3n";
    }
}
