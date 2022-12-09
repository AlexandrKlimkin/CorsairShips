namespace Stats {
    public struct StatsDescriptor {
        public readonly StatsHolderType StatsHolder;
        public readonly string Id;
        public readonly int UpgradeLevel;
        
        public StatsDescriptor(StatsHolderType statsHolder, string id, int upgradeLevel) {
            StatsHolder = statsHolder;
            Id = id;
            UpgradeLevel = upgradeLevel;
        }
    }

    public enum StatsHolderType {
        Ship,
    }
}