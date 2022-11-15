
namespace BackendCommon.Code.IapValidator
{
#pragma warning disable 649
    sealed class IosIapValidatorConfig
    {
        public string SharedSecret;
        public bool Sandbox = true;
        public string BundleId;
        public bool EnableRevalidate;
        public bool RejectEmptyReceipt;
    }
#pragma warning restore 649
}