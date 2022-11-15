using Newtonsoft.Json;
using MessagePack;

namespace S
{
    // нельзя обфусцировать используется как JSON
    [System.Reflection.Obfuscation(Exclude = true)]
    [MessagePackObject]
    public class UnityPurchaseReceipt
    {
        public const string IOS = "AppleAppStore";
        public const string ANDROID = "GooglePlay";
        public const string FAKE = "fake";

        [Key(0)]
        public string Store;
        [Key(1)]
        public string TransactionID;
        [Key(2)]
        public string Payload;
        [Key(3)]
        public string DeveloperPayload;

        public static UnityPurchaseReceipt FromString(string receipt)
        {
            return JsonConvert.DeserializeObject<UnityPurchaseReceipt>(receipt);
        }
    }
}
