using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerShared;
using ServerShared.GlobalConflict;
using UnityDI;

namespace BackendCommon.Code.GlobalConflict.Server.Stages
{
    public class BattlePointOfInterestsDeployer
    {
#pragma warning disable 0649
        [Dependency]
        private GlobalConflictPrivateApi _api;
        [Dependency]
        private IPointOfInterestNodePicker _nodePicker;
#pragma warning restore 0649

        public BattlePointOfInterestsDeployer()
        {
            ContainerHolder.Container.BuildUp(this);
        }

        public async Task<bool> HasWork(GlobalConflictState conflictState)
        {
            var state = await LoadPointsState(conflictState).ConfigureAwait(false);
            var deployable = GetDeployablePois(conflictState, state);
            return deployable.Length > 0;
        }

        public virtual async Task<(PointOfInterest[] protos, DeployedPointsOfInterest deployed)> LoadPointsState(GlobalConflictState conflictState)
        {
            var deployed = new DeployedPointsOfInterest();
            var autoProtos = conflictState.PointsOfInterest.Where(_ => _.Auto && _.GeneralLevel < 1).ToArray();
            var autoPointsT = autoProtos.SelectMany(_ =>
            {
                var teams = string.IsNullOrEmpty(_.OwnerTeam) ? conflictState.Teams : new[] {_.OwnerTeam};
                return teams.Select(t =>
                    _api.PointOfInterestPrivateApi.GetPointOfInterestAsync((string) conflictState.Id, (string) _.Id, (string) t)
                        .ContinueWith(u => deployed.AddDeployed(u.Result, true)));
            }).ToArray();
            await Task.WhenAll(autoPointsT).ConfigureAwait(false);
            return (autoProtos, deployed);
        }

        public (PointOfInterest poi, List<string> teams)[] GetDeployablePois(GlobalConflictState conflictState, (PointOfInterest[] protos, DeployedPointsOfInterest deployed) state)
        {
            var result = new List<(PointOfInterest poi, List<string> teams)>();
            (var autoProtos, var deployedPoints) = state;
            for (var i = 0; i < autoProtos.Length; i++)
            {
                var ap = autoProtos[i];
                var teams = new List<string>();
                if (string.IsNullOrEmpty(ap.OwnerTeam))
                {
                    teams.AddRange(conflictState.Teams);
                }
                else
                {
                    teams.Add(ap.OwnerTeam);
                }

                teams = teams.Where(_ => !deployedPoints.HasTeamPoi(ap.Id, _)).ToList();
                if (teams.Count < 1)
                    continue;

                result.Add((ap, teams));
            }
            return result.ToArray();
        }

        public virtual async Task AutoDeployPointsOfInterests(Conflict conflict)
        {
            var state = await LoadPointsState(conflict.State).ConfigureAwait(false);
            var pois = GetDeployablePois(conflict.State, state);
            var conflictState = conflict.State;
            for (var i = 0; i < pois.Length; ++i)
                await DeployForTeams(conflictState, pois[i].poi, pois[i].teams, state.deployed).ConfigureAwait(false);
        }

        public virtual Task DeployForTeams(GlobalConflictState conflictState, PointOfInterest poi, List<string> teams, DeployedPointsOfInterest deployedPoints)
        {
            var parallel = new List<Task>();
            for (var i = 0; i < teams.Count; ++i)
            {
                var team = teams[i];
                var pickedNode = _nodePicker.PickNode(conflictState, team, poi, deployedPoints);

                if (pickedNode == null)
                    continue;

                var poiCopy = poi.Clone();
                poiCopy.OwnerTeam = team;
                poiCopy.NodeId = pickedNode.Id;
                deployedPoints.AddDeployed(poiCopy, false);

                var task = _api.PointOfInterestPrivateApi.DeployPointOfInterestAsync(conflictState.Id, null, team, pickedNode.Id, poi.Id);
                parallel.Add(task);
            }
            return Task.WhenAll(parallel);
        }
    }
}