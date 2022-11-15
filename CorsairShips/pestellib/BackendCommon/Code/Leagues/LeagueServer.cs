using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Microsoft.Extensions.Caching.Memory;
using PestelLib.ServerProtocol;
using S;

namespace BackendCommon.Code.Leagues
{
    public class LeagueServer : ILeagueServerModuleApi
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LeagueServer));
        private readonly ILeagueStorage _storage;
        private readonly LeagueLeaderboardConfig _config;
        public readonly LeagueDivisionPool DivisionPool;
        public readonly LeagueSeasonController SeasonController;
        private ConcurrentQueue<Action> _jobs = new ConcurrentQueue<Action>();
        public DateTime SeasonEndTime => _state.SeasonEnds;
        private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private ConcurrentDictionary<string, Task<LeagueRegisterResponse>> _loginCache = new ConcurrentDictionary<string, Task<LeagueRegisterResponse>>();
        private LeagueStateCache _state;

        public LeagueServer(ILeagueStorage storage, LeagueLeaderboardConfig config, LeagueStateCache state, LeagueDefHelper defs)
        {
            _storage = storage;
            _config = config;
            _state = state;
            DivisionPool = new LeagueDivisionPool(storage, _config, state, defs);
            SeasonController = new LeagueSeasonController(storage, DivisionPool, config, state, defs);

        }

        public Task<LeagueRegisterResponse> Register(Guid playerId, string name, string facebookId)
        {
            var pi = _storage.PlayerLogin(playerId, name, facebookId) ?? CreatePlayer(playerId, name, facebookId);
            if (pi.DivisionId == Guid.Empty)
            {
                var div = DivisionPool.AllocateDivision(pi.LeagueLevel);
                Log.Debug($"Player {playerId} bound to division {div.Id}.");
                _storage.SetPlayerDivision(playerId, div.Id);
            }

            _cache.Set(PlayerInfoCacheKey(playerId), pi, TimeSpan.FromMinutes(1));

            return Task.FromResult(new LeagueRegisterResponse()
            {
                PlayerInfo = pi,
                CurrentSeason = _state.Season,
                SeasonStart = _state.SeasonStarts,
                SeasonEnd = _state.SeasonEnds
            });
        }

        public LeaguePlayerInfo BanByScore(long score)
        {
            return _storage.BanByScore(score);
        }

        public LeaguePlayerInfo BanById(Guid id)
        {
            return _storage.BanById(id);
        }

        public LeaguePlayerInfo Remove(Guid id)
        {
            return _storage.Remove(id);
        }

        private string PlayerInfoCacheKey(Guid playerId)
        {
            return $"lplayer{playerId}_{_state.Season}";
        }

        private LeaguePlayerInfo CachedGetPlayer(Guid playerId)
        {
            var key = PlayerInfoCacheKey(playerId);
            var pi = _cache.Get<LeaguePlayerInfo>(key);
            if (pi == null)
            {
                pi = _storage.GetPlayer(playerId);
                _cache.Set(key, pi, TimeSpan.FromMinutes(1));
            }

            return pi;
        }

        public Task<LeaguePlayerInfo> GetPlayer(Guid playerId)
        {
            return Task.FromResult(_storage.GetPlayer(playerId));
        }

        public SeasonEndInfo PullSeasonEndInfo(Guid playerId, int season)
        {
            using (_storage.Lock().Result)
            {
                return _storage.PopSeasonInfo(playerId, season);
            }
        }

        public void Score(Guid playerId, long score)
        {
            if (_storage.HasBan(playerId))
            {
                Log.Debug($"Cant add score player {playerId}: Banned in leagues. Score={score}.");
                return;
            }

            var pi = _cache.Get<LeaguePlayerInfo>(PlayerInfoCacheKey(playerId));
            if (pi != null)
                pi.Score += score;

            _storage.ScheduleAddScore(playerId, score);
        }

        public Task<int> PlayerGlobalRank(Guid playerId)
        {
            return Task.FromResult(_storage.GetGlobalRank(playerId));
        }

        public Task<int> PlayerLeagueRank(Guid playerId)
        {
            return Task.FromResult(PlayerLeagueRankInt(playerId));
        }

        private int PlayerLeagueRankInt(Guid playerId)
        {
            var p = _storage.GetPlayer(playerId);
            return _storage.GetLeagueRank(playerId, p.LeagueLevel);
        }

        public Task<LeagueTopResponse> DivisionPlayersRanks(Guid playerId)
        {
            var playerInfo = _storage.GetPlayer(playerId);
            // ошибка на клиенте, логин и запросы топов отправляются асинхронно, поэтому может запросить топ пустого дивизиона
            if (playerInfo.DivisionId == Guid.Empty)
                return Task.FromResult(new LeagueTopResponse()
                {
                    Ranks = new[] { playerInfo },
                    PlayerRank = 1
                });
            var ranks = _storage.GetDivision(playerInfo.DivisionId).ToList();
            var myRank = ranks.ToList().FindIndex(_ => _.PlayerId == playerId) + 1;
            return Task.FromResult(new LeagueTopResponse()
            {
                Ranks = ranks.ToArray(),
                PlayerRank = myRank
            });
        }

        public Task<LeagueTopResponse> LeagueTop(Guid playerId, int amount)
        {
            var playerInfo = CachedGetPlayer(playerId);
            var cacheKey = $"league{playerInfo.LeagueLevel}top{_state.Season}";
            var ranks = _cache.Get(cacheKey) as List<LeaguePlayerInfo>;
            if (ranks == null)
            {
                ranks = _storage.GetLeagueTop(playerInfo.LeagueLevel, amount).ToList();
                _cache.Set(cacheKey, ranks, DateTimeOffset.UtcNow.AddSeconds(10));
            }

            var myRank = ranks.FindIndex(_ => _.PlayerId == playerId) + 1;
            var result = new LeagueTopResponse()
            {
                Ranks = ranks.ToArray(),
                PlayerRank = myRank < 1 ? CachedLeagueRank(playerInfo) : myRank
            };
            return Task.FromResult(result);
        }

        private int CachedLeagueRank(LeaguePlayerInfo pi)
        {
            var cacheKey = $"league{pi.LeagueLevel}top{_state.Season}_score{pi.Score}place";
            var result = _cache.Get<int>(cacheKey);
            if (result > 0)
                return result;
            result = _storage.GetLeagueRank(pi.PlayerId, pi.LeagueLevel);
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(1));
            return result;
        }

        public Task<LeagueTopResponse> GlobalTop(Guid playerId, int amount)
        {
            var cacheKey = $"globaltop{_state.Season}";
            var ranks = _cache.Get(cacheKey) as List<LeaguePlayerInfo>;
            if (ranks == null)
            {
                ranks = _storage.GetGlobalTop(amount).ToList();
                _cache.Set(cacheKey, ranks, DateTimeOffset.UtcNow.AddSeconds(20));
            }

            if (playerId == Guid.Empty)
            {
                return Task.FromResult(new LeagueTopResponse()
                {
                    Ranks = ranks.ToArray()
                });
            }

            var myRank = ranks.FindIndex(_ => _.PlayerId == playerId) + 1;
            return Task.FromResult(new LeagueTopResponse()
            {
                Ranks = ranks.ToArray(),
                PlayerRank = myRank < 1 ? CachedGetGlobalRank(playerId) : myRank
            });
        }

        private int CachedGetGlobalRank(Guid playerId)
        {
            var pi = CachedGetPlayer(playerId);
            var cacheKey = $"globaltop{_state.Season}_score{pi.Score}place";
            var result = _cache.Get<int>(cacheKey);
            if (result > 0)
                return result;
            result = _storage.GetGlobalRank(pi.PlayerId);
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(1));
            return result;
        }

        private LeaguePlayerInfo CreatePlayer(Guid playerId, string name, string facebookId)
        {
            var div = DivisionPool.AllocateDivision(0);
            return _storage.CreatePlayer(playerId, name, facebookId, div.Id);
        }
    }
}
