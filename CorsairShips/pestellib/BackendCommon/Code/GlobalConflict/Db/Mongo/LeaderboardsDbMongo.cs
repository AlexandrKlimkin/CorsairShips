using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using ServerLib;
using ServerShared.GlobalConflict;
using PestelLib.ServerCommon.Db;

namespace BackendCommon.Code.GlobalConflict.Db.Mongo
{
    class LeaderboardsDbMongo : ILeaderboardsDb
    {
        private readonly IMongoCollection<PlayerState> _players;

        public LeaderboardsDbMongo()
        {
            var cs = new MongoUrl(AppSettings.Default.GlobalConflictSettings.ConnectionString);
            var client = cs.GetServer();
            var db = client.GetDatabase(cs.DatabaseName);
            _players = db.GetCollection<PlayerState>("players");

            _players.Indexes.CreateOneAsync(Builders<PlayerState>.IndexKeys.Combine(
                Builders<PlayerState>.IndexKeys.Descending(_ => _.ConflictId),
                Builders<PlayerState>.IndexKeys.Descending(_ => _.TeamId),
                Builders<PlayerState>.IndexKeys.Descending(_ => _.DonationPoints)));

            _players.Indexes.CreateOneAsync(Builders<PlayerState>.IndexKeys.Descending(_ => _.WinPoints));
            _players.Indexes.CreateOneAsync(Builders<PlayerState>.IndexKeys.Ascending(_ => _.RegisterTime));
        }

        public async Task<long> GetDonationTopMyPositionAsync(string conflictId, string userId, string team, int userPoints, DateTime userRegTime)
        {
            var filters = new List<FilterDefinition<PlayerState>>()
            {
                Builders<PlayerState>.Filter.Eq(_ => _.ConflictId, conflictId),
                Builders<PlayerState>.Filter.Or(
                    Builders<PlayerState>.Filter.Ne(_ => _.DonationPoints, userPoints),
                    Builders<PlayerState>.Filter.Lte(_ => _.RegisterTime, userRegTime)
                ),
                Builders<PlayerState>.Filter.Lte(_ => _.DonationPoints, userPoints)
            };
            if (team != null)
            {
                filters.Add(Builders<PlayerState>.Filter.Eq(_ => _.TeamId, team));
            }
            var count = await _players.CountAsync(Builders<PlayerState>.Filter.And(filters.ToArray())).ConfigureAwait(false);
            return count + 1;
        }

        public async Task<PlayerState[]> GetDonationTopAsync(string conflictId, string team, int page, int pageSize)
        {
            var opts = new FindOptions<PlayerState, PlayerState>() { Limit = pageSize, Skip = page, Sort = Builders<PlayerState>.Sort.Descending(_ => _.DonationPoints) };
            var baseFilter = Builders<PlayerState>.Filter.Eq(_ => _.ConflictId, conflictId);
            var finalFilter = baseFilter;
            if (team != null)
            {
                finalFilter =
                    Builders<PlayerState>.Filter.And(
                        baseFilter,
                        Builders<PlayerState>.Filter.Eq(_ => _.TeamId, team)
                    );
            }
            var cursor = await _players.FindAsync(finalFilter, opts).ConfigureAwait(false);
            var result = new List<PlayerState>();
            while (await cursor.MoveNextAsync().ConfigureAwait(false))
            {
                result.AddRange(cursor.Current);
            }
            return result.ToArray();
        }

        public async Task<long> GetWinPointsTopMyPositionAsync(string conflictId, string userId, string team, int userPoints, DateTime userRegTime)
        {
            var filters = new List<FilterDefinition<PlayerState>>
            {
                Builders<PlayerState>.Filter.Eq(_ => _.ConflictId, conflictId),
                Builders<PlayerState>.Filter.Ne(_ => _.Id, userId),
                Builders<PlayerState>.Filter.Or(
                    Builders<PlayerState>.Filter.Ne(_ => _.WinPoints, userPoints),
                    Builders<PlayerState>.Filter.Lte(_ => _.RegisterTime, userRegTime)
                ),
                Builders<PlayerState>.Filter.Lte(_ => _.WinPoints, userPoints)
            };
            if(team != null)
                filters.Add(Builders<PlayerState>.Filter.Eq(_ => _.TeamId, team));
            var count = await _players.CountAsync(Builders<PlayerState>.Filter.And(filters.ToArray())).ConfigureAwait(false);
            return count + 1;
        }

        public async Task<PlayerState[]> GetWinPointsTopAsync(string conflictId, string team, int page, int pageSize)
        {
            var opts = new FindOptions<PlayerState, PlayerState>() { Limit = pageSize, Skip = page, Sort = Builders<PlayerState>.Sort.Descending(_ => _.WinPoints) };
            var baseFilter = Builders<PlayerState>.Filter.Eq(_ => _.ConflictId, conflictId);
            var finalFilter = baseFilter;
            if (team != null)
            {
                finalFilter = Builders<PlayerState>.Filter.And(
                    baseFilter,
                    Builders<PlayerState>.Filter.Eq(_ => _.TeamId, team));
            }
            var cursor = await _players.FindAsync(finalFilter, opts).ConfigureAwait(false);
            var result = new List<PlayerState>();
            while (await cursor.MoveNextAsync().ConfigureAwait(false))
            {
                result.AddRange(cursor.Current);
            }
            return result.ToArray();
        }
    }
}