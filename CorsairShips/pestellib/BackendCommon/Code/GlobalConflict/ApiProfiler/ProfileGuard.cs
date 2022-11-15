using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Backend.Code.Statistics;
using UnityDI;

namespace BackendCommon.Code.GlobalConflict.ApiProfiler
{
    public class ProfileGuard : IDisposable
    {
        private readonly string _category;
        private readonly string _counter;
        private readonly string _counterCount;
        private readonly string _ccKey;
        private readonly bool _sync;
        private readonly bool _countCalls;
        private readonly Stopwatch _sw;
        private static ConcurrentDictionary<string, long> _callCount = new ConcurrentDictionary<string, long>();

#pragma warning disable 0649
        [Dependency]
        private DefaultStatisticsClient _statisticsClient;
#pragma warning restore 0649

        public ProfileGuard(string category, string counter, bool sync = false, bool countCalls = true)
        {
            ContainerHolder.Container.BuildUp(this);
            _category = category;
            _counter = counter;
            _counterCount = counter + "_count";
            _ccKey = _category + _counter;
            _sync = sync;
            _countCalls = countCalls;
            _sw = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            if(_statisticsClient == null)
                return;

            if (_sync)
                _statisticsClient.Send(_category, _counter, _sw.ElapsedMilliseconds, true);
            else
                _statisticsClient.SendAsync(_category, _counter, _sw.ElapsedMilliseconds, true);

            if (_countCalls)
            {
                var cc = _callCount.AddOrUpdate(_ccKey, s => 1, (s, l) => l + 1);
                if (_sync)
                    _statisticsClient.Send(_category, _counterCount, cc, true);
                else
                    _statisticsClient.SendAsync(_category, _counterCount, cc, true);
            }
        }
    }
}