using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using log4net;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using ServerLib;

namespace BackendCommon.Code.Jobs
{
    public class BackgroundProcesses
    {
        public const string DefaultGroup = "group1";
        private static readonly ILog log = LogManager.GetLogger(typeof(BackgroundProcesses));
        static IScheduler _scheduler;
        static JobProfiler _jobProfiler;
        static ConcurrentDictionary<string, bool> _groupsState = new ConcurrentDictionary<string, bool>();
        static readonly IDictionary<string, object> _jobData = new Dictionary<string, object>();

        public static void Configure(IServiceProvider serviceProvider)
        {
            _jobData[nameof(IServiceProvider)] = serviceProvider;
        }

        public static IScheduler OnAppStart(AppSettings settings)
        {
            try
            {
                log.Debug("RedisBackgroundProcess started");

                //LogManager.Adapter = new Common.Logging.Simple.DebugLoggerFactoryAdapter (){ };
                //new Common.Logging.Simple.ConsoleOutLoggerFactoryAdapter { Level = Common.Logging.LogLevel.Info };

                // Grab the Scheduler instance from the Factory 
                _scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;

                // and start it off
                _scheduler.Start();

                if (settings.ProfileQuartzJobs)
                {
                    _jobProfiler = new JobProfiler();
                    _scheduler.ListenerManager.AddJobListener(_jobProfiler, GroupMatcher<JobKey>.GroupEquals(DefaultGroup));
                }

                //if(!string.IsNullOrEmpty(AppSettings.Default.PersistentStorageSettings.StorageConnectionString))

                RunMaintenanceCleanJob();

                if(settings.Leaderboard10)
                    RunUpdateChunksJob();
                if(settings.Leaderboard20)
                    RunLeaguesJob(settings.LeagueJobPeriod);
                if (settings.GloabalConflict)
                    RunGlobalConflictJob();
                if(settings.MatchInfoTTL < TimeSpan.MaxValue)
                    RunCleanOldMatchInfosJob();
                if(settings.PlayerProfileSettings.PlayerProfileActivated)
                    RunProfileCleanJob();
                if (settings.MinimizeRedisUsage)
                    RunMigrateToPersistentStorage();

                //RunUpdateSeasonsJob(scheduler);
                RunHiveHealthCheck();

                RunJobControl();

                return _scheduler;
            }
            catch (SchedulerException se)
            {
                Debug.WriteLine((object) se);
            }
            return null;
        }

        private static void RunMigrateToPersistentStorage()
        {
            CreateForeverJob<MigrateDeviceIdsToPersitantStorage>("MigrateDeviceIdsToPersitantStorage", DefaultGroup, (int)TimeSpan.FromMinutes(1).TotalSeconds, true);
            CreateForeverJob<MigrateNamesBindingsToPersistentStorage>("MigrateNamesBindingsToPersistentStorage", DefaultGroup, (int)TimeSpan.FromMinutes(1).TotalSeconds, true);
            CreateForeverJob<MigrateFacebookIdToPersistantStorage>("MigrateFacebookIdToPersistantStorage", DefaultGroup, (int)TimeSpan.FromMinutes(1).TotalSeconds, true);
            CreateForeverJob<RedisTryRemoveLegacyCollections>("RedisTryRemoveLegacyCollections", DefaultGroup, (int)TimeSpan.FromDays(1).TotalSeconds, true);
            CreateForeverJob<MigrateLastUsedDeviceIdToPersistentStorage>("MigrateLastUsedDeviceIdToPersistentStorage", DefaultGroup, (int)TimeSpan.FromMinutes(1).TotalSeconds, true);
        }

        public static void ResumeByGroup(string group)
        {
            var triggers = _scheduler.GetTriggerKeys(Quartz.Impl.Matchers.GroupMatcher<TriggerKey>.GroupEquals(group)).Result;
            foreach (var t in triggers)
            {
                if (_scheduler.GetTriggerState(t).Result == TriggerState.Paused)
                    _scheduler.ResumeTrigger(t);
            }
        }

        public static void PauseByGroup(string group)
        {
            var triggers = _scheduler.GetTriggerKeys(Quartz.Impl.Matchers.GroupMatcher<TriggerKey>.GroupEquals(group)).Result;
            foreach (var t in triggers)
            {
                if (_scheduler.GetTriggerState(t).Result != TriggerState.Paused)
                    _scheduler.PauseTrigger(t);
            }
        }

        public static void CreateForeverJob<T>(string name, string group, int period, bool pause) where T: IJob
        {
            if (_scheduler == null)
            {
                log.ErrorFormat($"Can't start job {typeof(T).Name}:{name}:{group}:{period}:{pause}. Init scheduler first");
                return;
            }

            var job = JobBuilder.Create<T>()
                .WithIdentity(name, group)
                .SetJobData(new JobDataMap(_jobData))
                .Build();

            var t = TriggerBuilder.Create()
                .WithIdentity($"{name}Trigger", group);

            if (pause)
                t = t.StartAt(SystemTime.UtcNow().AddSeconds(5));
            else
                t = t.StartNow();

            var trigger = t.WithSimpleSchedule(x =>
                x.WithIntervalInSeconds(period)
                .RepeatForever())
                .Build();

            _scheduler.ScheduleJob(job, trigger);

            if (pause)
                _scheduler.PauseTrigger(trigger.Key);
        }

        private static void RunProfileCleanJob()
        {
            CreateForeverJob<CleanPlayerProfilesJob>("clean_player_profiles", DefaultGroup, 86400, true);
        }

        private static void RunCleanOldMatchInfosJob()
        {
            CreateForeverJob<CleanOldMatchInfosJob>("clean_match_infos", DefaultGroup, 86400, true);
        }

        private static void RunGlobalConflictJob()
        {
            CreateForeverJob<GlobalConflictJob>("global_conflict", DefaultGroup, 60, true);
        }

        private static void RunLeaguesJob(int period)
        {
            if(period < 1)
                return;
            CreateForeverJob<LeaguesJob>("leagues", DefaultGroup, period, true);
        }

        private static void RunUpdateChunksJob()
        {
            CreateForeverJob<RedisRebuildLeaderboardChunks>("chunks", DefaultGroup, 60, true);
            CreateForeverJob<RedisUpdateCurrentSeasonIndex>("updateClassicSeasonIndex", DefaultGroup, 5, true);
        }

        private static void RunJobControl()
        {
            // dont start paused, this job unpauses other jobs it its allowed
            CreateForeverJob<JobsControl>("jobs_control", "control", 2, false);
        }

        private static void RunMaintenanceCleanJob()
        {
            CreateForeverJob<CleanUserMaintenanceJob>("clean", DefaultGroup, 60, true);
        }

        private static void RunHiveHealthCheck()
        {
            CreateForeverJob<HiveHealthCheckerJob>("hive_clean", DefaultGroup, 60, true);
        }
    }
}