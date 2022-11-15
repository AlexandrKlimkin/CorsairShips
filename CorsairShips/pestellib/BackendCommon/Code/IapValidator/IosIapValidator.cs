using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using PestelLib.ServerCommon.Config;
using PestelLib.ServerShared;
using ServerLib;

namespace BackendCommon.Code.IapValidator
{
#pragma warning disable 0649
    class AppStoreReceipt
    {
        public string bundle_id;
        public string application_version;
        public string original_application_version;
        public string creation_date;
        public string expiration_date;
    }

    class AppStoreValidateionRequest
    {
        [JsonProperty("receipt-data")]
        public string receipt_data;
        //public string password;
        //[JsonProperty("exclude-old-transactions")]
        //public bool exclude_old_transactions;

        [JsonProperty("password")]
        public string password;
    }

    class AppStoreValidationResponse
    {
        public string status;
        public AppStoreReceipt receipt;
    }
#pragma warning restore 0649

    class IosIapValidator : IIapValidator
    {
        const string TransKey = "IosIapValidatorTransactions";
        static readonly ILog Log = LogManager.GetLogger(typeof(IosIapValidator));
        HttpClient _httpClient = new HttpClient();
        private IosIapValidatorConfig _config;

        public IosIapValidator(IosIapValidatorConfig config)
        {
            _config = config;
        }

        public IosIapValidator()
            :this(SimpleJsonConfigLoader.LoadConfigFromRedis<IosIapValidatorConfig>(nameof(IosIapValidatorConfig), true))
        {
        }

        public IapValidateResult IsValid(IapValidateQuery query, bool acceptOnFail)
        {
            return IsValidAsync(query, acceptOnFail).Result;
        }

        public async Task<IapValidateResult> IsValidAsync(IapValidateQuery query, bool acceptOnFail)
        {
            var receiptPayload = query.Receipt;
            bool result = acceptOnFail;
            try
            {
                result = await IsValidReceipt(receiptPayload).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Validation failed. Error: " + e);
            }
            finally
            {
                Log.DebugFormat("Receipt '{0}' validation result is {1}", receiptPayload, result);
            }
            return new IapValidateResult() {IsValid = result};
        }

        private async Task<bool> IsValidReceipt(string receiptPayload)
        {
            return await CheckIfReceiptValid(receiptPayload, "https://buy.itunes.apple.com/verifyReceipt").ConfigureAwait(false);
        }

        private async Task<bool> CheckIfReceiptValid(string receiptPayload, string url)
        {
            var o = new AppStoreValidateionRequest
            {
                receipt_data = receiptPayload,
                password = _config.SharedSecret
            };

            IosReceipt receipt;
            try
            {
                receipt = IosReceipt.fromBase64(receiptPayload);
                if (!string.IsNullOrEmpty(_config.BundleId) && !receipt.ProductId.StartsWith(_config.BundleId))
                {
                    Log.WarnFormat("Receipt from foreign app '{0}'", receipt.ProductId);
                    return false;
                }

                if (receipt.TransactionIds.Length == 0 && _config.RejectEmptyReceipt)
                {
                    Log.WarnFormat("Empty receipt");
                    return false;
                }

                var newTransCount = 0;
                for (var i = 0; i < receipt.TransactionIds.Length; ++i)
                {
                    var transId = receipt.TransactionIds[i];
                    var newTrans = RedisUtils.Cache.HashSet(TransKey, transId, "");
                    if (newTrans)
                    {
                        ++newTransCount;
                    }
                }

                if (newTransCount == 0 && !_config.EnableRevalidate)
                {
                    Log.WarnFormat("Transaction(s) '{0}' already verified", string.Join(",", receipt.TransactionIds));
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Error("Local receipt validation failed. Error: " + e);
            }

            var json = JsonConvert.SerializeObject(o);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var result = await _httpClient.PostAsync(url, content).ConfigureAwait(false);
            if (!result.IsSuccessStatusCode)
            {
                Log.WarnFormat("Request {0} finished with HTTP status {1}", url, result.StatusCode);
                return true;
            }

            var itunesAnswerStr = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            var itunesAnswer = JsonConvert.DeserializeObject<AppStoreValidationResponse>(itunesAnswerStr);
            if (itunesAnswer.status == "0")
            {
                return true;
            }
            else
            {
                if (itunesAnswer.status == "21007")
                {
                    return await CheckIfReceiptValid(receiptPayload, "https://sandbox.itunes.apple.com/verifyReceipt").ConfigureAwait(false);
                }
                else
                {
                    Log.WarnFormat("Request {0} finished with AppStore error {1}: {2}", url, itunesAnswer.status,
                        GetStatusDescription(itunesAnswer.status));
                }
            }

            return false;
        }

        private string GetStatusDescription(string status)
        {
            switch (status)
            {
                case "21000":
                    return "The App Store could not read the JSON object you provided.";
                case "21002":
                    return "The data in the receipt-data property was malformed or missing.";
                case "21003":
                    return "The receipt could not be authenticated.";
                case "21004":
                    return "The shared secret you provided does not match the shared secret on file for your account.";
                case "21005":
                    return "The receipt server is not currently available.";
                case "21006":
                    return "This receipt is valid but the subscription has expired. When this status code is returned to your server, the receipt data is also decoded and returned as part of the response. Only returned for iOS 6 style transaction receipts for auto - renewable subscriptions.";
                case "21007":
                    return "This receipt is from the test environment, but it was sent to the production environment for verification. Send it to the test environment instead.";
                case "21008":
                    return "This receipt is from the production environment, but it was sent to the test environment for verification. Send it to the production environment instead.";
                case "21010":
                    return "This receipt could not be authorized. Treat this the same as if a purchase was never made.";
                default:
                    return "Unknown error";
            }
        }
    }
}
