using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using PestelLib.ServerShared;
using System;
using System.Text;

namespace PestelLib.ServerCommon.Db.Mongo
{
    public class MongoQuartzConfig : IQuartzConfig
    {
        static MongoQuartzConfig()
        {
            AppId = Md5.MD5string(Encoding.UTF8.GetBytes(AppPath));
            BsonClassMap.RegisterClassMap<QuartzInstance>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                cm.MapIdField(_ => _.Id);
            });
        }

        public MongoQuartzConfig(MongoUrl mongoUrl)
        {
            var server = mongoUrl.GetServer();
            var db = server.GetDatabase(mongoUrl.DatabaseName);
            _quartzInstsCollection = db.GetCollection<QuartzInstance>("QuartzInsts");
        }

        public QuartzInstance[] GetAllInstances()
        {
            var cur = _quartzInstsCollection.Find(Builders<QuartzInstance>.Filter.Empty);
            return cur.ToList().ToArray();
        }

        public bool SetQuartzState(string instanceId, bool isOn)
        {
            var filter = Builders<QuartzInstance>.Filter.Eq(_ => _.Id, instanceId);
            var update = Builders<QuartzInstance>.Update.Set(_ => _.IsOn, isOn);
            var r = _quartzInstsCollection.FindOneAndUpdate(filter, update);
            return r != null;
        }

        public bool ShouldExecuteJobs()
        {
            var filter = Builders<QuartzInstance>.Filter.And(
                Builders<QuartzInstance>.Filter.Eq(_ => _.Id, AppId),
                Builders<QuartzInstance>.Filter.Eq(_ => _.IsOn, true));
            var count = _quartzInstsCollection.Count(filter);
            if (count < 1)
            {
                var inst = 
                    new QuartzInstance
                    {
                        AppPath = AppPath,
                        IsOn = DefaultQuartsOn,
                        Id = AppId,
                        VirtualPath = AppDomain.CurrentDomain.BaseDirectory
                    };
                try
                {
                    _quartzInstsCollection.InsertOne(inst);
                }
                catch { }
            }
            return count > 0 || DefaultQuartsOn;
        }

        public void Save(QuartzInstance instance)
        {
            try
            {
                _quartzInstsCollection.InsertOne(instance);
            }
            catch { }
        }

        private IMongoCollection<QuartzInstance> _quartzInstsCollection;

        static readonly string AppPath = AppDomain.CurrentDomain.BaseDirectory;
        static readonly string AppId;
        static readonly string AppKey;
        const bool DefaultQuartsOn = false;
    }
}
