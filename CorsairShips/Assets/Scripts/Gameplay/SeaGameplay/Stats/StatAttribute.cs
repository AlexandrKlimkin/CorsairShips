using System;

namespace Stats {
    public class StatAttribute : Attribute {
        public StatId Id;
        public Type StatType;
    }
}