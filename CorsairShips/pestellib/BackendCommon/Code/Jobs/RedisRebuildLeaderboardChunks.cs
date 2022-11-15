using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendCommon.Code.Modules.ClassicLeaderboards;
using ClassicLeaderboards;
using log4net;
using PestelLib.ServerCommon.Config;
using Quartz;
using ServerLib;
using StackExchange.Redis;
using UnityDI;

namespace BackendCommon.Code.Jobs
{
    [DisallowConcurrentExecution]
    public class RedisRebuildLeaderboardChunks : IJob
    {
        private static LeaderboardConfig _leaderboardConfig;

        private static List<List<SortedSet<PlayerLeaderboardRecord>>> LeaguesChunkedLeaderboards;

        private static readonly ILog log = LogManager.GetLogger(typeof(RedisRebuildLeaderboardChunks));

        private static ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();

        private const int ChunkSize = 100;
        
        //[league][chunk]

        public static List<PlayerLeaderboardRecord> GetLeaderboardByLeagueIndex(Guid userId, int leagueIndex)
        {
            List<PlayerLeaderboardRecord> result = new List<PlayerLeaderboardRecord>();
            try
            {
                cacheLock.EnterReadLock();
                var chunkIndex = Math.Abs(userId.GetHashCode()) % LeaguesChunkedLeaderboards[leagueIndex].Count;
                result = LeaguesChunkedLeaderboards[leagueIndex][chunkIndex].ToList();
            }
            catch (Exception e)
            {
                log.Error("Can't get leaderboard for userId " + userId + " with hash " + Math.Abs(userId.GetHashCode()) + e.Message);
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
            return result;
        }

        public static List<PlayerLeaderboardRecord> GetLeaderboard(Guid userId, int score)
        {
            var leagueIndex = GetLeagueIndex(score);
            return GetLeaderboardByLeagueIndex(userId, leagueIndex);
        }
        
        private static int[] Leagues;

        static RedisRebuildLeaderboardChunks()
        {
            _leaderboardConfig = SimpleJsonConfigLoader.LoadConfig<LeaderboardConfig>();
            Leagues = _leaderboardConfig.Leagues;
        }

        public RedisRebuildLeaderboardChunks()
        {
            if (LeaguesChunkedLeaderboards == null)
            {
                cacheLock.EnterWriteLock();
                LeaguesChunkedLeaderboards = new List<List<SortedSet<PlayerLeaderboardRecord>>>();
                foreach (var league in Leagues)
                {
                    LeaguesChunkedLeaderboards.Add(new List<SortedSet<PlayerLeaderboardRecord>>());
                }
                cacheLock.ExitWriteLock();
            }
        }

        public static int GetLeagueIndex(double score)
        {
            for (var i = 0; i < Leagues.Length; i++)
            {
                if (score < Leagues[i])
                {
                    return i;
                }
            }
            return Leagues.Length - 1;
        }

        public Task Execute(IJobExecutionContext context)
        {
            var leaderboards = ContainerHolder.Container.Resolve<ILeaderboards>();
            var leaguesLeaderboards = new List<List<PlayerLeaderboardRecord>>();

            foreach (var league in Leagues)
            {
                leaguesLeaderboards.Add(new List<PlayerLeaderboardRecord>());
            }

            var pos = 0;
            var processed = 0;
            do
            {
                var chunk = leaderboards.GetTop(LeaderboardUtils.CurrentSeasonId, pos, ChunkSize);
                //все рекорды в разных листах в зависимости от их лиги
                foreach (var r in chunk)
                {
                    var leagueIndex = GetLeagueIndex(r.Score);
                    leaguesLeaderboards[leagueIndex].Add(new PlayerLeaderboardRecord(new Guid(r.UserId), r.Score));
                }
                processed = chunk.Length;
                pos += chunk.Length;
            } while (processed > 0);

            try
            {
                for (int leagueIndex = 0; leagueIndex < leaguesLeaderboards.Count; leagueIndex++)
                {
                    var leagueBoard = leaguesLeaderboards[leagueIndex];
                    var currentLeagueChunks = new List<SortedSet<PlayerLeaderboardRecord>>();
                    
                    var chunksAmount = leagueBoard.Count/ChunkSize + 1;
                    for (var i = 0; i < chunksAmount; i++)
                    {
                        currentLeagueChunks.Add(new SortedSet<PlayerLeaderboardRecord>(new PlayerLeaderboardRecordComparer()));
                    }

                    foreach (var record in leagueBoard)
                    {
                        var chunkIndex = Math.Abs(record.UserId.GetHashCode())%chunksAmount;
                        var elementsBefore = currentLeagueChunks[chunkIndex].Count;
                        currentLeagueChunks[chunkIndex].Add(record);
                        var elementsAfter = currentLeagueChunks[chunkIndex].Count;
                        if (elementsBefore == elementsAfter)
                        {
                            log.Error("Can't add element!");
                        }
                    }

                    cacheLock.EnterWriteLock();
                    LeaguesChunkedLeaderboards[leagueIndex] = currentLeagueChunks;
                    cacheLock.ExitWriteLock();
                }
            }
            catch (Exception e)
            {
                log.Error("Can't update leaderboard chunks: " + e.Message + " " + e.StackTrace);
            }
            finally
            {
                GC.Collect();
            }

            Console.WriteLine("Leaderboard chunks rebuild done");
            return Task.CompletedTask;
        }
    }
}