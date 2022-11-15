using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using ReportPlayersProtocol;
using PestelLib.ServerCommon.Db;

namespace ReportPlayers
{
    public class MongoReportsStorage : IReportsStorage
    {
        private IMongoCollection<PlayerReportData> _reports;
        private IMongoCollection<PlayerCounterData> _sessionCounters;

        public MongoReportsStorage(string mongoConnectionString)
        {
            void SetupStringSerializationForGuid<TClass, TMember>(BsonClassMap<TClass> m,
                Expression<Func<TClass, TMember>> memberLambda)
            {
                var member = m.GetMemberMap(memberLambda);
                var serializer = member.GetSerializer();
                var serializerConfigurable = (IRepresentationConfigurable)serializer;
                var newSerializer = serializerConfigurable.WithRepresentation(BsonType.String);
                member.SetSerializer(newSerializer);
            }

            void PrepareReportsCollection(MongoDb mongoDatabase)
            {
                if (!BsonClassMap.IsClassMapRegistered(typeof(PlayerReportData)))
                {
                    var map = BsonClassMap.RegisterClassMap<PlayerReportData>(cm =>
                    {
                        cm.AutoMap();
                        cm.MapMember(c => c.Timestamp).SetSerializer(new DateTimeSerializer(DateTimeKind.Utc));
                        cm.SetIgnoreExtraElements(true); //ignore _id field
                    });

                    SetupStringSerializationForGuid(map, _ => _.Sender);
                    SetupStringSerializationForGuid(map, _ => _.Reported);
                }

                _reports = mongoDatabase.GetCollection<PlayerReportData>("Reports");

                _reports.Indexes.CreateOneAsync(Builders<PlayerReportData>.IndexKeys.Ascending(_ => _.ReportedBySystem));
                _reports.Indexes.CreateOneAsync(Builders<PlayerReportData>.IndexKeys.Ascending(_ => _.Type));
                _reports.Indexes.CreateOneAsync(Builders<PlayerReportData>.IndexKeys.Ascending(_ => _.Timestamp));
                _reports.Indexes.CreateOneAsync(Builders<PlayerReportData>.IndexKeys.Ascending(_ => _.GamePayload));

                _reports.Indexes.CreateOneAsync(Builders<PlayerReportData>.IndexKeys.Combine(
                    Builders<PlayerReportData>.IndexKeys.Ascending(_ => _.Sender),
                    Builders<PlayerReportData>.IndexKeys.Ascending(_ => _.Reported))
                );
            }

            void PrepareSessionCounterCollection(MongoDb mongoDatabase)
            {
                if (!BsonClassMap.IsClassMapRegistered(typeof(PlayerCounterData)))
                {
                    var map = BsonClassMap.RegisterClassMap<PlayerCounterData>(cm =>
                    {
                        cm.AutoMap();
                        cm.MapIdField(_ => _.PlayerGuid);
                    });

                    SetupStringSerializationForGuid(map, _ => _.PlayerGuid);
                }

                _sessionCounters = mongoDatabase.GetCollection<PlayerCounterData>("Sessions");

                _sessionCounters.Indexes.CreateOneAsync(Builders<PlayerCounterData>.IndexKeys.Ascending(_ => _.SessionCounter));
                _sessionCounters.Indexes.CreateOneAsync(Builders<PlayerCounterData>.IndexKeys.Ascending(_ => _.ReportsCounter));
                _sessionCounters.Indexes.CreateOneAsync(Builders<PlayerCounterData>.IndexKeys.Ascending(_ => _.ReportsPerSession));
                _sessionCounters.Indexes.CreateOneAsync(Builders<PlayerCounterData>.IndexKeys.Ascending(_ => _.PlayerGuid));
                _sessionCounters.Indexes.CreateOneAsync(Builders<PlayerCounterData>.IndexKeys.Ascending(_ => _.PlayerName));
                _sessionCounters.Indexes.CreateOneAsync(Builders<PlayerCounterData>.IndexKeys.Ascending(_ => _.Whitelisted));
            }

            var client = new MongoUrl(mongoConnectionString).GetServer();
            var database = client.GetDatabase("database_android");

            PrepareReportsCollection(database);
            PrepareSessionCounterCollection(database);
        }

        
        public async Task<IEnumerable<PlayerReportData>> GetReportsByReportedAsync(Guid reported, int limit)
        {
            var results = new List<PlayerReportData>(limit);

            var filter = Builders<PlayerReportData>.Filter.Eq(_ => _.Reported, reported);

            await _reports.Find(filter)
                .Limit(limit)
                .Sort(Builders<PlayerReportData>.Sort.Descending(x => x.Timestamp))
                .ForEachAsync(result => results.Add(result));

            return results;
        }

        public async Task<IEnumerable<PlayerCounterData>> GetCheatersTopAsync(int limit)
        {
            var results = new List<PlayerCounterData>(limit);
            
            var filter = Builders<PlayerCounterData>.Filter.And(
                Builders<PlayerCounterData>.Filter.Gt(_ => _.SessionCounter, 10),
                Builders<PlayerCounterData>.Filter.Ne(_ => _.Whitelisted, true)
            );

            await _sessionCounters.Find(filter)
                .Limit(limit)
                .Sort(Builders<PlayerCounterData>.Sort.Descending(x => x.ReportsCounter))
                .ForEachAsync(result => results.Add(result));

            return results;
        }

