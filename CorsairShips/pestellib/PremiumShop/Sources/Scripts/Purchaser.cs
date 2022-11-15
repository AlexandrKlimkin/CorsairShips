using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IapValidator;
using PestelLib.Localization;
using PestelLib.SharedLogicClient;
using PestelLib.UI;
using S;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityDI;
using Newtonsoft.Json;
using PestelLib.ServerShared;
using ServerShared;

// Placing the Purchaser class in the CompleteProject namespace allows it to interact with ScoreManager,
// one of the existing Survival Shooter scripts.

namespace PestelLib.PremiumShop {
    // Deriving the Purchaser class from IStoreListener enables it to receive messages from Unity Purchasing.
    public class Purchaser : MonoBehaviour, IStoreListener
    {
        public static string ProcessingPaymentPrefab = "GenericMessageboxScreen";
        [Dependency] protected ILocalization _localizationData;
        [Dependency] protected Gui _gui;
        [Dependency] private IapValidatorClient _iapValidatorClient;

        private GenericMessageBoxScreen _processingPaymentScreen;

        public Action OnProductsLoaded = () => { };
        public event Action<Product, IapValidateResult> OnPurchaseValidated = (product, result) => { };
        public event Action<string> OnPurchaseSuccessfull;
        public event Action<Product> OnProductPurchased = (product) => { };
        public event Action<string> OnPurchaseFail = s => { };

        public bool PurchaseLoading { get; private set; }

        private static IStoreController m_StoreController; // The Unity Purchasing system.
        private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.
        private bool m_needRestore;    

        public Product[] Products
        {
            get { return _products; }
            private set { _products = value; }
        }

        private Product[] _products = new Product[0];
#if !NO_SUBSCRIPTIONS
        public List<SubscriptionInfo> subscriptions = new List<SubscriptionInfo>();
#endif

        // Product identifiers for all products capable of being purchased:
        // "convenience" general identifiers for use with Purchasing, and their store-specific identifier
        // counterparts for use with and outside of Unity Purchasing. Define store-specific identifiers
        // also on each platform's publisher dashboard (iTunes Connect, Google Play Developer Console, etc.)

        // General product identifiers for the consumable, non-consumable, and subscription products.
        // Use these handles in the code to reference which product to purchase. Also use these values
        // when defining the Product Identifiers on the store. Except, for illustration purposes, the
        // kProductIDSubscription - it has custom Apple and Google identifiers. We declare their store-
        // specific mapping to Unity Purchasing's AddProduct, below.
        //public static string kProductIDConsumable = "com.gdcompany.metalmadness.test_ingame_1a";
        //public static string kProductIDNonConsumable = "nonconsumable";
        //public static string kProductIDSubscription = "subscription";

        // Apple App Store-specific product identifier for the subscription product.
        private static string kProductNameAppleSubscription = "com.unity3d.subscription.new";

        // Google Play Store-specific product identifier subscription product.
        private static string kProductNameGooglePlaySubscription = "com.unity3d.subscription.original";
        protected Dictionary<string, string> IntroductoryInfoDict;

        protected virtual void Start() {
            ContainerHolder.Container.BuildUp(this);

			m_needRestore = Application.platform == RuntimePlatform.IPhonePlayer;

            var catalogue = ProductCatalog.LoadDefaultCatalog();

            // If we haven't set up the Unity Purchasing reference
            if (m_StoreController == null) {
                // Begin to configure our connection to Purchasing
                InitializePurchasing();
            }
        }

