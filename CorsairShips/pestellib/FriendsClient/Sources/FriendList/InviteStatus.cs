namespace FriendsClient.FriendList
{
    public enum InviteStatus
    {
        /// <summary>
        /// Invitation not sent.
        /// </summary>
        None,
        /// <summary>
        /// invitation awaiting player input.
        /// </summary>
        Pending,
        /// <summary>
        /// invitation accepted.
        /// </summary>
        Accepted,
        /// <summary>
        /// invitation rejected.
        /// </summary>
        Rejected,
        /// <summary>
        /// invitation rejected by answer timeout (ServerConfig.RoomInviteTTL).
        /// </summary>
        AutoRejected,
        /// <summary>
        /// while performing invite error had happen.
        /// </summary>
        Error
    }

    public static class InviteStatusExtensions
    {
        public static bool CanResend(this InviteStatus status)
        {
            if (status != InviteStatus.Accepted && status != InviteStatus.Pending)
                return true;
            return false;
        }
    }
}