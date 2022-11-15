using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using ServerLib;
using ServerShared.GlobalConflict;
using PestelLib.ServerCommon.Db;

namespace BackendCommon.Code.GlobalConflict.Db.Mongo
{
    class ConflictPrototypesDbMongo : IConflictPrototypesDb
    {
        private IMongoCollection<GlobalConflictState> _conflictProtos;

        public ConflictPrototypesDbMongo()
        {
            var cs = new MongoUrl(AppSettings.Default.GlobalConflictSettings.ConnectionString);
            var client = cs.GetServer();
            var db = client.GetDatabase(cs.DatabaseName);
            _conflictProtos = db.GetCollection<GlobalConflictState>("proto_conflicts");
        }

        public Task<long> GetCountAsync()
        {
            return _conflictProtos.CountAsync(Builders<GlobalConflictState>.Filter.Empty);
        }

        public async Task<GlobalConflictState[]> GetProtosAsync(int page, int pageSize)
        {
            var opts = new FindOptions<GlobalConflictState>() { Limit = pageSize, Skip = page * pageSize };
            var cursor = await _conflictProtos.FindAsync(Builders<GlobalConflictState>.Filter.Empty, opts).ConfigureAwait(false);
            var result = new List<GlobalConflictState>();
            while (await cursor.MoveNextAsync().ConfigureAwait(false))
            {
                result.AddRange(cursor.Current);
            }
            return result.ToArray();
        }

        public async Task<GlobalConflictState> GetProtoAsync(string protoId)
        {
            var c = await _conflictProtos.FindAsync(Builders<GlobalConflictState>.Filter.Eq(_ => _.Id, protoId)).ConfigureAwait(false);
            c.MoveNext();
            return c.Current.FirstOrDefault();
        }

        public Task<bool> InsertAsync(GlobalConflictState proto)
        {
            return GlobalConflictMongoInitializer.InsertNew(_conflictProtos.InsertOneAsync(proto));
        }

        public async Task Remove(string protoId)
        {
            var r = await _conflictProtos.DeleteOneAsync(Builders<GlobalConflictState>.Filter.Eq(_ => _.Id, protoId)).ConfigureAwait(false);
            var c = r.DeletedCount;
        }
    }
}