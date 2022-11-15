using System;
using PestelLib.ServerShared;
using System.Timers;

namespace ServerShared
{
    public abstract class StatsGathererBase<StatT>: IDisposable
    {
        StatsClient _statsClient;
        IStatsProvider<StatT> _statsProvider;
        Timer _timer;

        public StatsGathererBase(TimeSpan gatherPeriod, StatsClient statsClient, IStatsProvider<StatT> statsProvider)
        {
            _statsClient = statsClient;
            _statsProvider = statsProvider;
            _timer = new Timer(gatherPeriod.TotalMilliseconds);
            _timer.Elapsed += (s, e) => Gather();
            _timer.Start();
        }

        private void Gather()
        {
            var s = _statsProvider.GetStats();
            PrepareAndSend(_statsClient, s);
        }

        protected abstract void PrepareAndSend(StatsClient statsClient, StatT stat);

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
    }
}
