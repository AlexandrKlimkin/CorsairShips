using System;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using PestelLib.ServerShared;
using S;

namespace PestelLib.ServerCommon.Db.Mongo
{
    public class FeedbackStorageItemMongo : FeedbackStorageItem
    {
        [BsonId]
        public string Id;
    }

    public class MongoFeedbackStorage : IFeedbackStorage
    {
        public MongoFeedbackStorage(MongoUrl mongoUrl)
        {
            var server = mongoUrl.GetServer();
            var db = server.GetDatabase(mongoUrl.DatabaseName);
            _feedbackCollection = db.GetCollection<FeedbackStorageItemMongo>("Feedback");

            _feedbackCollection.Indexes.CreateOne(Builders<FeedbackStorageItemMongo>.IndexKeys.Ascending(_ => _.RegDate));
        }

        public long Count()
        {
            return _feedbackCollection.Count(Builders<FeedbackStorageItemMongo>.Filter.Empty);
        }

        public long Count(DateTime from, DateTime to)
        {
            return _feedbackCollection.Count(
                Builders<FeedbackStorageItemMongo>.Filter.And(
                    Builders<FeedbackStorageItemMongo>.Filter.Gte(_ => _.RegDate, from),
                    Builders<FeedbackStorageItemMongo>.Filter.Lt(_ => _.RegDate, to)
                    )
                );
        }

        public FeedbackStorageItem[] GetRange(DateTime from, DateTime to)
        {
            var filter = Builders<FeedbackStorageItemMongo>.Filter.And(
                Builders<FeedbackStorageItemMongo>.Filter.Gte(_ => _.RegDate, from),
                Builders<FeedbackStorageItemMongo>.Filter.Lt(_ => _.RegDate, to)
                );
            var cur = _feedbackCollection.Find(filter);
            return cur.ToEnumerable().ToArray();
        }

        public void Save(SendFeedback feedback, DateTime regDate)
        {
            try
            {
                _feedbackCollection.InsertOne(new FeedbackStorageItemMongo
                {
                    Id = regDate.ToString() +  Crc32.Compute(JsonConvert.SerializeObject(feedback)),
                    Feedback = feedback,
                    RegDate = regDate
                });
            }
            catch (MongoWriteException e)
            {
                if (e?.WriteError.Code == 11000)
                    return;
                throw;
            }
        }

        private IMongoCollection<FeedbackStorageItemMongo> _feedbackCollection;
    }
}
