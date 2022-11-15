using System;
using System.Collections.Generic;
using System.Text;
using FriendsClient.Private;

namespace FriendsClient.Lobby
{
    public interface ILobby
    {
        /// <summary>
        /// Player is busy (in shop, opening boxes, etc.). Asks server to resend battle invitations later if any are present.
        /// </summary>
        bool MuteBattleInvitations { get; set; }

        IFriendsRoom Room { get; }

        /// <summary>
        /// You were invited to join room for battle with friends.
        /// </summary>
        event Action<IIncomingRoomInvite> OnRoomInvite;
        /// <summary>
        /// If you have accepted room invite wait this event to join this room.
        /// </summary>
        event Action<IFriendsRoom> OnJoinRoom;
        /// <summary>
        /// Room created successfuly. Result of <see cref="CreateRoom"/> call.
        /// </summary>
        event Action<IFriendsRoom> OnRoomCreated;

        void CreateRoom(TimeSpan autostartDelay, string gameSpecificData, Action<RoomResult, IFriendsRoom> callback);
        /// <summary>
        /// Looks for IFriendsRoom in cache and returns it if found, null otherwise.
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        IFriendsRoom GetRoom(long roomId);
    }
}
