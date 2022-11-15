using System;
using GoogleSpreadsheet;
using PestelLib.SharedLogicBase;
using S;
using ServerShared.PlayerProfile;

namespace SharedLogicBase.Server.PlayerProfile
{
    public interface IPlayerProfileProvider
    {
        void Init(IGameDefinitions defs);
        ProfileDTO CreateFromState(LeaguePlayerInfo playerInfo, ISharedLogic state, string country);
        ProfileDTO CreateLeagueBot(LeaguePlayerInfo playerInfo, ProfileDTO[] templates);
        ProfileDTO UpdateLeagueBot(LeaguePlayerInfo playerInfo, ProfileDTO botProfile, TimeSpan delta);
    }
}
