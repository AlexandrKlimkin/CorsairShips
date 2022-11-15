using Newtonsoft.Json;
using PestelLib.ServerShared;
using System;
using System.Text;

namespace BackendCommon.Code.IapValidator.Android
{
    internal class UnityValidateQueryParser : IValidateQueryParser
    {
        public ValidationData[] Parse(IapValidateQuery query)
        {
            ValidationData result = new ValidationData();
            var receiptBytes = Convert.FromBase64String(query.Receipt);
            var receiptString = Encoding.UTF8.GetString(receiptBytes);
            var d = JsonConvert.DeserializeObject<UnityAndroidReceipt>(receiptString);
            d.jsonObj = JsonConvert.DeserializeObject<AndroidInappPurchaseData>(d.json);
            result.PurchaseData = d.jsonObj;
            try
            {
                result.SkuDetails = JsonConvert.DeserializeObject<AndroidSkuDetails>(d.skuDetails);
            }
            catch
            { }
            return new[] { result };
        }
    }
}
