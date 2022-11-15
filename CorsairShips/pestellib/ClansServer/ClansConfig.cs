using System;
using System.Collections.Generic;
using PestelLib.ServerCommon.Config;

namespace ClansServerLib
{

    class ClansConfig
    {
        public int Port = 10000;
        public TimeSpan TreasuryAuditHistoryTTL = TimeSpan.FromDays(180);
        public TimeSpan[] DonationsAggregateByPeriod = new TimeSpan[] { TimeSpan.FromDays(7) };
        public string DbConnectionString = "mongodb://localhost/Clans";
        public bool AllowDuplicatesForClanName = false;
        public bool AllowDuplicatesForClanTag = false;
        public bool AllowChangeClanName = false;
        public bool AllowChangeClanTag = false;
        public TimeSpan BackgroundWorkerPeriod = TimeSpan.FromSeconds(ClansConfigCache.MinBackgroundWorkeerPeriodSec);
        public TimeSpan BoosterCleanupDelay = TimeSpan.FromDays(7);
        public bool EnableBoosterHistory = true;
        public int TopClansAmount = 50;
        public TimeSpan TopClanCacheTTL = TimeSpan.Zero; // Zero - определяется автоматически как х20 от времени запроса топа
        public int SearchByNameTagLimit = 30;
        public int SearchByExactParamsLimit = 30;
        public int SearchByParamsLimit = 30;
        public string TokenStorageConnectionString = "mongodb://localhost/database_android";
        public long DonationsAuditMainCacheSize = 0x40000000;
        public long DonationsAuditSecondaryCacheSize = 0x20000000;
        public TimeSpan DonationsAuditCacheItemTTL = TimeSpan.FromMinutes(30);
        public long ClanRecordCacheSize = 0x40000000;
        public TimeSpan ClanRecordCacheTTL = TimeSpan.FromHours(1);
        public string MessageQueueConnectionString = "rabbitmq,localhost";
        public string MessageQueueAppId = "ClansInternalServer";
        public bool AllMembersCanSetClanRating = false;
        // если меняете эти значения на существующий базе нужно сделать вайп или накатить миграцию в lowerCase
        public bool NameCaseSensitive = false;
        public bool TagCaseSensitive = false;
        public bool AllClanMembersCanTakeConsumables = false; // по умолчанию только кланлидер может распоряжаться consumables. добавлять consumables могут все мемберы клана
    }

    static class ClansConfigCache
    {

        public static ClansConfig Get()
        {
            if (_inst == null)
            {
                _inst = SimpleJsonConfigLoader.LoadConfigFromFile<ClansConfig>("ClansConfig.json", true);
                Validate(_inst);
            }

            return _inst;
        }

        private static void Validate(ClansConfig config)
        {
            if (config.TreasuryAuditHistoryTTL.TotalSeconds < MinAggregatedPeriodSec)
            {
                System.Diagnostics.Debug.Assert(false);
                throw new Exception($"Invalid config. Treasury audit TTL is to short. Min allowed = {MinAggregatedPeriodSec}.");
            }

            var periods = new HashSet<int>();
            foreach (var span in config.DonationsAggregateByPeriod)
            {
                if (span.TotalSeconds < MinAggregatedPeriodSec)
                {
                    System.Diagnostics.Debug.Assert(false);
                    throw new Exception($"Invalid config. Aggregation period {span} is to small. Min allowed = {MinAggregatedPeriodSec}.");
                }

                var secPeriod = (int) span.TotalSeconds;
                if (periods.Contains(secPeriod))
                {
                    System.Diagnostics.Debug.Assert(false);
                    throw new Exception("Invalid config. Min unit of aggregation period is 1 second. Your periods contains duplicates.");
                }
            }

            if (config.BackgroundWorkerPeriod.TotalSeconds < MinBackgroundWorkeerPeriodSec)
            {
                System.Diagnostics.Debug.Assert(false);
                throw new Exception($"Invalid config. Background worker period {config.BackgroundWorkerPeriod} is to small. Min allowed = {MinBackgroundWorkeerPeriodSec}.");
            }
        }

        internal const double MinBackgroundWorkeerPeriodSec = 60;
        internal const double MinAggregatedPeriodSec = 5.0 * 60;
        private static ClansConfig _inst;
    }
}