        public void InitializePurchasing() {
            // If we have already connected to Purchasing ...
            if (IsInitialized()) {
                // ... we are done here.
                return;
            }
            // Create a builder, first passing in a suite of Unity provided stores.
            var purchasingModule = StandardPurchasingModule.Instance();

            if (Application.isEditor)
            {
                purchasingModule.useFakeStoreUIMode = FakeStoreUIMode.StandardUser; //use FakeStoreUIMode.DeveloperUser to debug billing
            }

#if DEBUG_PROGRESS
            purchasingModule.useFakeStoreAlways = true;
            purchasingModule.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
#endif

            // Create a builder, first passing in a suite of Unity provided stores.
            var builder = ConfigurationBuilder.Instance(purchasingModule);
            #if UNITY_ANDROID && !IAP_UPDATED
                builder.Configure<IGooglePlayConfiguration>().SetPublicKey(GooglePublicLicenseKey());
            #endif

            AddAllProductsFromGameDefinitions(builder);

            /*
            // Add a product to sell / restore by way of its identifier, associating the general identifier
            // with its store-specific identifiers.
            builder.AddProduct(kProductIDConsumable, ProductType.Consumable);
            // Continue adding the non-consumable product.
            builder.AddProduct(kProductIDNonConsumable, ProductType.NonConsumable);
            // And finish adding the subscription product. Notice this uses store-specific IDs, illustrating
            // if the Product ID was configured differently between Apple and Google stores. Also note that
            // one uses the general kProductIDSubscription handle inside the game - the store-specific IDs
            // must only be referenced here.
            builder.AddProduct(kProductIDSubscription, ProductType.Subscription, new IDs()
            {
                {kProductNameAppleSubscription, AppleAppStore.Name},
                {kProductNameGooglePlaySubscription, GooglePlay.Name},
            });
            */

            // Kick off the remainder of the set-up with an asynchrounous call, passing the configuration
            // and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.


            FinalizeInitialization(builder);
        }

        protected virtual void FinalizeInitialization(ConfigurationBuilder builder)
        {
            UnityPurchasing.Initialize(this, builder);
        }

        public bool IsInitialized() {
            // Only say we are initialized if both the Purchasing references are set.
            return m_StoreController != null && m_StoreExtensionProvider != null;
        }

        /*
        public void BuyConsumable() {
            // Buy the consumable product using its general identifier. Expect a response either
            // through ProcessPurchase or OnPurchaseFailed asynchronously.
            BuyProductID(kProductIDConsumable);
        }


        public void BuyNonConsumable() {
            // Buy the non-consumable product using its general identifier. Expect a response either
            // through ProcessPurchase or OnPurchaseFailed asynchronously.
            BuyProductID(kProductIDNonConsumable);
        }


        public void BuySubscription() {
            // Buy the subscription product using its the general identifier. Expect a response either
            // through ProcessPurchase or OnPurchaseFailed asynchronously.
            // Notice how we use the general product identifier in spite of this ID being mapped to
            // custom store-specific identifiers above.
            BuyProductID(kProductIDSubscription);
        }
        */

        public virtual bool BuyProductID(string productId)
        {
            return BuyProductID(productId, string.Empty);
        }

        public virtual bool BuyProductID(string productId, string developerPayload)
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorPrefs.GetBool("AppleScreens"))
            {
                var screenShotDir = Application.dataPath.Replace("/Assets", string.Empty) + "/PremiumShopScreenshots";
                if (!Directory.Exists(screenShotDir))
                {
                    Directory.CreateDirectory(screenShotDir);
                }

                var path = string.Format("{0}/{1}.png", screenShotDir, productId);
                ScreenCapture.CaptureScreenshot(path, 1);
                Debug.Log("Making screenshot" + path);
                throw new Exception("Screenshot exception (it's ok - we don't want to make purchase)");
            }
#endif
            if (_processingPaymentScreen != null)
            {
                Debug.LogError("Payment process has already startet before");
                return false;
            }

            _processingPaymentScreen = GenericMessageBoxScreen.Show(new GenericMessageBoxDef
            {
                Caption = _localizationData.Get("ProcessingPaymentCaption"),
                Description = _localizationData.Get("ProcessingPaymentDescription"),
                AutoHide = false,
                Prefab = ProcessingPaymentPrefab
            });
            
