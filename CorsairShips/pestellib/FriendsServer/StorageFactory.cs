using FriendsServer.Db;
using FriendsServer.Db.Cache;
using FriendsServer.Db.External;
using FriendsServer.Db.Mongo;
using UnityDI;

namespace FriendsServer
{
    class StorageFactory
    {
        public StorageFactory()
        {
            var c = ContainerHolder.Container;
            var config = ServerConfigCache.Get();
            var mongoConnection = config.ConnectionString;
            var friendsStorage = new MongoFriendsStorage(mongoConnection);
            var invitationStorage = new MongoInvitationStorage(mongoConnection);
            var giftStorage = new MongoGiftStorage(mongoConnection);
            var profileStorage = new ExternalProfileStorage();
            var roomStorage = new CachedRoomStorage();
            var roomInvStorage = new CachedRoomInviteStorage();
            var nonFriendsStatusWatchStorage = new MongoNonFriendsStatusWatch(mongoConnection);
            c.RegisterInstance<IFriendStorage>(friendsStorage);
            c.RegisterInstance<IInvitationStorage>(invitationStorage);
            c.RegisterInstance<IProfileStorage>(profileStorage);
            c.RegisterInstance<IGiftStorage>(giftStorage);
            c.RegisterInstance<IRoomStorage>(roomStorage);
            c.RegisterInstance<IRoomInviteStorage>(roomInvStorage);
            c.RegisterInstance<INonFriendsStatusWatch>(nonFriendsStatusWatchStorage);
        }
    }
}
