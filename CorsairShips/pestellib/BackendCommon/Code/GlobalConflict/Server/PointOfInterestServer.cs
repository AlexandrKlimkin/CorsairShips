using System;
using System.Linq;
using System.Threading.Tasks;
using BackendCommon.Code.GlobalConflict.Db;
using BackendCommon.Code.GlobalConflict.Server.Stages;
using log4net;
using PestelLib.ServerCommon.Extensions;
using ServerShared;
using ServerShared.GlobalConflict;
using ServerShared.Sources.GlobalConflict;
using UnityDI;

namespace BackendCommon.Code.GlobalConflict.Server
{
    class PointOfInterestServer : IPointOfInterest, IPointOfInterestPrivate
    {
        private static ILog Log = LogManager.GetLogger(typeof(PointOfInterestServer));
        private readonly GlobalConflictPrivateApi _api;
#pragma warning disable 0649
        [Dependency]
        private IPointsOfInterestsDb _pointsOfInterestsDb;
#pragma warning restore 0649

        public PointOfInterestServer(GlobalConflictPrivateApi api)
        {
            _api = api;
            ContainerHolder.Container.BuildUp(this);
        }

        public Task<PointOfInterest> GetPointOfInterestAsync(string conflictId, string poiId, string team)
        {
            return _pointsOfInterestsDb.GetByIdAsync(conflictId, poiId, team);
        }

        public Task<PointOfInterest> GetPointOfInterestAsync(string conflictId, string teamId, int nodeId)
        {
            return _pointsOfInterestsDb.GetByNode(conflictId, teamId, nodeId);
        }

        public Task<PointOfInterest[]> GetTeamPointsOfInterestAsync(string conflictId, string teamId)
        {
            return _pointsOfInterestsDb.GetByTeam(conflictId, teamId);
        }

        public async Task DeployPointOfInterestAsync(string conflictId, string playerId, string team, int nodeId, string poiId)
        {
            var conflictState = await _api.ConflictsSchedulePrivateApi.GetCurrentConflictAsync().ConfigureAwait(false);
            if (conflictState == null)
            {
                throw new Exception("No conflicts are currently running");
            }
            var conflict = new Conflict(conflictState);
            if (!await conflict.IsCurrentStage<BattleStage>().ConfigureAwait(false))
            {
                throw new Exception("Not in Battle stage");
            }
            var poiT = _pointsOfInterestsDb.GetByIdAsync(conflictId, poiId, team);
            var playerT = playerId != null ? _api.PlayersPrivateApi.GetPlayerAsync(playerId, conflictId) : Task.FromResult<PlayerState>(null);
            var poisT = _api.PointOfInterestPrivateApi.GetTeamPointsOfInterestAsync(conflictId, team);
            await Task.WhenAll(poiT, playerT, poisT).ConfigureAwait(false);
            var poi = poiT.Result;
            var player = playerT.Result;
            var pois = poisT.Result;
            var newPoi = poi == null;

            if (playerId != null && team != player.TeamId)
            {
                Log.Warn($"Bad request. conflictId={conflictId}, playerId={playerId}, nodeId={nodeId}, poiId={poiId}. {team} != {player.TeamId}");
                return;
            }

            if (!newPoi && poi.NextDeploy > DateTime.UtcNow)
            {
                throw new Exception($"Point {poi.Id} deploy cooldown. Next deploy available at {poi.NextDeploy}");
            }

            var poisOnNode = pois.Where(_ => _.Id != poiId && _.NextDeploy > DateTime.UtcNow && _.NodeId == nodeId).ToArray();
            if (poisOnNode.Length + 1 > conflictState.MaxPointsOfInterestAtNode)
            {
                throw new Exception($"Node already has max points of interest {conflictState.MaxPointsOfInterestAtNode}");
            }

            var samePoisOnNodeCount = poisOnNode.Count(_ => _.Type == poi.Type);
            if (samePoisOnNodeCount + 1 > conflictState.MaxSameTypePointsOfInterestAtNode)
            {
                throw new Exception($"Node already has max points of interest {conflictState.MaxSameTypePointsOfInterestAtNode}. type {poi.Type}");
            }

            if (!conflict.IsNodeReachable(nodeId, team))
            {
                throw new Exception($"Node {nodeId} is unreachable for team {team}");
            }

            poi = conflictState.PointsOfInterest.First(_ => _.Id == poiId);
            poi = poi.Clone();


            if (!poi.IsTeamValid(player))
            {
                throw new Exception($"Team {team} can't deploy point {poiId}");
            }

            if (!poi.IsGeneralValid(player))
            {
                throw new Exception($"Player {playerId} can't deploy general point {poiId} of level {poi.GeneralLevel}. Player's general level {player.GeneralLevel}");
            }

            if (!newPoi)
            {
                await _pointsOfInterestsDb.UpdateAsync(conflictId, poi.Id, team, nodeId, DateTime.UtcNow, poi.DeployCooldown, poi.BonusTime).ConfigureAwait(false);
                return;
            }

            poi.NextDeploy = DateTime.UtcNow + poi.DeployCooldown;
            poi.BonusExpiryDate = DateTime.UtcNow + poi.BonusTime;
            poi.OwnerTeam = team;
            poi.NodeId = nodeId;

            await _pointsOfInterestsDb.InsertAsync(conflictId, playerId, team, nodeId, poi, DateTime.UtcNow).ConfigureAwait(false);
        }

        public void GetTeamPointsOfInterest(string conflictId, string teamId, Action<PointOfInterest[]> callback)
        {
            _api.PointOfInterestPrivateApi.GetTeamPointsOfInterestAsync(conflictId, teamId).ResultToCallback(callback);
        }

        public void DeployPointOfInterest(string conflictId, string playerId, string team, int nodeId, string poiId, Action<bool> callback)
        {
            //TODO: disallow deploy of auto pois
            _api.PointOfInterestPrivateApi.DeployPointOfInterestAsync(conflictId, playerId, team, nodeId, poiId)
                .ReportOnFail().ResultToCallback(callback);
        }
    }
}