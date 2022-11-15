using System.Collections.Generic;
using MessagePack;

namespace PestelLib.SharedLogic.Modules
{
    [MessagePackObject()]
    public class PremiumShopModuleState
    {
        [Key(0)] public string DeveloperPayload;
        [Key(1)] public int PurchasesCount;
        [Key(2)] public List<uint> Transactions;
    }
}