using System.Collections.Generic;
using MessagePack;
using Newtonsoft.Json;
using S;

namespace PestelLib.SharedLogic.Modules
{
    [MessagePackObject()]
    public class CompositeModuleState
    {
        //[JsonConverter(typeof(UserProfileModulesConverter))]
        [Key(0)]
        public Dictionary<string, byte[]> ModulesDict = new Dictionary<string, byte[]>();
    }
}