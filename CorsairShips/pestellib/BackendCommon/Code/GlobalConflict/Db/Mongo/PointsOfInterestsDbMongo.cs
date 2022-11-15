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
    class PointsOfInterestsDbMongo : IPointsOfInterestsDb
    {
        private IMongoCollection<DeployedPointOfInterest> _pointsOfInterest;

        public PointsOfInterestsDbMongo()
        {
            var cs = new MongoUrl(AppSettings.Default.GlobalConflictSettings.ConnectionString);
            var client = cs.GetServer();
            var db = client.GetDatabase(cs.DatabaseName);

            _pointsOfInterest = db.GetCollection<DeployedPointOfInterest>("deployed_points_of_interest");

            _pointsOfInterest.Indexes.CreateOneAsync(Builders<DeployedPointOfInterest>.IndexKeys.Combine(
                Builders<DeployedPointOfInterest>.IndexKeys.Ascending(_ => _.Data.Id),
                Builders<DeployedPointOfInterest>.IndexKeys.Ascending(_ => _.ConflictId)
            ));
            _pointsOfInterest.Indexes.CreateOneAsync(Builders<DeployedPointOfInterest>.IndexKeys.Combine(
                Builders<DeployedPointOfInterest>.IndexKeys.Ascending(_ => _.ConflictId),
                Builders<DeployedPointOfInterest>.IndexKeys.Ascending(_ => _.TeamId)));
        }

        public async Task<PointOfInterest> GetByIdAsync(string conflictId, string poiId, string team)
        {
            var opts = new FindOptions<DeployedPointOfInterest>();
            var cursor = await
                _pointsOfInterest.FindAsync(Builders<DeployedPointOfInterest>.Filter.And(
                    Builders<DeployedPointOfInterest>.Filter.Eq(_ => _.Data.Id, poiId),
                    Builders<DeployedPointOfInterest>.Filter.Eq(_ => _.ConflictId, conflictId),
                    Builders<DeployedPointOfInterest>.Filter.Eq(_ => _.TeamId, team)), opts)
                    .ConfigureAwait(false);
            
            await cursor.MoveNextAsync().ConfigureAwait(false);
            var r = Enumerable.FirstOrDefault<DeployedPointOfInterest>(cursor.Current);
            return r?.Data;
        }

        public async Task<PointOfInterest[]> GetByTeam(string conflictId, string teamId)
        {
            var cursor = await
                _pointsOfInterest.FindAsync(Builders<DeployedPointOfInterest>.Filter.And(
                    Builders<DeployedPointOfInterest>.Filter.Eq(_ => _.ConflictId, conflictId),
                    Builders<DeployedPointOfInterest>.Filter.Eq(_ => _.TeamId, teamId))).ConfigureAwait(false);
            var result = new List<PointOfInterest>();
            while (await cursor.MoveNextAsync().ConfigureAwait(false))
            {
                result.AddRange(cursor.Current.Select(_ => _.Data));
            }
            return result.ToArray();
        }

        public async Task<PointOfInterest> GetByNode(string conflictId, string teamId, int nodeId)
        {
            var cursor =
                await _pointsOfInterest.FindAsync(Builders<DeployedPointOfInterest>.Filter.And(
                    Builders<DeployedPointOfInterest>.Filter.Eq(_ => _.ConflictId, conflictId),
                    Builders<DeployedPointOfInterest>.Filter.Eq(_ => _.TeamId, teamId),
                    Builders<DeployedPointOfInterest>.Filter.Eq(_ => _.Data.NodeId, nodeId),
                    Builders<DeployedPointOfInterest>.Filter.Gt(_ => _.Data.BonusExpiryDate, DateTime.UtcNow)
                )).ConfigureAwait(false);
            await cursor.MoveNextAsync().ConfigureAwait(false);
            var dpoi = Enumerable.FirstOrDefault<DeployedPointOfInterest>(cursor.Current);
            return dpoi?.Data;
        }

        public Task InsertAsync(string conflictId, string playerId, string team, int nodeId, PointOfInterest data, DateTime updateTime)
        {
            data.NextDeploy = updateTime + data.DeployCooldown;
            data.BonusExpiryDate = updateTime + data.BonusTime;
            data.OwnerTeam = team;
            data.NodeId = nodeId;

            return 
                _pointsOfInterest.InsertOneAsync(new DeployedPointOfInterest()
                {
                    Id = Guid.NewGuid().ToString(),
                    ConflictId = conflictId,
                    PlayerId = playerId,
                    TeamId = team,
                    UpdateTime = DateTime.UtcNow,
                    Data = data
                });
        }

        public Task UpdateAsync(string conflictId, string deployedPoiId, string team, int nodeId, DateTime updateTime, TimeSpan deployCooldown, TimeSpan bonusTime)
        {
            var filter = Builders<DeployedPointOfInterest>.Filter.And(
                Builders<DeployedPointOfInterest>.Filter.Eq(_ => _.Data.Id, deployedPoiId),
                Builders<DeployedPointOfInterest>.Filter.Eq(_ => _.ConflictId, conflictId),
                Builders<DeployedPointOfInterest>.Filter.Eq(_ => _.TeamId, team));
            return _pointsOfInterest.UpdateOneAsync(filter,
                Builders<DeployedPointOfInterest>.Update.Combine(
                    Builders<DeployedPointOfInterest>.Update.Set(_ => _.UpdateTime, updateTime),
                    Builders<DeployedPointOfInterest>.Update.Set(_ => _.Data.NextDeploy, updateTime + deployCooldown),
                    Builders<DeployedPointOfInterest>.Update.Set(_ => _.Data.BonusExpiryDate, updateTime + bonusTime),
                    Builders<DeployedPointOfInterest>.Update.Set(_ => _.Data.NodeId, nodeId)
                ));
        }

        public async Task Wipe()
        {
            await _pointsOfInterest.DeleteManyAsync(Builders<DeployedPointOfInterest>.Filter.Empty).ConfigureAwait(false);
        }
    }
}