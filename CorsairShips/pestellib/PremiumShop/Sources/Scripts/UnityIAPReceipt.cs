namespace PestelLib.PremiumShop
{
    // нельзя обфусцировать используется как JSON
    [System.Reflection.Obfuscation(Exclude = true)]
    public class UnityIAPReceipt
    {
        public string Store;
        public string TransactionID;
        public string Payload;
    }
}