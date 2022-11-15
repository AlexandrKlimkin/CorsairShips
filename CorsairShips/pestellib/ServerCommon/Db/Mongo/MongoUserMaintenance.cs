using System;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;

namespace PestelLib.ServerCommon.Db.Mongo
{
    [BsonIgnoreExtraElements]
    class UserMaintenanceItem
    {
        [BsonId]
        public Guid UserId;
        public DateTime MaintenanceEnd;
    }

    public class MongoUserMaintenance : IUserMaintenance, IUserMaintenancePrivate
    {
        private IMongoCollection<UserMaintenanceItem> _maintenanceCollection;

        public MongoUserMaintenance(MongoUrl connectionUrl)
        {
            var conn = connectionUrl.GetServer();
            var db = conn.GetDatabase(connectionUrl.DatabaseName);
            _maintenanceCollection = db.GetCollection<UserMaintenanceItem>("UserMaintenance");
        }

        public DateTime GetUserMaintenanceEnd(Guid userId)
        {
            var filter = Builders<UserMaintenanceItem>.Filter.Eq(_ => _.UserId, userId);
            var item = _maintenanceCollection.FindSync(filter).SingleOrDefault();
            if (item == null)
                return DateTime.MinValue;
            return item.MaintenanceEnd;
        }

        public void SetUserMaintenance(Guid userId, DateTime untilDate)
        {
            var filter = Builders<UserMaintenanceItem>.Filter.Eq(_ => _.UserId, userId);
            var update = Builders<UserMaintenanceItem>.Update.Set(_ => _.MaintenanceEnd, untilDate);
            var opts = new FindOneAndUpdateOptions<UserMaintenanceItem>();
            opts.IsUpsert = true;
            _maintenanceCollection.FindOneAndUpdate(filter, update, opts);
        }

        public long RemoveBeforeDate(DateTime excludeDate)
        {
            var filter = Builders<UserMaintenanceItem>.Filter.Lt(_ => _.MaintenanceEnd, excludeDate);
            var result = _maintenanceCollection.DeleteMany(filter);
            return result.DeletedCount;
        }

        public void Remove(Guid userId)
        {
            var filter = Builders<UserMaintenanceItem>.Filter.Eq(_ => _.UserId, userId);
            _maintenanceCollection.DeleteOne(filter);
        }
    }
}
