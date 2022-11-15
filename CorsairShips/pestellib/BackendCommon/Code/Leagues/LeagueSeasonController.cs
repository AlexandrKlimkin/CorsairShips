using System;
using System.Diagnostics;
using System.Linq;
using log4net;
using PestelLib.Utils;
using S;
using ServerLib;

namespace BackendCommon.Code.Leagues
{
    public class LeagueSeasonController
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LeagueSeasonController));
        private readonly ILeagueStorage _storage;
        private readonly LeagueDivisionPool _divisionPool;
        private readonly LeagueLeaderboardConfig _config;
        private readonly Random _rnd = new Random();
        private LeagueStateCache _state;
        private LeagueDefHelper _defs;

        public LeagueSeasonController(ILeagueStorage storage, LeagueDivisionPool divisionPool, LeagueLeaderboardConfig config, LeagueStateCache state, LeagueDefHelper defs)
        {
            _storage = storage;
            _divisionPool = divisionPool;
            _config = config;
            _state = state;
            _defs = defs;
        }

        public DateTime LastBotUpdateTime => _state.BotsUpdateTime;
        public DateTime NextBotUpdateTime => _state.BotsUpdateTime + _config.BotsUpdateDelay;

        public void UpdateBots(bool force = false)
        {
            UpdateBotsInternalWithLock(force);
        }

        private void UpdateBotsInternalWithLock(bool force)
        {
            using (_storage.Lock().Result)
            {
                UpdateBotsInternal(force);
            }
        }

        private void UpdateBotsInternal(bool force = false)
        {
            var sw = Stopwatch.StartNew();
            var botsUpdated = 0L;
            for (var i = 0; i < _defs.LeaguesAmount; ++i)
            {
                botsUpdated += _storage.IncrementBotScores(i, _defs.GetBotPointsMin(i), _defs.GetBotPointsMax(i), 1000, force);
            }
            if (sw.ElapsedMilliseconds >= _config.SeasonControllerTimeThreshold)
                Log.Debug($"Bots scores updated. bots_count={botsUpdated}, time={sw.ElapsedMilliseconds}ms.");
            _state.BotsUpdateTime = DateTime.UtcNow;
        }

        public void Update()
        {
            IDisposable l = null;
            try
            {
                Stopwatch sw;
                if (_state.SeasonStarts == DateTime.MinValue)
                {
                    l = _storage.Lock().Result;
                    Log.InfoFormat($"Starting {_state.Season + 1} season");
                    sw = Stopwatch.StartNew();
                    StartFirstSeason();
                    if (sw.ElapsedMilliseconds >= _config.SeasonControllerTimeThreshold)
                    {
                        Log.WarnFormat($"StartFirstSeason took {sw.ElapsedMilliseconds} ms");
                    }
                }

                if (_state.SeasonEnds <= DateTime.UtcNow)
                {
                    if (l == null) l = _storage.Lock().Result;
                    Log.InfoFormat($"Ending {_state.Season} season");
                    sw = Stopwatch.StartNew();
                    EndSeason();
                    //if (sw.ElapsedMilliseconds >= _config.SeasonControllerTimeThreshold)
                    {
                        Log.WarnFormat($"EndSeason took {sw.ElapsedMilliseconds} ms");
                    }
                    Log.InfoFormat($"Starting {_state.Season + 1} season");
                    sw = Stopwatch.StartNew();
                    StartNewSeason();
                    //if (sw.ElapsedMilliseconds >= _config.SeasonControllerTimeThreshold)
                    {
                        Log.WarnFormat($"StartNewSeason took {sw.ElapsedMilliseconds} ms");
                    }
                }

                if (_config.UseBots &&
                    _state.SeasonStarts <= DateTime.UtcNow && DateTime.UtcNow < _state.SeasonEnds)
                {
                    sw = Stopwatch.StartNew();
                    if (l == null) l = _storage.Lock().Result;
                    UpdateBotsInternal();
                    if (sw.ElapsedMilliseconds >= _config.SeasonControllerTimeThreshold)
                    {
                        Log.WarnFormat($"BotUpdate took {sw.ElapsedMilliseconds} ms");
                    }
                }
            }
            finally
            {
                l?.Dispose();
            }

            _storage.ProcessAddScoreSchedule();
        }

        private void StartFirstSeason()
        {
            _state.Season = 1;
            if (_config.SeasonStartAlign)
                _state.SeasonStarts = DateTime.UtcNow.Date.AddDays(1);
            else
                _state.SeasonStarts = DateTime.UtcNow;
            _state.SeasonEnds = _state.SeasonStarts + TimeSpan.FromHours(_defs.CycleTime);
        }

        private void StartNewSeason()
        {
            ++_state.Season;
            if (_config.SeasonStartAlign)
                _state.SeasonStarts = DateTime.UtcNow.Date;
            else
                _state.SeasonStarts = DateTime.UtcNow;
            var end = _state.SeasonStarts + TimeSpan.FromHours(_defs.CycleTime);
            if (_config.SeasonStartAlign && end < DateTime.UtcNow)
            {
                _state.SeasonStarts = DateTime.UtcNow.Date.AddDays(1);
                end = _state.SeasonStarts + TimeSpan.FromHours(_defs.CycleTime);
            }
            _state.SeasonEnds = end;

            var activePlayers = _storage.ActivePlayers(_config.ActivePlayersAmount).ToList().Shuffle();
            foreach (var player in activePlayers)
            {
                if(player == null)
                    continue;
                if (player.DivisionId != Guid.Empty)
                    continue;
                var div = _divisionPool.AllocateDivision(player.LeagueLevel);
                _storage.SetPlayerDivision(player.PlayerId, div.Id);
            }
        }

        private void EndSeason()
        {
            _storage.OnSeasonEnd();

            var wipeSeason = _state.Season - _config.SeasonHistory;
            if (wipeSeason > 0)
            {
                try
                {
                    _storage.WipeSeasonData(wipeSeason);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}