using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicClient;
using S;
using UnityEngine;
using UnityDI;

namespace PestelLib.Leagues
{
    public class LeaguesExampleGameInterface : MonoBehaviour, ILeaguesConcreteGameInterface
    {
        public List<ChestsRewardDef> ClaimRewards(SeasonEndInfo seasonEndInfo)
        {
            var cmd = new LeaguesModule_ClaimRewards();
            cmd.seasonEndInfo = seasonEndInfo;
            var result = CommandProcessor.Process<List<ChestsRewardDef>, LeaguesModule_ClaimRewards>(cmd);
            if (result == null)
                return null;
            var rewards = "";
            for (var i = 0; i < result.Count; ++i)
            {
                rewards += result[i].Id + " ";
            }
            Debug.Log(string.Format("Season {0} rewards {1}", seasonEndInfo.Season, rewards));
            return result;
        }

        public string GetLeagueName(int leagueLevel)
        {
            if (leagueLevel < 5)
                return "Bronze league";
            if (leagueLevel < 10)
                return "Silver league";
            if (leagueLevel < 15)
                return "Gold league";
            if (leagueLevel < 20)
                return "Platinum league";
            if (leagueLevel < 25)
                return "Diamond league";
            return "Champion league";
        }

        // только для примера. начисление трофеев должно просиходить по результатам боя в соответсвующем модуле
        public void Score(long score)
        {
            var cmd = new LeaguesModule_ScoreSL();
            cmd.score = score;
            CommandProcessor.Process<object, LeaguesModule_ScoreSL>(cmd);
        }

        // только для примера. управление сезонами происходит на сервере
        public void EndSeason()
        {
        }
    }
}
