using PestelLib.ServerShared;
using System;
using System.Collections.Generic;
using System.Text;

namespace BackendCommon.Code.IapValidator.Android
{
#pragma warning disable 0649
    class AndroidSkuDetails
    {
        public string productId;
        public string type;
        public string price;
        public long price_amount_micros;
        public string price_currency_code;
        public string subscriptionPeriod;
        public string freeTrialPeriod;
        public string title;
        public string description;
    }
    class AndroidInappPurchaseData
    {
        public bool autoRenewing;
        public string orderId;
        public string packageName;
        public string productId;
        public string purchaseTime;
        public string purchaseState;
        public string developerPayload;
        public string purchaseToken;
    }

    class ValidationData
    {
        public AndroidInappPurchaseData PurchaseData;
        public AndroidSkuDetails SkuDetails;
    }
#pragma warning restore 0649

    internal interface IValidateQueryParser
    {
        ValidationData[] Parse(IapValidateQuery query);
    }
}