            // If Purchasing has been initialized ...
            if (IsInitialized()) {
                // ... look up the Product reference with the general product identifier and the Purchasing
                // system's products collection.
                Product product = m_StoreController.products.WithID(productId);

                // If the look up found a product for this device's store and that product is ready to be sold ...
                if (product != null && product.availableToPurchase) {
                    Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));

                    PurchaseLoading = true;

                    CommandProcessor.Process<object, PremiumShopModule_SetDeveloperPayload>(
                        new PremiumShopModule_SetDeveloperPayload
                        {
                            developerPayload = developerPayload
                        }
                    );

                    // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed
                    // asynchronously.
                    m_StoreController.InitiatePurchase(product, developerPayload);
                    return true;
                }
                // Otherwise ...
                else {
                    // ... report the product look-up failure situation
                    Debug.Log(
                        "BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");

                    return false;
                }
            }
            // Otherwise ...
            else {
                // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or
                // retrying initiailization.
                Debug.Log("BuyProductID FAIL. Not initialized.");
                return false;
            }
        }


        // Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google.
        // Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
        public void RestorePurchases() {
            // If Purchasing has not yet been set up ...
            if (!IsInitialized()) {
                // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
                Debug.Log("RestorePurchases FAIL. Not initialized.");
                m_needRestore = true;
                return;
            }

            // If we are running on an Apple device ...
            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer) {
                // ... begin restoring purchases
                //Debug.Log("RestorePurchases started ...");

                // Fetch the Apple store-specific subsystem.
                var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
                // Begin the asynchronous process of restoring purchases. Expect a confirmation response in
                // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
                apple.RestoreTransactions((result) => {
                    // The first phase of restoration. If no more responses are received on ProcessPurchase then
                    // no purchases are available to be restored.
                    //Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
                });
            }
            // Otherwise ...
            else {
                // We are not running on an Apple device. No work is necessary to restore purchases.
                Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            }
        }

        public void UpdateSubscriptions(bool withLog = false)
        {
            if (m_StoreController == null) return;

#if !NO_SUBSCRIPTIONS
            subscriptions.Clear();
#endif
            Products = m_StoreController.products.all;
            StringBuilder purchaserLog = new StringBuilder();
            foreach (var item in m_StoreController.products.all)
            {
                if (item.receipt != null)
                {
                    purchaserLog.AppendLine("Item type: " + item.definition.type);
                    if (item.definition.type == ProductType.Subscription)
                    {
                        ProcessSubscription(item, IntroductoryInfoDict);
                    }
                }
            }

            if (withLog)
            {
                Debug.Log("PURCHASER: UpdateSubscriptions: " + purchaserLog);
            }
        }

        //
        // --- IStoreListener
        //
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
            // Purchasing has succeeded initializing. Collect our Purchasing references.
            Debug.Log("PURCHASER: OnInitialized: PASS");

            // Overall Purchasing system, configured with products for this application.
            m_StoreController = controller;
            // Store specific subsystem, for accessing device-specific store features.
            m_StoreExtensionProvider = extensions;

            var appleExtensions = extensions.GetExtension<IAppleExtensions>();
#if !NO_SUBSCRIPTIONS
            IntroductoryInfoDict = appleExtensions.GetIntroductoryPriceDictionary();
#else
            IntroductoryInfoDict = null;
