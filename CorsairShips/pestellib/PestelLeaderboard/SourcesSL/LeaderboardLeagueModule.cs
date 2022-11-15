using System.Collections.Generic;
using System.Linq;
using PestelLib.Serialization;
using UnityDI;
using PestelLib.SharedLogicBase;

namespace PestelLib.SharedLogic.Modules
{
    public class LeaderboardLeagueModule : SharedLogicModule<LeaderboardLeagueState>
    {
        [GooglePageRef("LeaderboardLeagues")] [Dependency] protected List<LeaderboardLeague> _leagues;

        public ScheduledAction OnHonorPointsChanged;
        public ScheduledAction<LeaderboardLeague> OnRevenueReceived;

        public LeaderboardLeagueModule()
        {
            OnHonorPointsChanged = new ScheduledAction(ScheduledActionCaller);
            OnRevenueReceived = new ScheduledAction<LeaderboardLeague>(ScheduledActionCaller);
        }

        public int HonorPoints
        {
            get { return State.HonorPoints; }
        }

        //override in derived class
        protected virtual void GiveReward(string revenueId)
        {

        }

        protected void SetHonorPoints(int newValue)
        {
            var leagueBefore = GetLeague(HonorPoints);

            State.HonorPoints = newValue;
            
            if (leagueBefore != League)
            {
                Log("New league reward");
                GiveReward(leagueBefore.RevenueId);
                //var msg = string.Format(_localizationData.Get("new_league_message"), GetLocalizedLeagueName());
                //_rewardUtility.GiveRevenue(_definitions.GetRevenueDef(League.RevenueId), false, msg);
                OnRevenueReceived.Schedule(leagueBefore);
            }

            OnHonorPointsChanged.Schedule();
        }

        public LeaderboardLeague League
        {
            get { return GetLeague(State.HonorPoints); }
        }

        public LeaderboardLeague GetLeague(int honorPoints)
        {
            for (var i = 0; i < _leagues.Count; i++)
            {
                if (_leagues[i].HonorPoints > honorPoints)
                {
                    return _leagues[i];
                }
            }

            return _leagues[_leagues.Count - 1];
        }

        public LeaderboardLeague GetNextLeague()
        {
            return GetLeague(League.HonorPoints + 1);
        }

        public LeaderboardLeague GetNextLeague(LeaderboardLeague current)
        {
            return GetLeague(current.HonorPoints + 1);
        }

        public float GetMinHonorPointsForLeague(LeaderboardLeague league)
        {
            var prevLeague = _leagues.LastOrDefault(x => x.HonorPoints < league.HonorPoints);
            if (prevLeague == null)
            {
                return 0;
            }
            else
            {
                return prevLeague.HonorPoints;
            }
        }

        public float GetMaxHonorPointForLeague(LeaderboardLeague league)
        {
            return league.HonorPoints;
        }

        public float NormalizedProgress {
            get
            {
                var range = GetMaxHonorPointForLeague(League) - GetMinHonorPointsForLeague(League);
                var currentLeagueProgress = HonorPoints - GetMinHonorPointsForLeague(League);
                return currentLeagueProgress / range;
            }
        }
    }
}