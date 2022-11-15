using System;
using System.Threading.Tasks;
using FriendsClient.FriendList;
using FriendsClient.Lobby;
using FriendsClient.Private;
using S;

namespace FriendsClient
{
    public interface IFriendsClient
    {
        /// <summary>
        /// Friend id of current user.
        /// </summary>
        MadId Id { get; }

        bool IsConnected { get; }
        bool Initialized { get; }
        int Status { get; set; }

        Task WaitInit();
        /// <summary>
        /// Client logged in and ready to send requests (only if <see cref="FriendInitResult.Success"/>).
        /// </summary>
        event Action<FriendInitResult> OnInitialized;

        event Action OnConnected;
        event Action OnConnectionError;
        event Action OnDisconnected;
        /// <summary>
        /// Control your friend list through this api (send/get invites, remove friends, get friend infos, gifts etc.).
        /// </summary>
        IFriendList FriendList { get; }
        /// <summary>
        /// Use this api to create/join room.
        /// </summary>
        ILobby Lobby { get; }
        /// <summary>
        /// Part of server config. Valid after init done (<see cref="OnInitialized"/>, <see cref="Initialized"/>).
        /// </summary>
        SharedConfig Config { get; }
    }
}
