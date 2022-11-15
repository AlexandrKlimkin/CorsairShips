using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerShared.Sources.Statistics
{
    public struct PeriodicAverageSnapshot
    {
        public long Min;
        public long Max;
        public long Med;
        public long Perc75;
        public long Perc90;
        public long Perc95;
        public long Count;
    }

    public class PeriodicAverageSimple
    {
        private readonly TimeSpan _period;

        public PeriodicAverageSimple(TimeSpan period, int maxItems)
        {
            _period = period;
            _maxItems = maxItems;
        }

        public void Add(long val)
        {
            if(_items.Count >= _maxItems)
                return;
            lock (_items)
            {
                if (_items.Count >= _maxItems)
                    return;
                _items.Add(new Wrap() {Time = DateTime.UtcNow, Value = val});
                ++_count;
            }
        }

        public PeriodicAverageSnapshot GetSnapshot()
        {
            var result = new PeriodicAverageSnapshot();
            var dt = DateTime.UtcNow;
            lock (_items)
            {
                _count -= _items.RemoveAll(_ => dt - _.Time > _period);
                if (_items.Any())
                {
                    _items.Sort(PeriodicAverageSnapshotComparer.Shared);
                    result.Min = _items.Select(_ => _.Value).Min();
                    result.Max = _items.Select(_ => _.Value).Max();
                    result.Med = _items[_items.Count / 2].Value;
                    result.Perc75 = _items[(int) (_items.Count * 0.75)].Value;
                    result.Perc90 = _items[(int) (_items.Count * 0.90)].Value;
                    result.Perc95 = _items[(int) (_items.Count * 0.95)].Value;
                    result.Count = _count;
                }
            }

            return result;
        }

        struct Wrap
        {
            public long Value;
            public DateTime Time;
        }

        class PeriodicAverageSnapshotComparer : IComparer<Wrap>
        {
            public int Compare(Wrap x, Wrap y)
            {
                return x.Value.CompareTo(y.Value);
            }

            public static readonly PeriodicAverageSnapshotComparer Shared = new PeriodicAverageSnapshotComparer();
        }

        private List<Wrap> _items = new List<Wrap>();
        private int _maxItems;
        private int _count;
    }
}
