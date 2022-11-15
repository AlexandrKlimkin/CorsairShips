using MessagePack;

namespace PestelLib.SharedLogic.Modules
{
    [MessagePackObject()]
    public class MyTestModuleState
    {
        [Key(0)]
        public int Ingame { get; set; }

        [Key(1)]
        public int Real { get; set; }
    }
}