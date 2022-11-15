using MessagePack;
using PestelLib.Localization;
using PestelLib.ServerClientUtils;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicClient;
using S;
using UnityEngine;
using UnityEngine.UI;
using UnityDI;

namespace PestelLib.Promo
{
    public class ExamplePromoModuleScreen : MonoBehaviour
    {
        [Dependency] private PromoModule _promoModule;
        [Dependency] private RequestQueue _requestQueue;
        [Dependency] private ILocalization _localization;

        [SerializeField] private InputField _promoCode;
        [SerializeField] private Text _statusLine;
        [SerializeField] private Button _button;

        private void Start()
        {
            ContainerHolder.Container.BuildUp(this);
            _button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            _requestQueue.SendRequest("Promo", new Request {
                UsePromo = new UsePromo
                {
                    Promo = _promoCode.text
                }
            }, OnRequestCompleted);
        }

        private void OnRequestCompleted(Response response, DataCollection dataCollection)
        {
            var resp = MessagePackSerializer.Deserialize<UsePromoResponse>(dataCollection.Data);
            OnPromoReceived(resp);
        }

        protected virtual void StatusChange(string message, Color color)
        {
            _statusLine.text = message;
            _statusLine.color = color;
        }

        public void OnPromoReceived(UsePromoResponse resp)
        {
            switch (resp.PromoResponseCode)
            {
                case PromoCodeResponseCode.ALREADY:
                    StatusChange(_localization.Get("PromoAlreadyActivated"), Color.red);
                    break;

                case PromoCodeResponseCode.NO_PROMO:
                    StatusChange(_localization.Get("BadPromoCode"), Color.red);
                    break;

                case PromoCodeResponseCode.LIMIT_MAX:
                    StatusChange(_localization.Get("PromocodeLimitReached"), Color.red);
                    break;

                default:
                    if (resp.PromoResponseCode == PromoCodeResponseCode.ACTIVATED)
                    {
                        var cmd = new S.PromoModule_UsePromo
                        {
                            function = resp.Function,
                            parameter = resp.Parameter,
                            promocode = resp.PromoCode
                        };
                        CommandProcessor.Process<object, PromoModule_UsePromo>(cmd);

                        StatusChange(_localization.Get("PromocodeActivated"), Color.green);
                    }
                    else
                    {
                        Debug.Log("Nothing");
                    }
                    break;
            }
        }
    }
}