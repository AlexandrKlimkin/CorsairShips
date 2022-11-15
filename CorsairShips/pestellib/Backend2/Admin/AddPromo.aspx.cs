using System;
using BackendCommon.Code;
using PestelLib.ServerCommon.Db;

namespace Server.Admin
{
    public partial class AddPromo : System.Web.UI.Page
    {
        public AddPromo()
        {
            _promoStorage = MainHandlerBase.ServiceProvider.GetService(typeof(IPromoStorage)) as IPromoStorage;
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            this.registerNewPromo.Click += RegisterNewPromoOnClick;
        }

        private void RegisterNewPromoOnClick(object sender, EventArgs eventArgs)
        {
            if (string.IsNullOrEmpty(function.Text))
            {
                this.statusLine.Text += "Error: You have to specify function<br>";
                return;
            }

            var promoCount = int.Parse(this.count.Text);

            if (promoCount < 1 || promoCount > 10000)
            {
                this.statusLine.Text += "Error: Promo count have to be in range 1...10000<br>";
                return;
            }

            var promoInfo = _promoStorage.Get(promoCode.Text);
            if (promoInfo != null)
            {
                this.statusLine.Text += "Error: Promo with code " + promoCode.Text + " already registred in database<br>";
                return;
            }

            var promoFunc = this.function.Text;

            if (!_promoStorage.Create(promoCode.Text, promoFunc, parameter.Text, promoCount))
            {
                this.statusLine.Text += "Error: Promo with code " + promoCode.Text + " already registred in database<br>";
                return;
            }

            var log = string.Format("New promo: {0} {1}({2}) for {3} users<br/>", promoCode.Text, promoFunc, this.parameter.Text, promoCount);

            this.statusLine.Text += log;
        }

        private IPromoStorage _promoStorage;
    }
}