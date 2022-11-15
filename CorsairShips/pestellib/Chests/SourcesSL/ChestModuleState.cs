using MessagePack;
using System.Collections.Generic;
namespace PestelLib.SharedLogic.Modules
{
    [MessagePackObject]
    public class ChestModuleState
    {
    	[Key(0)]
        public Dictionary<string, Dictionary<string, int>> PoolWeightMods;
    }
}