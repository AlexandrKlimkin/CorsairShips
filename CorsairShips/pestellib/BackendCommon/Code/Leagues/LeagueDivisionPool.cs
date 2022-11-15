using System;
using System.Linq;
using System.Threading;
using PestelLib;
using S;
using ServerLib;

namespace BackendCommon.Code.Leagues
{
    public class LeagueDivisionPool
    {
        private ILeagueStorage _storage;
        private LeagueLeaderboardConfig _config;
        private LeagueStateCache _state;
        private LeagueDefHelper _defs;

        public LeagueDivisionPool(ILeagueStorage storage, LeagueLeaderboardConfig config, LeagueStateCache state, LeagueDefHelper defs)
        {
            _storage = storage;
            _config = config;
            _state = state;
            _defs = defs;
        }

        private bool IsClosureTime()
        {
            var closureHours = (int)(_defs.CycleTime * _defs.PreClosureTimeFactor);
            var hoursToEnd = (_state.SeasonEnds - DateTime.UtcNow).TotalHours;
            return hoursToEnd <= closureHours;
        }

        private DivisionInfo AllocateSeasonStartDivision(int leagueLvl, out bool isNew)
        {
            int theshold = (int)(_defs.DivisionSize * _defs.DivisionReserveSlots);
            var div = _storage.GetDivisionsWithMaxPopulation(leagueLvl, _defs.DivisionSize - theshold)
                .FirstOrDefault();
            isNew = div == null;
            if (isNew)
                return _storage.CreateDivision(leagueLvl);
            return div;
        }

        private DivisionInfo AllocateSeasonEndDivision(int leagueLvl, out bool isNew)
        {
            var div = _storage.GetDivisionsWithMaxPopulation(leagueLvl, _defs.DivisionSize)
                .FirstOrDefault();
            isNew = div == null;
            if (isNew)
                return _storage.CreateDivision(leagueLvl);
            return div;
        }

        public DivisionInfo AllocateDivision(int leagueLvl)
        {
            DivisionInfo result;
            bool isNew;
            if (IsClosureTime())
                result = AllocateSeasonEndDivision(leagueLvl, out isNew);
            else
                result =  AllocateSeasonStartDivision(leagueLvl, out isNew);
            if(isNew)
                FillDivisionWithBots(result);
            return result;
        }

        private int InitialBotsAmount => _defs.DivisionSize - (int)(_defs.DivisionSize * _defs.DivisionReserveSlots);

        // only for new divisions (see add bot scoring logic)
        private void FillDivisionWithBots(DivisionInfo division)
        {
            if (!_config.UseBots)
                return;

            var amount = InitialBotsAmount;
            for (var i = 0; i < amount; ++i)
            {
                CreateBot(division.Id, division.LeagueLevel);
            }
        }

        private void CreateBot(Guid divisionId, int leagueLvl)
        {
            var id = Guid.NewGuid();
            var name = NicknameGenerator.GenerateName();
            _storage.CreatePlayer(id, name, null, divisionId, true, leagueLvl);
            var min = _defs.GetBotPointsMin(leagueLvl);
            var max = _defs.GetBotPointsMax(leagueLvl);
            var score = min + _safeRandom.Value.Next((int)(max - min));
            _storage.ScheduleAddScore(id, score);
        }

        private readonly ThreadLocal<Random> _safeRandom = new ThreadLocal<Random>(() => new Random());
    }
}