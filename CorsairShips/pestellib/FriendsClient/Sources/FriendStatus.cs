namespace FriendsClient.Sources
{
    public class FriendStatus
    {
        public const int Offline = 0;
        public const int InRoom = 1;    // Joined or creted room and is about to start battle. Not same as in battle (server dont handle in battle state automaticaly).
        public const int Online = 2;

        public const int Count = 3;
    }

    // This is an example of how you could add your values for FriendBase.Status.
    // Dont use same status codes if you want to distinguish one from another ^_^.
    /*
    
    public class FriendStatusMyGame : FriendStatus
    {
        public const int InBattle = 3;  // in battle
        public const int InGarage = 4;  // specific scene
    }

    */
    
}
