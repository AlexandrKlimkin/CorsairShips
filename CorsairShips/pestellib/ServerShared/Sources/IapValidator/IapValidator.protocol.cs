using System;

namespace PestelLib.ServerShared
{
    // нельзя обфусцировать используется как JSON
    [System.Reflection.Obfuscation(Exclude = true)]
    public class IapValidateQuery
    {
        public string Platform;
        public string Receipt;
        public Guid PlayerId;
    }

    // нельзя обфусцировать используется как JSON
    [System.Reflection.Obfuscation(Exclude = true)]
    public class IapValidateResult
    {
        public bool IsValid;
        public bool IsTest;
        public bool IsPromo;
        public bool IsCanceled;

        public override string ToString()
        {
            return "is valid: " + IsValid + " is test: " + IsTest + " is promo: " + IsPromo + " is canceled: " + IsCanceled;
        }
    }
}
