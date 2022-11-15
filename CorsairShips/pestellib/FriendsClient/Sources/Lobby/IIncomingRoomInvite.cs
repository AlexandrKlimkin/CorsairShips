using System;
using PestelLib.ServerClientUtils;

namespace FriendsClient.Lobby
{
    public interface IIncomingRoomInvite
    {
        /// <summary>
        /// Invitation timeout. Use <see cref="SharedTime"/> for calculations.
        /// </summary>
        DateTime Timeout { get; }

        FriendBase FriendInfo { get; }
        /// <summary>
        /// The room you were invited to.
        /// Not null only if ServerConfig.RoomUpdatesForInvited == true.
        /// </summary>
        IFriendsRoom Room { get; }

        bool Expired { get; }

        event Action<IIncomingRoomInvite> OnExpired;

        /// <summary>
        /// Accepts invitation. Wait <see cref="ILobby.OnJoinRoom"/> fro room controller.
        /// 
        /// posible RoomResult values passed to callback:
        ///     * Success       - you are joined the room. Wait for <see cref="ILobby.OnJoinRoom"/>.
        ///     * InvalidStatus - battle already started
        ///     * NotInvited    - you are notinvited to room (hacker)
        ///     * AlreadyJoined - duplicated Accept calls, already Accepted
        ///     * RoomIdFull    - room capacity reached, can't invite more players
        /// </summary>
        void Accept(FriendsDelegate.RoomAnswerCallback callback = null);
        /// <summary>
        /// Rejects invitation.
        /// </summary>
        void Reject(FriendsDelegate.RoomAnswerCallback callback = null);
    }
}
