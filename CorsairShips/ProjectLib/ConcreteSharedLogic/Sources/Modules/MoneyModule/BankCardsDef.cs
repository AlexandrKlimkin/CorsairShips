using System;

namespace PestelLib.SharedLogic.Modules
{
    [Serializable]
    public class BankCardsDef
    {
        public string Id;
        public MoneyType MoneyType;
        public int Amount;
        public double Price;
        public string Name;
        public string Icon;
    }
}
