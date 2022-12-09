namespace Stats.Buffs {
    public abstract class BuffBase {
        public StatBase StatContext;
        public abstract int Priority { get; }
    }
}