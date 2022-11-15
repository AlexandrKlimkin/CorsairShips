using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using ServerLib;
using ServerShared.GlobalConflict;
using PestelLib.ServerCommon.Db;

namespace BackendCommon.Code.GlobalConflict.Db.Mongo
{
    class PlayerNameData
    {
        public string PlayerId;
        public string PlayerName;
    }

    class PlayersDbMongo : IPlayersDb
    {
        private IMongoCollection<PlayerState> _players;
        private IMongoCollection<PlayerNameData> _playerNames;

        public PlayersDbMongo()
        {
            var cs = new MongoUrl(AppSettings.Default.GlobalConflictSettings.ConnectionString);
            var client = cs.GetServer();
            var database = client.GetDatabase(cs.DatabaseName);
            _players = database.GetCollection<PlayerState>("players");
            _playerNames = database.GetCollection<PlayerNameData>("player_names");

            _players.Indexes.CreateOneAsync(Builders<PlayerState>.IndexKeys.Combine(
                Builders<PlayerState>.IndexKeys.Ascending(_ => _.Id),
                Builders<PlayerState>.IndexKeys.Ascending(_ => _.ConflictId)
            ), new CreateIndexOptions() { Unique = true });
        }

        public Task<bool> InsertAsync(PlayerState playerState)
        {
            return GlobalConflictMongoInitializer.InsertNew(_players.InsertOneAsync(playerState));
        }

        public async Task<PlayerState> GetPlayerAsync(string userId, string conflictId)
        {
            var p = await _players.FindAsync(Builders<PlayerState>.Filter.And(
                Builders<PlayerState>.Filter.Eq(_ => _.Id, userId),
                Builders<PlayerState>.Filter.Eq(_ => _.ConflictId, conflictId)
            )).ConfigureAwait(false);
            await p.MoveNextAsync().ConfigureAwait(false);
            return p.Current.FirstOrDefault();
        }

        public Task<long> GetCountTeamPlayersAsync(string conflictId, string teamId)
        {
            return _players.CountAsync(Builders<PlayerState>.Filter.And(
                Builders<PlayerState>.Filter.Eq(_ => _.ConflictId, conflictId),
                Builders<PlayerState>.Filter.Eq(_ => _.TeamId, teamId)));
        }

        public Task<long> GetCountGeneralsAsync(string conflictId)
        {
            return 
            _players.CountAsync(Builders<PlayerState>.Filter.And(
                Builders<PlayerState>.Filter.Eq(_ => _.ConflictId, conflictId),
                Builders<PlayerState>.Filter.Gt(_ => _.GeneralLevel, 0)));
        }

        public Task<long> GetCountGeneralsAsync(string conflictId, string teamId)
        {
            return _players.CountAsync(Builders<PlayerState>.Filter.And(
                Builders<PlayerState>.Filter.Eq(_ => _.ConflictId, conflictId),
                Builders<PlayerState>.Filter.Eq(_ => _.TeamId, teamId),
                Builders<PlayerState>.Filter.Gt(_ => _.GeneralLevel, 0)));
        }

        public Task IncrementPlayerDonationAsync(string conflictId, string userId, int amount)
        {
            return _players.FindOneAndUpdateAsync(Builders<PlayerState>.Filter.And(
                Builders<PlayerState>.Filter.Eq(_ => _.Id, userId),
                Builders<PlayerState>.Filter.Eq(_ => _.ConflictId, conflictId)),
                Builders<PlayerState>.Update.Inc(_ => _.DonationPoints, amount));
        }

        public Task GiveBonusesToPlayerAsync(string conflictId, string userId, params DonationBonus[] bonuses)
        {
            var updates = Builders<PlayerState>.Update.PushEach(_ => _.DonationBonuses, bonuses);
            return
                _players.FindOneAndUpdateAsync(Builders<PlayerState>.Filter.Eq(_ => _.Id, userId), updates);
        }

        public Task SaveAsync(PlayerState playerState)
        {
            return _players.ReplaceOneAsync(Builders<PlayerState>.Filter.And(
                Builders<PlayerState>.Filter.Eq(_ => _.Id, playerState.Id),
                Builders<PlayerState>.Filter.Eq(_ => _.ConflictId, playerState.ConflictId)), playerState);
        }

        public async Task Wipe()
        {
            await _players.DeleteManyAsync(Builders<PlayerState>.Filter.Empty).ConfigureAwait(false);
        }

        public async Task SetPlayerName(string userId, string name)
        {
            var r = await 
            _playerNames.FindOneAndUpdateAsync(Builders<PlayerNameData>.Filter.Eq(_ => _.PlayerId, userId),
                Builders<PlayerNameData>.Update.Set(_ => _.PlayerName, name)).ConfigureAwait(false);
            if(r != null)
                return;
            await _playerNames.InsertOneAsync(new PlayerNameData() { PlayerId = userId, PlayerName = name}).ConfigureAwait(false);
        }

        public async Task<string[]> GetPlayersNames(string[] userIds)
        {
            var playerData = new List<PlayerNameData>();
            var c = await _playerNames.FindAsync(Builders<PlayerNameData>.Filter.In(_ => _.PlayerId, userIds)).ConfigureAwait(false);
            while (await c.MoveNextAsync().ConfigureAwait(false))
            {
                playerData.AddRange(c.Current);
            }

            var idToName = playerData.ToDictionary(_ => _.PlayerId, _ => _.PlayerName);
            var result = new string[userIds.Length];
            for (var i = 0; i < userIds.Length; ++i)
            {
                idToName.TryGetValue(userIds[i], out result[i]);
            }
            return result;
        }
    }
}