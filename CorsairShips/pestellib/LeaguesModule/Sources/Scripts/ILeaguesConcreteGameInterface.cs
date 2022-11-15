using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PestelLib.SharedLogic.Modules;
using S;

namespace PestelLib.Leagues
{
    public interface ILeaguesConcreteGameInterface
    {
        string GetLeagueName(int leagueLevel);
        List<ChestsRewardDef> ClaimRewards(SeasonEndInfo seasonEndInfo);
    }
}