#endif

            Products = controller.products.all;
            StringBuilder purchaserLog = new StringBuilder();
            foreach (var item in controller.products.all) {
                if (item.availableToPurchase) {
                    purchaserLog.AppendLine(string.Join(" - ",
                        new[]
                        {
                         item.metadata.localizedTitle,
                         item.metadata.localizedDescription,
                         item.metadata.isoCurrencyCode,
                         item.metadata.localizedPrice.ToString(),
                         item.metadata.localizedPriceString,
                         item.definition.id,
                         item.definition.type.ToString()
                        }));
                }
            }
            Debug.Log("PURCHASER: OnInitialized: " + purchaserLog);

            UpdateSubscriptions(true);

            if (m_needRestore)
            {
                m_needRestore = false;
                RestorePurchases();
            }

            OnProductsLoaded();
        }

        void ProcessSubscription(Product item, Dictionary<string, string> introductory_info_dict) {
            HideProcessingMessageBox();

            StringBuilder subscriptionLog = new StringBuilder();

            var receipt = JsonConvert.DeserializeObject<UnityIAPReceipt>(item.receipt);
            if (string.IsNullOrEmpty(receipt.Payload))
            {
                Debug.LogError("PURCHASER: Strange king of receipt: no payload. Item id: " + item.definition.id + " receipt: " + item.receipt);
                return;
            }

            if (checkIfProductIsAvailableForSubscriptionManager(item.receipt)) {
                string intro_json = (introductory_info_dict == null || !introductory_info_dict.ContainsKey(item.definition.storeSpecificId)) ? null : introductory_info_dict[item.definition.storeSpecificId];
#if !NO_SUBSCRIPTIONS
                SubscriptionManager p = new SubscriptionManager(item, intro_json);
                SubscriptionInfo info = p.getSubscriptionInfo();
                subscriptions.Add(info);
                subscriptionLog.AppendLine("=========================================\nproduct id is: " + info.getProductId() + "\n=========================================");
                subscriptionLog.AppendLine("purchase date is: " + info.getPurchaseDate());
                subscriptionLog.AppendLine("subscription next billing date is: " + info.getExpireDate());
                subscriptionLog.AppendLine("is subscribed? " + info.isSubscribed().ToString());
                subscriptionLog.AppendLine("=========================================\nis expired? " + info.isExpired().ToString() + "\n=========================================");
                subscriptionLog.AppendLine("is cancelled? " + info.isCancelled());
                subscriptionLog.AppendLine("product is in free trial peroid? " + info.isFreeTrial());
                subscriptionLog.AppendLine("product is auto renewing? " + info.isAutoRenewing());
                subscriptionLog.AppendLine("subscription remaining valid time until next billing date is: " + info.getRemainingTime());
                subscriptionLog.AppendLine("is this product in introductory price period? " + info.isIntroductoryPricePeriod());
                subscriptionLog.AppendLine("the product introductory localized price is: " + info.getIntroductoryPrice());
                subscriptionLog.AppendLine("the product introductory price period is: " + info.getIntroductoryPricePeriod());
                subscriptionLog.AppendLine("the number of product introductory price period cycles is: " + info.getIntroductoryPricePeriodCycles());
#endif
            } else {
                subscriptionLog.AppendLine("This product is not available for SubscriptionManager class, only products that are purchase by 1.19+ SDK can use this class.");
            }
            Debug.Log("PURCHASER: " + subscriptionLog);
        }

        private bool checkIfProductIsAvailableForSubscriptionManager(string receipt) {
            var receipt_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(receipt);
            if (!receipt_wrapper.ContainsKey("Store") || !receipt_wrapper.ContainsKey("Payload")) {
                Debug.Log("The product receipt does not contain enough information");
                return false;
            }
            var store = (string)receipt_wrapper ["Store"];
            var payload = (string)receipt_wrapper ["Payload"];

            if (payload != null ) {
                switch (store) {
                case "GooglePlay":
                    {
                        var payload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(payload);
                        if (!payload_wrapper.ContainsKey("json")) {
                            Debug.Log("The product receipt does not contain enough information, the 'json' field is missing");
                            return false;
                        }
                        var original_json_payload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode((string)payload_wrapper["json"]);
                        if (original_json_payload_wrapper != null && !original_json_payload_wrapper.ContainsKey("developerPayload")) {
                            Debug.Log("The product receipt does not contain enough information, the 'developerPayload' field is missing");
                            return false;
                        }
                        var developerPayloadJSON = (string)original_json_payload_wrapper["developerPayload"];
                        var developerPayload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(developerPayloadJSON);
                        if (developerPayload_wrapper == null || !developerPayload_wrapper.ContainsKey("is_free_trial") || !developerPayload_wrapper.ContainsKey("has_introductory_price_trial")) {
                            Debug.Log("The product receipt does not contain enough information, the product is not purchased using 1.19 or later");
                            return false;
                        }
                        return true;
                    }
                case "AppleAppStore":
                case "MacAppStore":
                    {
                        return true;
                    }
                default:
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        /*
         * Получение developerPayload - параметров, которые были переданы гуглу при запросе покупки.
         * Поддерживается только в гугл сторе.
         * Основано на методе checkIfProductIsAvailableForSubscriptionManager из стандартных примеров юнити
         *
         * Пример receipt который присылает юнити на андроиде:
{
    INAPP_PURCHASE_DATA = {
	    "orderId": "GPA.3379-9017-8987-96829",
	    "packageName": "com.gdcompany.metalmadness",
	    "productId": "com.gdcompany.metalmadness.group_b_crystal_pack_1",
	    "purchaseTime": 1551794194859,
	    "purchaseState": 0,
	    "developerPayload": "{\"developerPayload\":\"TXlUZXN0UGF5bG9hZA==\\n\",\"is_free_trial\":false,\"has_introductory_price_trial\":false,\"is_updated\":false}",
	    "purchaseToken": "mgckckmalimmnmdfflelnail.AO-J1OyTypLYlDLTS4SAhyT7BaVldvm1h-5KtYmGmSv5sykcknbeQYpbFw7EbuaZn6q0VlzRHSYjh3jHkfK1n2SlxCOlou9-F1MIyI_x9wbI3ySUOmju2eXBNK_qrueq5vJEuIRrSKZvlgcpHWHgnX539VYiKb-OVpN0CMDKhKUsU8aHAGQtkeE"
    },
    INAPP_DATA_SIGNATURE = LpSPUXFIVGq16vRSBKc46M6 / LrIIe590Q3qwwyZZTjYuniGKhqHQX6Q26g8cmK + hiA4Rpv04dF2xhv1094okaV / 0dUYbcuS7kpLe8jDq21Bu5q8rFGu6R0pqNRSjH0H9bq3cZMM0YYjXcEaM + 7JTVmHfW3UZkt + TMBH3nKnxxIXG / 0Gjg49dTMTIi0ylLRPUmHPnAXc5FZ6aOoHF8VudZVgd7dddk84YoCvLOS92IIbDcnq / 6Q9eXryMLWESXVdsM + 6eT8 + EZC7V4zb9FLCXrxxRb4dtZigSbKJkuQttwFk48hrPhIistnwpuVf1D + i4O2f0DxaRO + 4WOREgeAKPFQ == ,
    RESPONSE_CODE = 0
}
         */
        private string GetDeveloperPayload(string receipt)
        {
            var receiptWrapper = (Dictionary<string, object>)MiniJson.JsonDecode(receipt);
            if (!receiptWrapper.ContainsKey("Store") || !receiptWrapper.ContainsKey("Payload"))
            {
                Debug.Log("The product receipt does not contain enough information");
                return string.Empty;
            }
            var store = (string)receiptWrapper["Store"];

            if (store != "GooglePlay") return "";

            var payload = (string)receiptWrapper["Payload"];

            var payloadWrapper = (Dictionary<string, object>)MiniJson.JsonDecode(payload);
            if (!payloadWrapper.ContainsKey("json"))
            {
                Debug.Log("The product receipt does not contain enough information, the 'json' field is missing");
                return "";
            }
            var originalJsonPayloadWrapper = (Dictionary<string, object>)MiniJson.JsonDecode((string)payloadWrapper["json"]);
            if (originalJsonPayloadWrapper != null && !originalJsonPayloadWrapper.ContainsKey("developerPayload"))
            {
                Debug.Log("The product receipt does not contain enough information, the 'developerPayload' field is missing");
                return "";
            }

            //тут нет ошибки: developerPayload содержит внутри строку, в которой есть developerPayload - так уж приходят данные из юнити
            var googlePlayPayload = (string)originalJsonPayloadWrapper["developerPayload"];
            var googlePlayPayloadDict = (Dictionary<string, object>)MiniJson.JsonDecode(googlePlayPayload);
            if (googlePlayPayloadDict != null && !googlePlayPayloadDict.ContainsKey("developerPayload"))
            {
                Debug.Log("The product 'developerPayload' doesn't contain 'developerPayload'");
                return "";
            }
            
            var base64encodedDeveloperPayload = (string)googlePlayPayloadDict["developerPayload"];

            var payloadBytes = Convert.FromBase64String(base64encodedDeveloperPayload);
            return Encoding.UTF8.GetString(payloadBytes);
        }

        public void OnInitializeFailed(InitializationFailureReason error) {
            // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
            Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
            StartCoroutine(RetryInitialization());
        }

        private IEnumerator RetryInitialization()
        {
            yield return new WaitForSecondsRealtime(3f);
            InitializePurchasing();
        }

        private PurchaseProcessingResult ProcessPurchase(string productId, PurchaseEventArgs args)
        {
            HideProcessingMessageBox();
            PurchaseLoading = false;

            // A consumable product has been purchased by this user.
            if (m_StoreController.products.all.Any(x => String.Equals(x.definition.id, productId, StringComparison.Ordinal)))
            {
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", productId));
                // The consumable item has been successfully purchased, add 100 coins to the player's in-game score.
                var id = productId;
                AddPurchasedItemById(id, args);

                if (_iapValidatorClient != null)
                {
                    _iapValidatorClient.ValidateReceipt(args.purchasedProduct, (product, result) =>
                        {
                            try
                            {
                                OnPurchaseValidated?.Invoke(product, result);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError(e);
                            }

                            Debug.Log("Item type: " + args.purchasedProduct.definition.type);

                            if (args.purchasedProduct.definition.type == ProductType.Subscription)
                            {
                                ProcessSubscription(args.purchasedProduct, null);
                            }

                            if (result != null && result.IsValid)
                            {
                                OnPurchaseSuccessfull?.Invoke(productId);
                                OnProductPurchased?.Invoke(args.purchasedProduct);
                            }
                            else
                            {
                                OnPurchaseFail?.Invoke(productId);
                            }
                        });
                }
                else
                {
                    Debug.Log("Item type: " + args.purchasedProduct.definition.type);

                    if (args.purchasedProduct.definition.type == ProductType.Subscription)
                    {
                        ProcessSubscription(args.purchasedProduct, null);
                    }

                    OnPurchaseSuccessfull?.Invoke(productId);
                    OnProductPurchased?.Invoke(args.purchasedProduct);
                }
            }
            /*
            // Or ... a non-consumable product has been purchased by this user.
            else if (String.Equals(productId, kProductIDNonConsumable, StringComparison.Ordinal)) {
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", productId));
                // TODO: The non-consumable item has been successfully purchased, grant this item to the player.
            }
            // Or ... a subscription product has been purchased by this user.
            else if (String.Equals(productId, kProductIDSubscription,
                StringComparison.Ordinal)) {
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'",
                    productId));
                // TODO: The subscription item has been successfully purchased, grant this to the player.
            }
            */
            // Or ... an unknown product has been purchased by this user. Fill in additional products here....
            else
            {
                Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'",
                    productId));
            }

            // Return a flag indicating whether this product has completely been received, or if the application needs
            // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still
            // saving purchased products to the cloud, and when that save is delayed.
            return PurchaseProcessingResult.Complete;
        }

        private void HideProcessingMessageBox()
        {
            if (_processingPaymentScreen != null)
            {
                _gui.Close(_processingPaymentScreen.gameObject);
                _processingPaymentScreen = null;
            }
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            return ProcessPurchase(args.purchasedProduct.definition.id, args);
        }

        public virtual string GetPriceString(string productId, decimal multiplier = 1)
        {
            var product = Products.FirstOrDefault(x => x.definition.id == productId);
            if (product != null && product.metadata != null)
            {
                return GetPriceString(product.metadata.localizedPrice, product.metadata.isoCurrencyCode, multiplier);
            }
            else
            {
                return "N/A";
            }
        }

        public virtual string GetPriceStringWithCurrencySymbol(string productId)
        {
            var product = Products.FirstOrDefault(x => x.definition.id == productId);
            if (product != null && product.metadata != null)
            {
                return product.metadata.localizedPriceString;
            }
            else
            {
                return "N/A";
            }
        }

        private static string GetPriceString(decimal localizedPrice, string currencyCode, decimal multiplier = 1) {
            var resultingPrice = multiplier * localizedPrice;

            var roundedPrice = ((decimal)(int)(resultingPrice * 100m) / 100);
            var roundedPriceString = "";

            if (roundedPrice >= 100) {
                roundedPriceString = roundedPrice.ToString("N0");
            } else {
                roundedPriceString = roundedPrice.ToString("N2");
            }

            return roundedPriceString + " " + currencyCode;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            //на iOS приходится делать RestorePurchases при снятии игры с паузы
            //(т.к. игрок мог включить подписку через настройки, а не из приложения, и она должна обновиться сразу и включиться)
            //но ставиться на паузу так же и во время транзакций, а это приводит к тому, что приходит второй раз та транзакция,
            //которая сейчас выполняется
            Debug.Log("LOG - OnPurchaseFailed");
            if (failureReason == PurchaseFailureReason.DuplicateTransaction &&
                Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return;
            }

            HideProcessingMessageBox();
            PurchaseLoading = false;

            // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing
            // this reason with the user to guide their troubleshooting actions.

            var key = "SystemErrorPurchaseFailedDescription";
            var msg = _localizationData.Get(key);
            var keyFound = msg != key;

            var message = keyFound ?
                string.Format(msg, product.definition.storeSpecificId, failureReason) :
                string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason);

            Debug.Log(message);

            if (failureReason == PurchaseFailureReason.UserCancelled)
                return;

            //В редакторе при отмене покупки кидает этот код ошибки. Решили отключить совсем. 
#if UNITY_EDITOR
            if (failureReason == PurchaseFailureReason.PurchasingUnavailable)
                return;
#endif
            GenericMessageBoxScreen.Show(new GenericMessageBoxDef()
            {
                Caption = _localizationData.Get("SystemErrorPurchaseFailedCaption"),
                Description = message,
                ButtonAAction = () => { },
                ButtonALabel = _localizationData.Get("SystemOK"),
                AutoHide = true
            });
        }


        //PestelLib.PremiumShop.Purchaser implementation
        protected virtual void AddPurchasedItemById(string skuId, PurchaseEventArgs args)
        {
            var receipt = UnityPurchaseReceipt.FromString(args.purchasedProduct.receipt);
            receipt.DeveloperPayload = GetDeveloperPayload(args.purchasedProduct.receipt);

            CommandProcessor.Process<object, PremiumShopModule_ClaimItem>(
                new PremiumShopModule_ClaimItem
                {
                    skuId = skuId,
                    receipt = receipt
                }
            );
        }

        protected virtual string GooglePublicLicenseKey()
        {
            var settings = ContainerHolder.Container.Resolve<PurchaserSettings>();
            return settings.GooglePublicKey;
        }

        protected virtual void AddAllProductsFromGameDefinitions(ConfigurationBuilder builder)
        {
            var catalogue = ProductCatalog.LoadDefaultCatalog();

            foreach (var product in catalogue.allProducts)
            {
                var ids = new IDs();
                foreach (var storeID in product.allStoreIDs)
                {
                    ids.Add(storeID.id, storeID.store);
                }
                builder.AddProduct(product.id, product.type, ids);
            }
        }
    }
}