using System.Collections;
using System.Collections.Generic;
using Stats.Buffs;
using UnityEngine;

namespace Stats {
    public abstract class StatBase {
        public StatId StatId;
        public virtual string DisplayValue { get; }

        public abstract void AddBuff(BuffBase buff);
        public abstract void RemoveBuff(BuffBase buff);
    }
}
