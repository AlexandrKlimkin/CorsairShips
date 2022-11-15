using MessagePack;

namespace PestelLib.SharedLogic.Modules
{
    [System.Serializable]
    [MessagePackObject]
    public class RandomModuleState
    {
         [Key(0)]
        public int RandomSeed;
    }
}