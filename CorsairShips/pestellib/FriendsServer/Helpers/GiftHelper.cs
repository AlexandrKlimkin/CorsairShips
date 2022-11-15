using System;

namespace FriendsServer.Helpers
{
    class GiftHelper
    {
        public static ServerConfig config = ServerConfigCache.Get();
        public static DateTime NextGift(DateTime lastGift)
        {
            if (config.UnlockGiftsAtMidnight)
            {
                return lastGift.Date.AddDays(1);
            }

            return lastGift + config.GiftCooldown;
        }
    }
}
