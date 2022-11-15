using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using ServerLib;
using PestelLib.ServerCommon.Db;

namespace BackendCommon.Code.GlobalConflict.Db.Mongo
{
#pragma warning disable 0649
    class BattleResultInfoMongo : BattleResultInfo
    {
        public bool Processed;
    }
#pragma warning restore 0649

    class BattleDbMongo : IBattleDb
    {
        protected IMongoCollection<BattleResultInfoMongo> _battle_results;

        public BattleDbMongo()
        {
            var cs = new MongoUrl(AppSettings.Default.GlobalConflictSettings.ConnectionString);
            var client = cs.GetServer();
            var db = client.GetDatabase(cs.DatabaseName);
            _battle_results = db.GetCollection<BattleResultInfoMongo>("battle_results");

            _battle_results.Indexes.CreateOneAsync(Builders<BattleResultInfoMongo>.IndexKeys.Ascending(_ => _.Processed));
            _battle_results.Indexes.CreateOneAsync(Builders<BattleResultInfoMongo>.IndexKeys.Ascending(_ => _.ConflictId));
        }

        public Task InsertAsync(string conflictId, string playerId, int nodeId, bool win, decimal winMod, decimal loseMod)
        {
            var result = new BattleResultInfoMongo()
            {
                ConflictId = conflictId,
                PlayerId = playerId,
                NodeId = nodeId,
                Win = win,
                WinMod = winMod,
                LoseMod = loseMod
            };
            return _battle_results.InsertOneAsync(result);
        }

        public async Task<BattleResultInfo[]> GetUnprocessedAsync(string conflictId, int page, int batchSize)
        {
            var opts = new FindOptions<BattleResultInfoMongo>() { Skip = page * batchSize, Limit = batchSize };
            var cursor = await
                _battle_results.FindAsync(Builders<BattleResultInfoMongo>.Filter.And(
                    Builders<BattleResultInfoMongo>.Filter.Eq(_ => _.ConflictId, conflictId),
                    Builders<BattleResultInfoMongo>.Filter.Eq(_ => _.Processed, false)
                ), opts).ConfigureAwait(false);
            var result = new List<BattleResultInfo>();
            while (await cursor.MoveNextAsync().ConfigureAwait(false))
            {
                result.AddRange(cursor.Current);
            }
            return result.ToArray();
        }

        public Task MarkProcessedAsync(BattleResultInfo[] results)
        {
            var ids = results.Select(_ => _.Id).ToArray();
            return _battle_results.UpdateManyAsync(Builders<BattleResultInfoMongo>.Filter.In(_ => _.Id, ids),
                Builders<BattleResultInfoMongo>.Update.Set(_ => _.Processed, true));
        }

        public async Task Wipe()
        {
            await _battle_results.DeleteManyAsync(Builders<BattleResultInfoMongo>.Filter.Empty).ConfigureAwait(false);
        }
    }
}