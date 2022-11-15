using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict.Server.Stages
{
    public class BattleStage : Stage
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BattleStage));
        private BattlePointOfInterestsDeployer _poiDeployer = new BattlePointOfInterestsDeployer();

        public override async Task<bool> HasWork()
        {
            var result = await _api.BattlePrivateApi.UnprocessedBattlesAsync(0, 1).ConfigureAwait(false);
            if (result.Length > 0)
                return true;
            var conflictState = await _api.ConflictsSchedulePrivateApi.GetCurrentConflictAsync().ConfigureAwait(false);
            return await _poiDeployer.HasWork(conflictState).ConfigureAwait(false);
        }

        public override async Task<bool> Update()
        {
            using (await _lockManager.LockAsync(StageLock, 0, false).ConfigureAwait(false))
            {
                var battlesT = _api.BattlePrivateApi.UnprocessedBattlesAsync(0, 1000);
                var conflictState = await _api.ConflictsSchedulePrivateApi.GetCurrentConflictAsync().ConfigureAwait(false);
                var conflict = new Conflict(conflictState);
                var playersToSave = new List<PlayerState>();
                var playersCache = new Dictionary<string, PlayerState>();

                var t = _poiDeployer.AutoDeployPointsOfInterests(conflict);
                await Task.WhenAll(t, battlesT).ConfigureAwait(false);
                var battles = battlesT.Result;
                try
                {
                    for (var i = 0; i < battles.Length; ++i)
                    {
                        try
                        {
                            var b = battles[i];
                            if (!playersCache.TryGetValue(b.PlayerId, out var playerState))
                                playersCache[b.PlayerId] = playerState = await _api.PlayersPrivateApi.GetPlayerAsync(b.PlayerId, b.ConflictId).ConfigureAwait(false);
                            var player = new Player(playerState);
                            var nodeState = conflict.GetNode(b.NodeId);
                            var node = new Node(nodeState);
                            var teamState = conflict.GetTeamState(playerState.TeamId);
                            var team = new Team(teamState);
                            var support = 0f;
                            var points = 0;
                            var playerBonus = player.GetTeamPointsBonus(b.Win);
                            var teamBonus = team.GetTeamPointsBonus(b.Win);
                            var nodePoi = await _api.PointOfInterestPrivateApi.GetPointOfInterestAsync((string) conflictState.Id, (string) playerState.TeamId, (int) b.NodeId).ConfigureAwait(false);
                            var nodeBonus = nodePoi != null ? (float)PointOfInterestExtensions.GetBonus(nodePoi, PointOfInterestServerLogic.CaptureInterest) : 0f;
                            if (b.Win)
                            {
                                support += conflict.SupportBonus(b.NodeId, playerState.TeamId);
                                points = (int)(nodeState.WinPoints * b.WinMod *
                                                (decimal)(1f + support + playerBonus + teamBonus + nodeBonus));
                            }
                            else
                            {
                                points = (int)(nodeState.LosePoints * b.LoseMod *
                                                (decimal)(1f + support + playerBonus + teamBonus + nodeBonus));
                            }
                            playerState.WinPoints += points;
                            node.AddTeamPoints(playerState.TeamId, points);
                            teamState.WinPoints += points;
                            if (!playersToSave.Contains(playerState))
                                playersToSave.Add(playerState);
                        }
                        catch (Exception e)
                        {
                            Log.Error("Battle result processing failed", e);
                        }
                    }
                }
                finally
                {
                    await _api.BattlePrivateApi.BattlesProcessedAsync(battles).ConfigureAwait(false);
                }
                
                // evaluate nodes state if can
                EvaluateNodes(conflict, conflictState);

                // save state
                await _api.ConflictsSchedulePrivateApi.SaveAsync(conflictState).ConfigureAwait(false);
                for (var i = 0; i < playersToSave.Count; i++)
                {
                    var p = playersToSave[i];
                    await _api.PlayersPrivateApi.SaveAsync(p).ConfigureAwait(false);
                }

            }
            return true;
        }

        private void EvaluateNodes(Conflict conflict, GlobalConflictState conflictState)
        {
            var round = conflict.CurrentRound();
            var isNewRound = round != conflictState.LastRound;
            if (isNewRound || conflictState.CaptureTime < 1)
            {
                if (isNewRound)
                    conflictState.LastRound = round;
                foreach (var nodeState in conflictState.Map.Nodes)
                {
                    var node = new Node(nodeState);
                    var prev = nodeState.NodeStatus;
                    if (node.EvaluateNodeStatus())
                    {
                        Log.Debug($"{conflictState.Id} node state changed {prev}->{nodeState.NodeStatus} at round {round}");
                    }
                }
            }
        }
    }
}