using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using ShortPlayerId.DTO;
using PestelLib.ServerCommon.Db;

namespace ShortPlayerId.Storage
{
    public class ShortPlayerIdStorage
    {
        private IMongoCollection<PlayerIdBinding> _shortPlayerIds;
        private IMongoCollection<PlayerSerialNumber> _playerSerialNumber;

        public ShortPlayerIdStorage(string mongoConnectionString)
        {
            void SetupStringSerializationForGuid<TClass, TMember>(BsonClassMap<TClass> m,
                Expression<Func<TClass, TMember>> memberLambda)
            {
                var member = m.GetMemberMap(memberLambda);
                var serializer = member.GetSerializer();
                var serializerConfigurable = (IRepresentationConfigurable) serializer;
                var newSerializer = serializerConfigurable.WithRepresentation(BsonType.String);
                member.SetSerializer(newSerializer);
            }

            void PrepareSessionCounterCollection(MongoDb mongoDatabase)
            {
                if (!BsonClassMap.IsClassMapRegistered(typeof(PlayerIdBinding)))
                {
                    var map = BsonClassMap.RegisterClassMap<PlayerIdBinding>(cm =>
                    {
                        cm.AutoMap();
                        cm.MapIdField(_ => _.PlayerGuid);
                    });

                    SetupStringSerializationForGuid(map, _ => _.PlayerGuid);
                }

                _shortPlayerIds = mongoDatabase.GetCollection<PlayerIdBinding>("PlayerIdBinding");

                _shortPlayerIds.Indexes.CreateOneAsync(
                    Builders<PlayerIdBinding>.IndexKeys.Ascending(_ => _.PlayerGuid));
                _shortPlayerIds.Indexes.CreateOneAsync(
                    Builders<PlayerIdBinding>.IndexKeys.Ascending(_ => _.ShortPlayerId));
            }

            void PrepareSerialNumberCollection(MongoDb mongoDatabase)
            {
                if (!BsonClassMap.IsClassMapRegistered(typeof(PlayerSerialNumber)))
                {
                    BsonClassMap.RegisterClassMap<PlayerSerialNumber>(cm =>
                    {
                        cm.AutoMap();
                        cm.MapIdField(_ => _.Id);
                    });
                }

                _playerSerialNumber = mongoDatabase.GetCollection<PlayerSerialNumber>("PlayerSerialNumber");

                _playerSerialNumber.Indexes.CreateOneAsync(
                    Builders<PlayerSerialNumber>.IndexKeys.Ascending(_ => _.Id));
            }

            var url = new MongoUrl(mongoConnectionString);
            var client = url.GetServer();
            var database = client.GetDatabase("database_android");

            PrepareSessionCounterCollection(database);
            PrepareSerialNumberCollection(database);
        }

        public async Task<int> GetShortPlayerId(Guid playerId)
        {
            var filter = Builders<PlayerIdBinding>.Filter.Eq(_ => _.PlayerGuid, playerId);

            var data = await _shortPlayerIds.FindAsync(filter);
            var doc = await data.SingleOrDefaultAsync();
            if (doc != null)
            {
                return doc.ShortPlayerId;
            }

            return await CreateNewBinding(playerId);
        }

        private async Task<int> CreateNewBinding(Guid playerId)
        {
            var shortPlayerId = await AssignNewSerialNumber();
            await _shortPlayerIds.InsertOneAsync(new PlayerIdBinding()
            {
                ShortPlayerId = shortPlayerId,
                PlayerGuid = playerId
            });
            return shortPlayerId;
        }

        private async Task<int> AssignNewSerialNumber()
        {
            var filter = Builders<PlayerSerialNumber>.Filter.Eq(_ => _.Id, 0);
            var update = Builders<PlayerSerialNumber>.Update.Inc(x => x.SerialNumber, 1);

            var options = new FindOneAndUpdateOptions<PlayerSerialNumber, PlayerSerialNumber> { IsUpsert = true };

            var updated = await _playerSerialNumber.FindOneAndUpdateAsync(filter, update, options);
            if(update == null) // на пустой базе вернет null, но мы не хотим чтоб первый игрок был с id = 0, ReturnDocument.After мы тоже не хотим т.к. для конкурентных запросов возвращает идентичный документ
            {
                updated = await _playerSerialNumber.FindOneAndUpdateAsync(filter, update, options);
            }
            return updated.SerialNumber;
        }

        public async Task<Guid> GetFullPlayerId(int shortPlayerId)
        {
            var filter = Builders<PlayerIdBinding>.Filter.Eq(_ => _.ShortPlayerId, shortPlayerId);

            var data = await _shortPlayerIds.FindAsync(filter);
            var doc = await data.SingleOrDefaultAsync();
            if (doc == null)
            {
                throw new ArgumentOutOfRangeException(nameof(shortPlayerId));
            }

            return doc.PlayerGuid;
        }
    }
}