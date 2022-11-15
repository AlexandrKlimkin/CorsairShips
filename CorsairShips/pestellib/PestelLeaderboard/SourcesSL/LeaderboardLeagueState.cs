using MessagePack;

namespace PestelLib.SharedLogic.Modules
{
    [MessagePackObject]
    public class LeaderboardLeagueState
    {
        [Key(0)] public int HonorPoints;

        [Key(1)] public int LeagueIndex;
    }
}