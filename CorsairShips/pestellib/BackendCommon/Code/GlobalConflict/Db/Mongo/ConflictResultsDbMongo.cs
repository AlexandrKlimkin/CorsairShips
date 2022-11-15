using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using S;
using ServerLib;
using PestelLib.ServerCommon.Db;

namespace BackendCommon.Code.GlobalConflict.Db.Mongo
{
    class ConflictResultsDbMongo: IConflictResultsDb
    {
        protected IMongoCollection<ConflictResult> _conflict_results;

        public ConflictResultsDbMongo()
        {
            var cs = new MongoUrl(AppSettings.Default.GlobalConflictSettings.ConnectionString);
            var client = cs.GetServer();
            var db = client.GetDatabase(cs.DatabaseName);
            _conflict_results = db.GetCollection<ConflictResult>("conflict_results");
        }

        public async Task<ConflictResult> GetResultAsync(string conflictId)
        {
            var c = await _conflict_results.FindAsync(Builders<ConflictResult>.Filter.Eq(_ => _.ConflictId, conflictId)).ConfigureAwait(false);
            await c.MoveNextAsync().ConfigureAwait(false);
            return c.Current.FirstOrDefault();
        }

        public Task<bool> InsertAsync(ConflictResult conflictResult)
        {
            return GlobalConflictMongoInitializer.InsertNew(_conflict_results.InsertOneAsync(conflictResult));
        }

        public async Task Wipe()
        {
            await _conflict_results.DeleteManyAsync(Builders<ConflictResult>.Filter.Empty).ConfigureAwait(false);
        }
    }
}