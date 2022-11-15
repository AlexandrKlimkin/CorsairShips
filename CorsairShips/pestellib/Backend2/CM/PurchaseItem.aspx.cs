using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.UI.WebControls;
using BackendCommon.Code;
using BackendCommon.Code.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PestelLib.ServerShared;
using Server;
using ServerLib;
using BackendCommon.Code.Utils;

namespace Backend.Admin
{
    public partial class PurchaseItem : System.Web.UI.Page
    {
        public PurchaseItem()
        {
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            giveItem.Click += GiveItemOnClick;

            var iaps = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\PremiumShopItems.json";
            var iapsContent = File.ReadAllText(iaps);
            var iapsList = JsonConvert.DeserializeObject<List<JObject>>(iapsContent);
            var skuListValues = iapsList.Select(x => x.GetValue("SkuId").Value<string>());
            var lst = skuListValues.Distinct().Select(x => new ListItem(x)).ToArray();
            skuList.Items.AddRange(lst);

            UpdateTransactions();
        }

        private void UpdateTransactions()
        {
            transactions.Text = string.Join("\n",
                RedisUtils.Cache.ListRange("SupportTransactions", 0, 100).Select(x => x.ToString()).ToArray());
        }

        private void GiveItemOnClick(object sender, EventArgs eventArgs)
        {
            Guid id = PlayerIdHelper.FromString(guid.Text);
            if (id == Guid.Empty)
            {
                statusLine.Text = "Can't parse user id";
                return;
            }

            if (skuList.SelectedIndex == -1)
            {
                statusLine.Text = "please select sku id";
                return;
            }

            if (!StateLoader.Storage.UserExist(id))
            {
                statusLine.Text = "User doesn't exist";
                return;
            }

            var bytes = StateLoader.LoadBytes(MainHandlerBase.ConcreteGame, id, null, 0, out id);

            var sl = MainHandlerBase.ConcreteGame.SharedLogic(bytes, MainHandlerBase.FeaturesCollection);

            var fileName = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\PestelLib.ConcreteSharedLogic.dll";
            if (!File.Exists(fileName))
            {
                statusLine.Text = "Concrete shared logic dll not found";
                return;
            }
            
            var assembly = Assembly.LoadFrom(fileName);
            var premiumShopModuleType = assembly.GetTypes().FirstOrDefault(t => t.Name == "PremiumShopModule");

            if (premiumShopModuleType == null)
            {
                statusLine.Text = "PremiumShopModule not found";
                return;
            }

            var concreteGamePremiumShop = ServerReflectionUtils.GetTheMostDerivedType(assembly, premiumShopModuleType);
            var claimItem = concreteGamePremiumShop.GetMethod("ClaimItemReward", 
                BindingFlags.Instance | BindingFlags.NonPublic, 
                Type.DefaultBinder, 
                CallingConventions.Any, 
                new []
                {
                    typeof(string),
                    typeof(IapValidateResult)
                }, new ParameterModifier[0]
            );

            if (claimItem == null)
            {
                statusLine.Text = "ClaimItemReward method not found";
                return;
            }

            sl.CommandTimestamp = DateTime.UtcNow;
            sl.CommandSerialNumber = sl.CommandSerialNumber + 1;

            var concretePremiumShopModuleInstance = sl.Container.Resolve(concreteGamePremiumShop);
            claimItem.Invoke(concretePremiumShopModuleInstance, new object[] { skuList.SelectedItem.Text, new IapValidateResult { IsValid = true} });
            
            StateLoader.Save(id, sl.SerializedState, Guid.NewGuid().ToString());

            statusLine.Text = "Success!";

            RedisUtils.Cache.ListLeftPush("SupportTransactions", DateTime.UtcNow + " " + skuList.SelectedItem.Text + " " + receipt.Text);
            UpdateTransactions();
        }

        protected void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}