using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityDI;
using PestelLib.ServerShared;
using PestelLib.Serialization;
using PestelLib.SharedLogicBase;
using S;
using ServerShared;

namespace PestelLib.SharedLogic.Modules
{
    [System.Reflection.Obfuscation(ApplyToMembers=false)]
    public class PremiumShopModule : SharedLogicModule<PremiumShopModuleState>
    {
#if !UNITY_5_3_OR_NEWER
        private static readonly log4net.ILog serverLog = log4net.LogManager.GetLogger(typeof(PremiumShopModule));
#endif

        public ScheduledAction onPurchasesCountChanged;
        public int PurchasesCount { get { return State.PurchasesCount; } }

        [GooglePageRef("PremiumShopItems")] [Dependency] protected List<PremiumShopItemDef> _premiumShopItemDefs;
        [Dependency] protected Dictionary<string, PremiumShopItemDef> _premiumShopItemDefsDict;

        [GooglePageRef("PremiumShopClaims")] [Dependency] protected List<PremiumShopClaimDef> _premiumShopClaimDefs;

        public PremiumShopModule()
        {
            onPurchasesCountChanged = new ScheduledAction(ScheduledActionCaller);
        }

        [SharedCommand]
        internal void SetDeveloperPayload(string developerPayload)
        {
            State.DeveloperPayload = developerPayload;
        }

        [SharedCommand]
        internal void ClaimItem(string skuId, UnityPurchaseReceipt receipt)
        {
            IapValidateResult receiptValidResp;
            if (!SaveReceipt(receipt))
            {
                receiptValidResp = new IapValidateResult() { IsValid = false };
            }
            else
            {
#if UNITY_5_3_OR_NEWER
                receiptValidResp = new IapValidateResult() {IsValid = true};
#else
                receiptValidResp = ValidateReceipt(receipt);
#endif
            }

            if (!receiptValidResp.IsValid)
                throw new ResponseException(ResponseCode.IVALID_RECEIPT);

            var developerPayload = string.IsNullOrEmpty(receipt.DeveloperPayload)
                ? State.DeveloperPayload
                : receipt.DeveloperPayload;

            if (string.IsNullOrEmpty(developerPayload))
            {
                ClaimItemReward(skuId, receiptValidResp);
            }
            else
            {
                ClaimItemReward(skuId, receiptValidResp, developerPayload);
                State.DeveloperPayload = string.Empty;
            }

            State.PurchasesCount++;
        }


        protected virtual void ClaimItemReward(string skuId, IapValidateResult validatorResult, string developerPayload)
        {
            //TODO give reward to player in concrete shared logic module
            Log("Give reward to player with skuId: " + skuId + " and payload " + developerPayload);
        }

        protected virtual void ClaimItemReward(string skuId, IapValidateResult validatorResult)
        {
            //TODO give reward to player in concrete shared logic module
            Log("Give reward to player with skuId: " + skuId);
        }

        public override void MakeDefaultState()
        {
            base.MakeDefaultState();
            State.Transactions = new List<uint>();
        }

        private bool SaveReceipt(UnityPurchaseReceipt receipt)
        {
            var transCrc = Crc32.Compute(receipt.TransactionID);
            if (State.Transactions.Contains(transCrc))
            {
                var msg = "Transaction revalidate for " + receipt.TransactionID + ". Invalid receipt.";
                Log(msg);
#if !UNITY_5_3_OR_NEWER
                serverLog.Error(msg);
#endif
                return false;
            }
            State.Transactions.Add(transCrc);
            return true;
        }

        private IapValidateResult ValidateReceipt(UnityPurchaseReceipt receipt)
        {
            if (receipt.Store == UnityPurchaseReceipt.IOS)
            {
                return ValidateReceipt("ios", receipt.Payload);
            }
            if (receipt.Store == UnityPurchaseReceipt.ANDROID)
            {
                var d = Convert.ToBase64String(Encoding.UTF8.GetBytes(receipt.Payload));
                return ValidateReceipt("android", d);
            }
            if (receipt.Store == UnityPurchaseReceipt.FAKE)
            {
                return ValidateReceipt("fake", receipt.Payload);
            }
            throw new Exception(string.Format("Unsupported store '{0}'", receipt.Store));
        }

        private IapValidateResult ValidateReceipt(string platform, string receipt)
        {
            var query = new IapValidateQuery()
            {
                Platform = platform,
                Receipt = receipt,
                PlayerId = SharedLogic.PlayerId
            };
            
            var validator = Container.Resolve<IIapValidator>();
            if (validator != null)
            {
                return validator.IsValid(query, true);
            }
            else
            {
#if !UNITY_5_3_OR_NEWER
                serverLog.Error("Validator is null!!");
#endif
                return new IapValidateResult();
            }
        }

        public override byte[] SerializedState
        {
            get
            {
                return base.SerializedState;
            }
            set
            {
                base.SerializedState = value;
                State.Transactions = State.Transactions ?? new List<uint>(); //migration from older versions
            }
        }
    }
}
