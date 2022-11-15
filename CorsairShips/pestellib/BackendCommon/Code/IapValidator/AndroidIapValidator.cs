using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Backend.Code.Statistics;
using BackendCommon.Code.Data;
using BackendCommon.Code.IapValidator.Android;
using Google;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using log4net;
using Newtonsoft.Json;
using PestelLib.ServerShared;
using PestelLib.ServerCommon.Config;
using ServerLib;
using UnityDI;

namespace BackendCommon.Code.IapValidator
{
    class AndroidIapValidator : IIapValidator
    {
        private static readonly Lazy<UserStorage> _storage = new Lazy<UserStorage>(() => new UserStorage());
        public static UserStorage Storage => _storage.Value;

        static readonly ILog Log = LogManager.GetLogger(typeof(AndroidIapValidator));
        private AndroidPublisherService publisherService;
        private AndroidIapValidatorConfig _config;
        private IValidateQueryParser[] _parsers;

        public static AndroidIapValidator Create()
        {
            if (!string.IsNullOrEmpty(AppSettings.Default.AndroidInAppValidatorCredFileOverride))
                return new AndroidIapValidator(AppSettings.Default.AndroidInAppValidatorCredFileOverride);
            return new AndroidIapValidator();
        }

        public AndroidIapValidator(string credFile)
        {
            if (!File.Exists(credFile))
            {
                Log.Error($"Cred file not found. '{credFile}'.");
                throw new Exception($"Cred file not found. '{credFile}'.");
            }

            _config = new AndroidIapValidatorConfig();
            _config.CredentialFile = credFile;
            _config.EnableRevalidate = true;
            Log.Debug($"Cred file {credFile}.");
            Init();
        }


        public AndroidIapValidator()
            :this(SimpleJsonConfigLoader.LoadConfigFromRedis<AndroidIapValidatorConfig>(nameof(AndroidIapValidatorConfig), true))
        {
        }

        public AndroidIapValidator(AndroidIapValidatorConfig config)
        {
            _config = config;
            Init();
        }

        private void Init()
        {
            Log.Debug("AndroidIapValidator ctor called");

            _parsers = new IValidateQueryParser[]
            {
                new UnityValidateQueryParser(),
                new UEValidateQueryParser()
            };

            string secretFile = _config.CredentialFile;

            if (!File.Exists(secretFile))
            {
                Log.ErrorFormat("Can't find IAP validator config file: {0}", secretFile);
                return;
            }

            var secrets = GoogleCredential.FromFile(secretFile).CreateScoped(AndroidPublisherService.Scope.Androidpublisher);
            publisherService = new AndroidPublisherService(new BaseClientService.Initializer
            {
                HttpClientInitializer = secrets,
                ApplicationName = "IapValidator",
            });
        }

        public IapValidateResult IsValid(IapValidateQuery query, bool acceptOnFail)
        {
            return IsValidAsync(query, acceptOnFail).Result;
        }

        private ValidationData[] PrepareValidationData(IapValidateQuery query)
        {
            var exceptions = new List<Exception>();
            foreach (var parser in _parsers)
            {
                try
                {
                    return parser.Parse(query);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }
            throw new AggregateException("Cant parse query", exceptions);
        }

        public Task<IapValidateResult> IsValidAsync(IapValidateQuery query, bool acceptOnFail)
        {
            Log.Info("Trying to validate payment");

            var result = new IapValidateResult() {IsValid = acceptOnFail};
            var receipt = query?.Receipt;
            try
            {
                var validationData = PrepareValidationData(query);
                foreach (var data in validationData)
                {
                    var packageName = data.PurchaseData.packageName;
                    var productId = data.PurchaseData.productId;
                    var token = data.PurchaseData.purchaseToken;
                    if (data.SkuDetails != null && data.SkuDetails.type == "subs")
                    {
                        var r = publisherService.Purchases.Subscriptions.Get(packageName, productId, token).Execute();
                        Log.Debug($"Receipt {receipt}, google api result {JsonConvert.SerializeObject(r)}, skuDetails {data.SkuDetails}");
                        var isTest = r.PurchaseType == 0;
                        var canceled = r.CancelReason == 0 || r.CancelReason == 1;
                        result.IsValid = true;
                        result.IsTest = isTest;
                        result.IsPromo = false;
                        result.IsCanceled = canceled;
                    }
                    else
                    {
                        var r = publisherService.Purchases.Products.Get(packageName, productId, token).Execute();
                        Log.Debug($"Receipt {receipt}, google api result {JsonConvert.SerializeObject(r)}");
                        result.IsValid = r.PurchaseState == 0;
                        result.IsTest = r.PurchaseType == 0;
                        result.IsPromo = r.PurchaseType == 1;
                    }
                }
                SendValidationValid();
            }
            catch (GoogleApiException e)
            {
                Log.Error($"Validation failed. Error: {e}");
                if (e.Error == null)
                {
                    SendValidationFailure();
                }
                // fraud detection
                else if (e.Error.Code == 400 || e.Error.Code == 404)
                    result = new IapValidateResult();
            }
            catch (Exception e)
            {
                Log.Error($"Validation failed. Error: {e}");
                SendValidationFailure();
            }
            finally
            {
                Log.Debug($"Receipt '{receipt}' validation result is {result}");
            }
            if(!result.IsValid) SendValidationInvalid();
            return Task.FromResult(result);
        }

        private void SendValidationResult(string result, int val)
        {
            try
            {
                DefaultStatisticsClient client = ContainerHolder.Container.Resolve<DefaultStatisticsClient>();
                client.SendAsync("androidinapp", result, val);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private void SendValidationFailure()
        {
            SendValidationResult("fail", Interlocked.Increment(ref failure));
        }

        private void SendValidationValid()
        {
            SendValidationResult("valid", Interlocked.Increment(ref valid));
        }

        private void SendValidationInvalid()
        {
            SendValidationResult("invalid", Interlocked.Increment(ref invalid));
        }

        private static int valid;
        private static int invalid;
        private static int failure;
    }
}
