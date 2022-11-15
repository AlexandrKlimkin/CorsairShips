using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendCommon.Code.GlobalConflict.Db;
using BackendCommon.Code.GlobalConflict.Server.Stages;
using log4net;
using Microsoft.Extensions.Caching.Memory;
using PestelLib.ServerCommon.Extensions;
using ServerLib;
using ServerShared.GlobalConflict;
using UnityDI;

namespace BackendCommon.Code.GlobalConflict.Server
{
    class PlayersServer : IPlayers
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PlayersServer));
        protected GlobalConflictPrivateApi _api;
#pragma warning disable 0649
        [Dependency]
        protected IPlayersDb _playersDb;
#pragma warning restore 0649

        public PlayersServer(GlobalConflictPrivateApi api)
        {
            _api = api;
            ContainerHolder.Container.BuildUp(this);
        }

        public async Task<PlayerState> RegisterAsync(string conflictId, string userId, string teamId)
        {
            var conflictState = await _api.ConflictsSchedulePrivateApi.GetConflictAsync(conflictId).ConfigureAwait(false);
            if (conflictState == null)
                return null;
            var conflict = new Conflict(conflictState);
            if (await conflict.IsCurrentStage<FinalStage>().ConfigureAwait(false))
                return null;
            var player = new PlayerState()
            {
                Id = userId,
                ConflictId = conflictId,
                TeamId = teamId
            };
            if (!await _playersDb.InsertAsync(player).ConfigureAwait(false))
            {
                Log.WarnFormat($"{player.Id} already has state in conflict {player.ConflictId}");
                return null;
            }
            _api.StatisticsClient?.SendAsync(GlobalConflictPrivateApi._statCategory, $"reg.{teamId}", 1);
            return player;
        }

        public Task<PlayerState> GetPlayerAsync(string userId, string conflictId)
        {
            return _playersDb.GetPlayerAsync(userId, conflictId);
        }

        public void SetName(string userId, string name, Action callback)
        {
            _api.PlayersPrivateApi.SetNameAsync(userId, name).ResultToCallback(callback);
        }

        public void Register(string conflictId, string userId, string teamId, Action<PlayerState> callback)
        {
            _api.PlayersPrivateApi.RegisterAsync(conflictId, userId, teamId).ResultToCallback(callback);
        }

        public void GetPlayer(string userId, string conflictId, Action<PlayerState> callback)
        {
            _api.PlayersPrivateApi.GetPlayerAsync(userId, conflictId).ResultToCallback(callback);
        }

        public void GetTeamPlayersStat(string conflict, Action<TeamPlayersStat> callback)
        {
            _api.PlayersPrivateApi.GetTeamPlayersStatAsync(conflict).ResultToCallback(callback);
        }

        public PlayerState GetPlayer(string userId, string conflictId)
        {
            return _api.PlayersPrivateApi.GetPlayerAsync(userId, conflictId).Result;
        }

        public async Task<TeamPlayersStat> GetTeamPlayersStatAsync(string conflict)
        {
            var conflictState = await _api.ConflictsSchedulePrivateApi.GetCurrentConflictAsync().ConfigureAwait(false);
            if (conflictState == null)
                return null;

            var result = new TeamPlayersStat();
            result.Teams = conflictState.Teams;
            result.PlayersCount = new int[result.Teams.Length];
            result.GeneralsCount = new int[result.Teams.Length];
            var tasks = new List<Task>();
            for (var i = 0; i < conflictState.Teams.Length; i++)
            {
                var teamId = conflictState.Teams[i];
                var teamIdx = i;
                var t = _playersDb.GetCountTeamPlayersAsync(conflictState.Id, teamId).ContinueWith(_ =>
                {
                    result.PlayersCount[teamIdx] = (int) _.Result;
                });
                tasks.Add(t);
                t = _playersDb.GetCountGeneralsAsync(conflictState.Id, teamId).ContinueWith(_ =>
                {
                    result.GeneralsCount[teamIdx] = (int) _.Result;
                });
                tasks.Add(t);
            }
            Task.WaitAll(tasks.ToArray());
            return result;
        }
    }

    class PlayersPrivateServer : PlayersServer, IPlayersPrivate
    {
        private readonly MemoryCache _nameCache = new MemoryCache(new MemoryCacheOptions());
        public Task<long> CountGeneralsAsync(string conflictId)
        {
            return _playersDb.GetCountGeneralsAsync(conflictId);
        }

        public Task AddDonationAsync(string conflictId, Donation donation)
        {
            _api.StatisticsClient?.SendAsync(GlobalConflictPrivateApi._statCategory, "donat", donation.Amount);
            return _playersDb.IncrementPlayerDonationAsync(conflictId, donation.UserId, donation.Amount);
        }

        public Task GiveBonusesToPlayerAsync(string conflictId, string userId, params DonationBonus[] bonuses)
        {
            return _playersDb.GiveBonusesToPlayerAsync(conflictId, userId, bonuses);
        }

        public Task SaveAsync(PlayerState playerState)
        {
            return _playersDb.SaveAsync(playerState);
        }

        public Task SetNameAsync(string userId, string name)
        {
            return _playersDb.SetPlayerName(userId, name);
        }

        public async Task<string[]> GetNamesAsync(string[] userIds)
        {
            var key = string.Join("", userIds);
            var fromCache = (string[]) _nameCache.Get(key);
            if (fromCache != null)
                return fromCache;
            var r = await _playersDb.GetPlayersNames(userIds).ConfigureAwait(false);
            var ttl = AppSettings.Default.GlobalConflictSettings.NameCacheTTL;
            if (ttl > TimeSpan.Zero)
            {
                var e = _nameCache.CreateEntry(key);
                e.Value = r;
                e.AbsoluteExpirationRelativeToNow = ttl;
            }

            return r;
        }

        public PlayersPrivateServer(GlobalConflictPrivateApi api) : base(api)
        {
        }
    }
}