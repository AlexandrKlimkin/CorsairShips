using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendCommon.Code.GlobalConflict.Server.Stages;
using ServerShared.GlobalConflict;
using UnityDI;

namespace BackendCommon.Code.GlobalConflict
{
    partial class Conflict
    {
#pragma warning disable 0649
        [Dependency]
        private StageFactory _stageFactory;
#pragma warning restore 0649

        Task<(StageDef Prev, StageDef Curr, TimeSpan CurrEndOffset)> GetStageDef()
        {
            var timePassed = DateTime.UtcNow - State.StartTime;
            for (var i = 0; i < State.Stages.Length; ++i)
            {
                var s = State.Stages[i];
                timePassed -= s.Period;
                if (timePassed < TimeSpan.Zero)
                    if (i > 0)
                        return Task.FromResult((State.Stages[i - 1], s, -timePassed));
                    else
                        return Task.FromResult<(StageDef Prev, StageDef Curr, TimeSpan CurrEndOffset)>((null, s, -timePassed));
            }
            return Task.FromResult<(StageDef Prev, StageDef Curr, TimeSpan CurrEndOffset)>((null, null, TimeSpan.Zero));
        }

        async Task<Stage[]> GetStage()
        {
            var def = await GetStageDef().ConfigureAwait(false);
            if (def.Curr == null)
                return new Stage[] {};
            var prev = def.Prev != null ? _stageFactory.GetStage(def.Prev.Id) : null;
            var curr = def.Curr != null ? _stageFactory.GetStage(def.Curr.Id) : null;
            var result = new List<Stage>();
            if(curr != null)
                result.Add(curr);
            if (prev != null && await prev.HasWork().ConfigureAwait(false))
                result.Insert(0, prev);

            return result.ToArray();
        }

        public async Task<(StageType info, DateTime endTime)> GetCurrentStage()
        {
            var def = await GetStageDef().ConfigureAwait(false);
            if (def.Curr == null)
                return (StageType.Unknown, default(DateTime));
            var endTime = DateTime.UtcNow + def.CurrEndOffset;
            return (def.Curr.Id, endTime);
        }

        public async Task<bool> IsCurrentStage<T>() where T : Stage
        {
            var def = await GetStageDef().ConfigureAwait(false);
            if (def.Curr == null)
                return false;
            var cs = _stageFactory.GetStage(def.Curr.Id);
            return cs is T;
        }
    }
}