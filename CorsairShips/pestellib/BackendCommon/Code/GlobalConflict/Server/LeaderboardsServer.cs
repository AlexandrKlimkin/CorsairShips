using System;
using System.Threading.Tasks;
using BackendCommon.Code.GlobalConflict.Db;
using Microsoft.Extensions.Caching.Memory;
using PestelLib.ServerCommon.Extensions;
using ServerLib;
using ServerShared.GlobalConflict;
using UnityDI;

namespace BackendCommon.Code.GlobalConflict.Server
{
    class LeaderboardsServer : ILeaderboards, ILeaderboardsPrivate
    {
        private readonly GlobalConflictPrivateApi _api;
#pragma warning disable 0649
        [Dependency]
        private ILeaderboardsDb _leaderboardsDb;
#pragma warning restore 0649

        public LeaderboardsServer(GlobalConflictPrivateApi api)
        {
            _api = api;
            ContainerHolder.Container.BuildUp(this);
        }

        public async Task<long> GetDonationTopMyPositionAsync(string userId, bool myTeamOnly, string conflictId)
        {
            var p = await _api.PlayersPrivateApi.GetPlayerAsync(userId, conflictId).ConfigureAwait(false);
            var team = myTeamOnly ? p.TeamId : null;
            return await _leaderboardsDb.GetDonationTopMyPositionAsync(conflictId, userId, team, p.DonationPoints, p.RegisterTime).ConfigureAwait(false);
        }

        public Task<PlayerState[]> GetDonationTopAsync(string conflictId, string teamId, int page, int pageSize)
        {
            return _leaderboardsDb.GetDonationTopAsync(conflictId, teamId, page, pageSize);
        }

        public async Task<long> GetWinPointsTopMyPositionAsync(string userId, bool myTeamOnly, string conflictId)
        {
            var p = await _api.PlayersPrivateApi.GetPlayerAsync(userId, conflictId).ConfigureAwait(false);
            var team = myTeamOnly ? p.TeamId : null;
            return await _leaderboardsDb.GetWinPointsTopMyPositionAsync(conflictId, userId, team, p.WinPoints, p.RegisterTime).ConfigureAwait(false);
        }

        private PlayerState[] GetWinPointsTopFromDb(string conflictId, string team, int page, int pageSize)
        {
            return _leaderboardsDb.GetWinPointsTopAsync(conflictId, team, page, pageSize).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public Task<PlayerState[]> GetWinPointsTopAsync(string conflictId, string team, int page, int pageSize)
        {
            return _leaderboardsDb.GetWinPointsTopAsync(conflictId, team, page, pageSize);
        }

        public void GetDonationTopMyPosition(string userId, bool myTeamOnly, string conflictId, Action<long> callback)
        {
            _api.LeaderboardsPrivateApi.GetDonationTopMyPositionAsync(userId, myTeamOnly, conflictId).ResultToCallback(callback);
        }

        public void GetDonationTop(string conflictId, string team, int page, int pageSize, Action<PlayerState[]> callback)
        {
            _api.LeaderboardsPrivateApi.GetDonationTopAsync(conflictId, team, page, pageSize).ResultToCallback(callback);
        }

        public void GetWinPointsTopMyPosition(string userId, bool myTeamOnly, string conflictId, Action<long> callback)
        {
            _api.LeaderboardsPrivateApi.GetWinPointsTopMyPositionAsync(userId, myTeamOnly, conflictId).ResultToCallback(callback);
        }

        public void GetWinPointsTop(string conflictId, string teamId, int page, int pageSize, Action<PlayerState[]> callback)
        {
            _api.LeaderboardsPrivateApi.GetWinPointsTopAsync(conflictId, teamId, page, pageSize).ResultToCallback(callback);
        }
    }
}