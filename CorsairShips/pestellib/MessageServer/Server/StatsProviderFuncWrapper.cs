using System;
using MessageClient.Sources;
using ServerShared;

namespace MessageServer.Server
{
    public class StatsProviderFuncWrapper : IStatsProvider<BaseMessageDispatcher.DispatcherStatistics>
    {
        private readonly Func<BaseMessageDispatcher.DispatcherStatistics> _getter;

        public StatsProviderFuncWrapper(Func<BaseMessageDispatcher.DispatcherStatistics> getter)
        {
            _getter = getter;
        }

        public BaseMessageDispatcher.DispatcherStatistics GetStats()
        {
            return _getter();
        }
    }
}