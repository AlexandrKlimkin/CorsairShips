using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using ServerLib;
using ServerShared.GlobalConflict;
using PestelLib.ServerCommon.Db;

namespace BackendCommon.Code.GlobalConflict.Db.Mongo
{
    class ConflictsScheduleDbMongo : IConflictsScheduleDb
    {
        protected IMongoCollection<GlobalConflictState> _scheduled_conflicts;

        public ConflictsScheduleDbMongo()
        {
            var cs = new MongoUrl(AppSettings.Default.GlobalConflictSettings.ConnectionString);
            var client = cs.GetServer();
            var db = client.GetDatabase(cs.DatabaseName);
            _scheduled_conflicts = db.GetCollection<GlobalConflictState>("scheduled_conflicts");

            _scheduled_conflicts.Indexes.CreateOneAsync(Builders<GlobalConflictState>.IndexKeys.Ascending(_ => _.StartTime));
            _scheduled_conflicts.Indexes.CreateOneAsync(Builders<GlobalConflictState>.IndexKeys.Ascending(_ => _.EndTime));
        }

        public async Task<GlobalConflictState> GetByDateAsync(DateTime dateTime)
        {
            var cursor = await
                _scheduled_conflicts.FindAsync(Builders<GlobalConflictState>.Filter.And(
                    Builders<GlobalConflictState>.Filter.Lte(_ => _.StartTime, dateTime),
                    Builders<GlobalConflictState>.Filter.Gt(_ => _.EndTime, dateTime)
                )).ConfigureAwait(false);
            await cursor.MoveNextAsync().ConfigureAwait(false);
            return cursor.Current.FirstOrDefault();
        }

        public async Task<GlobalConflictState> GetOverlappedAsync(DateTime start, DateTime end)
        {
            var filter =
                Builders<GlobalConflictState>.Filter.Or(
                    Builders<GlobalConflictState>.Filter.And(
                        Builders<GlobalConflictState>.Filter.Lte(_ => _.StartTime, start),
                        Builders<GlobalConflictState>.Filter.Gt(_ => _.EndTime, start)
                    ),
                    Builders<GlobalConflictState>.Filter.And(
                        Builders<GlobalConflictState>.Filter.Lt(_ => _.StartTime, end),
                        Builders<GlobalConflictState>.Filter.Gte(_ => _.EndTime, end)
                    ),
                    Builders<GlobalConflictState>.Filter.And(
                        Builders<GlobalConflictState>.Filter.Gte(_ => _.StartTime, start),
                        Builders<GlobalConflictState>.Filter.Lte(_ => _.EndTime, end)
                    )
                );
            var cursor = await _scheduled_conflicts.FindAsync(filter).ConfigureAwait(false);
            await cursor.MoveNextAsync().ConfigureAwait(false);
            return cursor.Current.FirstOrDefault();
        }

        public Task<bool> InsertAsync(GlobalConflictState conflictState)
        {
            return GlobalConflictMongoInitializer.InsertNew(_scheduled_conflicts.InsertOneAsync(conflictState));
        }

        public Task SaveAsync(GlobalConflictState conflictState)
        {
            return _scheduled_conflicts
                .ReplaceOneAsync(Builders<GlobalConflictState>.Filter.Eq(_ => _.Id, conflictState.Id), conflictState);
        }

        public async Task<long> DeleteAsync(string scheduledConflictId, DateTime after)
        {
            var result = await _scheduled_conflicts.DeleteOneAsync(Builders<GlobalConflictState>.Filter.And(
                Builders<GlobalConflictState>.Filter.Eq(_ => _.Id, scheduledConflictId),
                Builders<GlobalConflictState>.Filter.Gt(_ => _.StartTime, after))).ConfigureAwait(false);
            return result.DeletedCount;
        }

        public Task<long> GetCountAsync()
        {
            return _scheduled_conflicts.CountAsync(Builders<GlobalConflictState>.Filter.Empty);
        }

        public async Task<GlobalConflictState[]> GetOrderedAsync(int page, int pageSize)
        {
            var opts = new FindOptions<GlobalConflictState>() { Limit = pageSize, Skip = page * pageSize, Sort = Builders<GlobalConflictState>.Sort.Ascending(_ => _.EndTime) };
            var cursor = await _scheduled_conflicts.FindAsync(Builders<GlobalConflictState>.Filter.Empty, opts).ConfigureAwait(false);
            var result = new List<GlobalConflictState>();
            while (await cursor.MoveNextAsync().ConfigureAwait(false))
            {
                result.AddRange(cursor.Current);
            }
            return result.ToArray();
        }

        public async Task<GlobalConflictState> GetByIdAsync(string conflictId)
        {
            var r = await _scheduled_conflicts.FindAsync(Builders<GlobalConflictState>.Filter.Eq(_ => _.Id, conflictId)).ConfigureAwait(false);
            r.MoveNext();
            return r.Current.FirstOrDefault();
        }

        public async Task Wipe()
        {
            await _scheduled_conflicts.DeleteManyAsync(Builders<GlobalConflictState>.Filter.Empty).ConfigureAwait(false);
        }
    }
}