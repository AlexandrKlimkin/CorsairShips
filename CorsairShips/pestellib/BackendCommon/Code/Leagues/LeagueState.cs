using System;
using System.Linq;
using log4net;
using ServerShared.Leagues;
using ServerLib;
using Newtonsoft.Json;
using PestelLib.ServerCommon.Config;
using ServerLib.Config;
using BackendCommon.Code.Jobs;

namespace BackendCommon.Code.Leagues
{
    public class LeagueState
    {
        public int Season;
        public DateTime SeasonStarts;
        public DateTime SeasonEnds;
        public DateTime BotsUpdateTime;
    }

    public class LeagueStateCache
    {
        public int Season
        {
            get => CheckRead((s) => s.Season);
            set => CheckAndWrite(ref _dbState.Season, nameof(Season), value);
        }
        public DateTime SeasonEnds
        {
            get => CheckRead((s) => s.SeasonEnds);
            set => CheckAndWrite(ref _dbState.SeasonEnds, nameof(SeasonEnds), value);
        }

        public DateTime SeasonStarts
        {
            get => CheckRead((s) => s.SeasonStarts);
            set => CheckAndWrite(ref _dbState.SeasonStarts, nameof(SeasonStarts), value);
        }

        public DateTime BotsUpdateTime
        {
            get => CheckRead((s) => s.BotsUpdateTime);
            set => CheckAndWrite(ref _dbState.BotsUpdateTime, nameof(BotsUpdateTime), value);
        }

        public LeagueStateCache()
            :this(QuartzConfig.ShouldExecuteJobs)
        { }

        public LeagueStateCache(Func<bool> canWriteCheck)
        {
            _canWrite = canWriteCheck;
            var newFromatState = RedisUtils.Cache.StringGet(RedisKey);
            if (!canWriteCheck())
            {
                if (!newFromatState.HasValue)
                {
                    Log.Debug("Write disabled. Loading key from League:Config.");
                    _migrator = new RedisConfig<LeagueState>(LeagueLeaderboardConfigCache.LeagueLeaderboardConfigKey,
                        TimeSpan.FromSeconds(10));
                }
                else
                {
                    Log.Debug("Write disabled. Loading key from League:State.");
                    _migrator = new RedisConfig<LeagueState>(RedisKey, TimeSpan.FromSeconds(10));
                }
            }
            else
            {
                // только что включили бэк с этим кодом, нужно выполнить миграцию
                if (!newFromatState.HasValue)
                {
                    _dbState = SimpleJsonConfigLoader.LoadConfigFromRedis<LeagueState>(LeagueLeaderboardConfigCache.LeagueLeaderboardConfigKey, false);
                    Log.Debug($"Write enabled. Migrate from old league state format. data={JsonConvert.SerializeObject(_dbState)}.");
                    Save();
                }
                else
                {
                    Log.Debug("Write enabled. Loading key from League:State.");
                    _dbState = JsonConvert.DeserializeObject<LeagueState>(newFromatState);
                }
            }
        }

        private T CheckRead<T>(Func<LeagueState, T> func)
        {
            if (_canWrite())
            {
                // и так все актуальные данные в памяти, ничего не делаем
                return func(_dbState);
            }
            _dbState = _migrator.Config;
            return func(_dbState);
        }

        private bool CheckAndWrite<T>(ref T tochange, string name, T value, bool dontThrow = false)
        {
            if (!_canWrite())
            {
                var msg = $"Cant change {name} from {tochange} to {value}. Write operation not allowed.";
                Log.Error(msg);
                if (dontThrow)
                    return false;
                throw new Exception(msg);
            }
            tochange = value;
            Save();
            return true;
        }

        private void Save()
        {
            var raw = JsonConvert.SerializeObject(_dbState);
            RedisUtils.Cache.StringSet(RedisKey, raw);
        }

        private LeagueState _dbState;
        private Func<bool> _canWrite = () => true;
        private RedisConfig<LeagueState> _migrator;

        private static readonly ILog Log = LogManager.GetLogger(typeof(LeagueStateCache));

        private const string RedisKey = "League:State";
    }

    public class LeagueDefHelper
    {
        public int LeaguesAmount => _defProvider.LeaguesAmount;
        public int DivisionSize => _defProvider.LeagueDivisionSize;
        public int CycleTime => _defProvider.LeagueCycleTime;
        public float PreClosureTimeFactor => _defProvider.LeaguePreClosureTimeFactor;
        public float DivisionReserveSlots => _defProvider.LeagueDivisionReserveSlots;

        public virtual float GetDivisionUpCoeff(int leagueLvl)
        {
            return _defProvider.GetDivisionUpCoeff(leagueLvl);
        }

        public virtual float GetDivisionDownCoeff(int leagueLvl)
        {
            return _defProvider.GetDivisionDownCoeff(leagueLvl);
        }

        public long GetBotPointsMin(int leagueLvl)
        {
            return _defProvider.GetBotMinPoints(leagueLvl);
        }

        public long GetBotPointsMax(int leagueLvl)
        {
            return _defProvider.GetBotMaxPoints(leagueLvl);
        }


        public LeagueDefHelper(ILeagueDefProvider defProvider)
        {
            _defProvider = defProvider != null && defProvider.IsValid ? defProvider : null;

            var defs = JsonConvert.SerializeObject(_defProvider);
            var lvlsList = Enumerable.Range(0, _defProvider.LeaguesAmount).Select(_ => $"lvl: {_}, divUp: {_defProvider.GetDivisionUpCoeff(_)}, divDown: {_defProvider.GetDivisionDownCoeff(_)}, botMin: {_defProvider.GetBotMinPoints(_)}, botMax: {_defProvider.GetBotMaxPoints(_)}.");
            var lvlsStr = string.Join(" ", lvlsList);
            Log.Debug($"Loaded league defs: {defs}, lvls: {lvlsStr}.");
        }

        private ILeagueDefProvider _defProvider;

        private static readonly ILog Log = LogManager.GetLogger(typeof(LeagueDefHelper));
    }
}
