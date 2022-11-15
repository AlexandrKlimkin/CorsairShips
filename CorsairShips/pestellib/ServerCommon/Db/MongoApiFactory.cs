using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using PestelLib.ServerCommon.Db;

namespace PestelLib.ServerCommon.Db
{
    class MatchResult
    {
        [BsonId]
        public Guid MatchId;
        [BsonElement("time")]
        public DateTime CreateTime;
        [BsonElement("w")]
        public string[] Winners;
        [BsonElement("l")]
        public string[] Losers;
        [BsonElement("d")]
        public string[] Draw;
        [BsonElement("e")]
        public Dictionary<string,string> Extra;
    }

    class MongoMatchInfo : IMatchInfo
    {
        private IMongoCollection<MatchResult> MatchResults;

        public MongoMatchInfo(MongoDb db)
        {
            MatchResults = db.GetCollection<MatchResult>(nameof(MatchResults));
        }

        public void SetMatchEnd(string match, string[] winners, string[] losers, string[] draw, Dictionary<string,string> extra)
        {
            MatchResults.InsertOne(new MatchResult()
            {
                MatchId = Guid.Parse(match),
                CreateTime = DateTime.UtcNow,
                Winners = winners,
                Losers = losers,
                Draw = draw,
                Extra = extra
            });
        }

        public bool Validate(string match, S.MatchResult result, string userId)
        {
            var matchGuid = Guid.Parse(match);
            var m = MatchResults.FindSync(r => r.MatchId == matchGuid).FirstOrDefault();
            if (m == null)
                return false;
            switch (result)
            {
                case S.MatchResult.None:
                    return false;
                case S.MatchResult.Win:
                    return m.Winners.Contains(userId);
                case S.MatchResult.Lose:
                    return m.Losers.Contains(userId);
                case S.MatchResult.Draw:
                    return m.Draw.Contains(userId);
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }

        public long CleanOldMatchInfos(DateTime olderThan)
        {
            var filter = Builders<MatchResult>.Filter.Lt(_ => _.CreateTime, olderThan);
            var r = MatchResults.DeleteMany(filter);
            return r.DeletedCount;
        }
    }

    class MongoApiFactory : IApiFactory
    {
        private IMatchInfo _matchInfo;

        public MongoApiFactory(string connectionString)
        {
            var url = new MongoUrl(connectionString);
            var client = url.GetServer();
            var db = client.GetDatabase(url.DatabaseName);
            _matchInfo = new MongoMatchInfo(db);
        }

        public IMatchInfo GetMatchInfoApi()
        {
            return _matchInfo;
        }
    }
}
