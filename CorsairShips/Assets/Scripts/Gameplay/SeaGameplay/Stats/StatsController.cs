using System;
using System.Collections.Generic;
using Stats.Buffs;

namespace Stats {
    public class StatsController {
        private Dictionary<StatId, StatBase> _StatsMap;

        public StatsController(Dictionary<StatId, StatBase> stats) {
            _StatsMap = stats;
        }
        
        public bool StatExists(StatId statId) {
            return _StatsMap.ContainsKey(statId);
        }
        
        private Stat<T> GetStat<T>(StatId statId) {
            var stat = _StatsMap[statId];
            if (stat is not Stat<T> stat1)
                throw new ArgumentException($"stat {statId} type of value is {stat.GetType()}, requested type was {typeof(Stat<T>)}");
            return stat1;
        }

        public T GetBuffedStatValue<T>(StatId statId) {
            return GetStat<T>(statId).BuffedValue;
        }
        
        public T GetRawStatValue<T>(StatId statId) {
            return GetStat<T>(statId).RawValue;
        }
        
        public void AddBuffToStat(StatId statId, BuffBase buff) {
            if (_StatsMap.TryGetValue(statId, out var stat))
                stat.AddBuff(buff);
        }

        public void RemoveBuffFromStat(StatId statId, BuffBase buff) {
            if (_StatsMap.TryGetValue(statId, out var stat))
                stat.RemoveBuff(buff);
        }
    }
}