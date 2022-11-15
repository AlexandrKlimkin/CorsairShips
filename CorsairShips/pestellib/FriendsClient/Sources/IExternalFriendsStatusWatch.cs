using System.Collections.Generic;
using S;

namespace FriendsClient
{
    public interface IExternalFriendsStatusWatch
    {
        IEnumerable<FriendBase> GetWatchedPlayers();
        bool IsWatchedPlayer(MadId id);
    }
}