        public void AddUserToWhitelist(Guid playerId)
        {
            SetWhitelist(playerId, true);
        }

        public void RemoveUserFromWhitelist(Guid playerId)
        {
            SetWhitelist(playerId, false);
        }

        private void SetWhitelist(Guid playerId, bool whitelist)
        {
            var filter = Builders<PlayerCounterData>.Filter.Eq(_ => _.PlayerGuid, playerId);
            var update = Builders<PlayerCounterData>.Update
                .Set(x => x.Whitelisted, whitelist);

            var options = new FindOneAndUpdateOptions<PlayerCounterData, PlayerCounterData> {IsUpsert = true};

            _sessionCounters.FindOneAndUpdate(filter, update, options);
        }

        public void RemoveReportsByReported(Guid reported, bool wipeAllReports)
        {
            if (wipeAllReports)
            {
                var filter = Builders<PlayerReportData>.Filter.Eq(_ => _.Reported, reported);
                _reports.DeleteMany(filter);
            }

            var filterCounter = Builders<PlayerCounterData>.Filter.Eq(_ => _.PlayerGuid, reported);
            var update = Builders<PlayerCounterData>.Update
                .Set(x => x.ReportsCounter, 0)
                .Set(x => x.ReportsPerSession, 0f);
            var options = new FindOneAndUpdateOptions<PlayerCounterData, PlayerCounterData> { IsUpsert = true };
            _sessionCounters.FindOneAndUpdate(filterCounter, update, options);
        }

        public void RegisterReport(PlayerReportData report)
        {
            var filter = Builders<PlayerReportData>.Filter.And(
                Builders<PlayerReportData>.Filter.Eq(_ => _.Sender, report.Sender),
                Builders<PlayerReportData>.Filter.Eq(_ => _.Reported, report.Reported));

            var update = Builders<PlayerReportData>.Update.Combine(
                Builders<PlayerReportData>.Update.Set(_ => _.Type, report.Type),
                Builders<PlayerReportData>.Update.Set(_ => _.Description, report.Description),
                Builders<PlayerReportData>.Update.Set(_ => _.Timestamp, report.Timestamp),
                Builders<PlayerReportData>.Update.Set(_ => _.ReportedBySystem, report.ReportedBySystem),
                Builders<PlayerReportData>.Update.Set(_ => _.GamePayload, report.GamePayload));

            _reports.UpdateOne(filter, update, new UpdateOptions
            {
                IsUpsert = true
            });

            if (!report.ReportedBySystem)
            {
                IncrementReportsCounter(report.Reported);
            }
        }

        public void IncrementSessionCounter(Guid playerId, string playerName)
        {
            var filter = Builders<PlayerCounterData>.Filter.Eq(_ => _.PlayerGuid, playerId);
            var update = Builders<PlayerCounterData>.Update
                .Inc(x => x.SessionCounter, 1)
                .Set(x => x.PlayerName, playerName);

            var options = new FindOneAndUpdateOptions<PlayerCounterData, PlayerCounterData> {IsUpsert = true};

            _sessionCounters.FindOneAndUpdate(filter, update, options);
        }

        public PlayerCounterData GetCounterData(Guid playerId)
        {
            return _sessionCounters
                .FindSync(rd => rd.PlayerGuid == playerId)
                .FirstOrDefault();
        }

        private void IncrementReportsCounter(Guid reportedPlayerId)
        {
            var filter = Builders<PlayerCounterData>.Filter.Eq(_ => _.PlayerGuid, reportedPlayerId);
            var update = Builders<PlayerCounterData>.Update.Inc(x => x.ReportsCounter, 1);
            var options = new FindOneAndUpdateOptions<PlayerCounterData, PlayerCounterData> { IsUpsert = true };

            var result = _sessionCounters.FindOneAndUpdate(filter, update, options);
            
            if (result != null)
            {
                //Тут возвращается результат до применения инкремента, так что увеличиваем его
                result.ReportsCounter++;

                var reportsPerSession = (float) result.ReportsCounter / result.SessionCounter;
                if (float.IsInfinity(reportsPerSession))
                {
                    reportsPerSession = 0;
                }
                SetReportsPerSession(reportsPerSession, reportedPlayerId);
            }
            else
            {
                SetReportsPerSession(0, reportedPlayerId);
            }
        }

        private void SetReportsPerSession(float rate, Guid playerId)
        {
            var filter = Builders<PlayerCounterData>.Filter.Eq(_ => _.PlayerGuid, playerId);
            var update = Builders<PlayerCounterData>.Update.Set(x => x.ReportsPerSession, rate); 
            var options = new FindOneAndUpdateOptions<PlayerCounterData, PlayerCounterData> { IsUpsert = true };

            _sessionCounters.FindOneAndUpdate(filter, update, options);
        }
    }
}