using System;
using S;

namespace PestelLib.ServerProtocol
{
    public interface ILeagueServerModuleApi
    {
        SeasonEndInfo PullSeasonEndInfo(Guid playerId, int season);
        void Score(Guid playerId, long score);
    }
}
