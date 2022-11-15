using Newtonsoft.Json;
using PestelLib.ServerShared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BackendCommon.Code.IapValidator.Android
{
#pragma warning disable 0649
    class UnityAndroidReceipt
    {
        public string signature;
        public string json;
        public string skuDetails;
        public AndroidInappPurchaseData jsonObj;
        public AndroidSkuDetails skuDetailsObj;
    }

    class UEAndroidValidationData
    {
        public string receiptData;
        public string signature;
    }

    class UEAndroidReceiptListItem
    {
        public string itemName;
        public string uniqueItemId;
        public string validationInfo;
    }

    class UEAndroidReceiptList
    {
        public string offerId;
        public int quantity;
        public UEAndroidReceiptListItem[] items;
    }

    class UEAndroidReceipt
    {
        public string transactionId;
        public UEAndroidReceiptList[] receiptList;
    }


#pragma warning restore 0649

    internal class UEValidateQueryParser : IValidateQueryParser
    {
        public ValidationData[] Parse(IapValidateQuery query)
        {
            List<ValidationData> result = new List<ValidationData>();
            var receiptBytes = Convert.FromBase64String(query.Receipt);
            var receiptString = Encoding.UTF8.GetString(receiptBytes);
            var d = JsonConvert.DeserializeObject<UEAndroidReceipt>(receiptString);
            if (d.receiptList.Length == 0)
                throw new Exception("Receipts list is empty.");
            var items = d.receiptList.SelectMany(_ => _.items).ToArray();
            if (items.Length == 0)
                throw new Exception("Receipts list is empty.");
            foreach (var item in items)
            {
                var vd = JsonConvert.DeserializeObject<UEAndroidValidationData>(item.validationInfo);
                var dataBytes = Convert.FromBase64String(vd.receiptData);
                var dataString = Encoding.UTF8.GetString(dataBytes);
                var data = JsonConvert.DeserializeObject<AndroidInappPurchaseData>(dataString);
                if (string.IsNullOrEmpty(data.orderId))
                    throw new Exception($"Bad order id '{receiptString}'.");
                if (string.IsNullOrEmpty(data.purchaseToken))
                    throw new Exception($"No purchaseToken '{receiptString}'.");
                result.Add(new ValidationData { PurchaseData = data });
            }
            return result.ToArray();
        }
    }
}
