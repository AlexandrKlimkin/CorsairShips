using MessagePack;

namespace PestelLib.SharedLogic.Modules
{
    [MessagePackObject]
    public class MoneyModuleState
    {
        [Key(0)]
        public int Real { get; set; }

        [Key(1)]
        public int RealInapp { get; set; }

        [Key(2)]
        public int Ingame { get; set; }

        [Key(3)]
        public int IngameInapp { get; set; }
    }
    
    public enum MoneyType
    {
        MoneyTypeIngame,
        MoneyTypeReal
    }
}