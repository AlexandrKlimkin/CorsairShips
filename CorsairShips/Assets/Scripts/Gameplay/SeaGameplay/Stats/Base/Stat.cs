using System.Collections;
using System.Collections.Generic;
using Stats.Buffs;
using UnityEngine;

namespace Stats {
    public abstract class Stat<T> : StatBase {
        public readonly T RawValue;
        public T BuffedValue { get; private set; }
        
        public Stat(StatId id, T rawValue) {
            StatId = id;
            RawValue = rawValue;
            BuffedValue = RawValue;
        }

        public override void AddBuff(BuffBase buff) {
            
        }
        public override void RemoveBuff(BuffBase buff) {
            
        }
        
    }
}
