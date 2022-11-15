using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using S;
using ServerShared.GlobalConflict;
using UnityDI;

namespace BackendCommon.Code.GlobalConflict
{
    public partial class Conflict
    {
        public readonly GlobalConflictState State;
        public TimeSpan Period { get; }
        public bool Started => State.StartTime < DateTime.UtcNow;
        public bool InProgress => Started && State.EndTime > DateTime.UtcNow;
        public bool Finished => Started && !InProgress;

        public Conflict(GlobalConflictState state)
        {
            ContainerHolder.Container.BuildUp(this);
            State = state;
            Period = GetPeriod();
        }

        public List<ConflictResultPoints> GetResultPointsByTeam()
        {
            return State.Map.Nodes
                .Where(_ => !string.IsNullOrEmpty(_.Owner))
                .GroupBy(_ => _.Owner)
                .Select(_ => new ConflictResultPoints(){Team = _.Key, Points = _.Sum(p => p.ResultPoints)})
                .ToList();
        }

        private TimeSpan GetPeriod()
        {
            var result = TimeSpan.Zero;
            for (var i = 0; i < State.Stages.Length; ++i)
            {
                result += State.Stages[i].Period;
            }
            return result;
        }

        public TeamState GetTeamState(string teamId)
        {
            return State.TeamsStates.FirstOrDefault(_ => _.Id == teamId);
        }

        public TeamState GetWinningTeam()
        {
            return State.TeamsStates.OrderByDescending(_ => _.WinPoints).First();
        }

        public int CurrentRound()
        {
            if (!Started || State.CaptureTime < 1)
                return 0;
            // through all stages for now, its simpler
            return (int)((DateTime.UtcNow - State.StartTime).TotalSeconds / State.CaptureTime);
        }

        public async Task Update()
        {
            var stages = await GetStage().ConfigureAwait(false);
            for (var i = 0; i < stages.Length; i++)
            {
                var s = stages[i];
                while (await s.HasWork().ConfigureAwait(false))
                {
                    await s.Update().ConfigureAwait(false);
                }
            }
        }
    }
}