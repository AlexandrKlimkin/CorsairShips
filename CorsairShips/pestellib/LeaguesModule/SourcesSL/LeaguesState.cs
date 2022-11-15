using System.Collections.Generic;
using MessagePack;

namespace PestelLib.SharedLogic.Modules
{
    [System.Serializable]
    [MessagePackObject]
    public class LeaguesState
    {
        [Key(0)] public List<int> ClaimedSeasons;
        [Key(1)] public int CurrentLeague;
        [Key(2)] public int MaxLeague;
    }
}
