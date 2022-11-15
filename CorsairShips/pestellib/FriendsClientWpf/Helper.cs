using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FriendsClient;
using FriendsClient.Sources;

namespace FriendsClientWpf
{
    static class Helper
    {
        public static string GetFriendStatusString(FriendBase friend)
        {
            switch (friend.Status)
            {
                case FriendStatus.Online: return nameof(FriendStatus.Online);
                case FriendStatus.InRoom: return nameof(FriendStatus.InRoom);
                case FriendStatus.Offline: return nameof(FriendStatus.Offline);
                default: return friend.Status.ToString();
            }
        }
    }
}
