using System;
using System.Linq;
using MessagePack;
using PestelLib.ServerShared;
using PestelLib.ServerCommon.Db;
using S;

namespace ServerLib.Modules
{
    public class PromoModule : IModule
    {
        public PromoModule(IServiceProvider serviceProvider)
        {
            _promoStorage = serviceProvider.GetService(typeof(IPromoStorage)) as IPromoStorage;
        }

        public ServerResponse ProcessCommand(ServerRequest serverRequest)
        {
            var request = serverRequest.Request.UsePromo;
            var guid = new Guid(serverRequest.Request.UserId);

            var promoInfo = _promoStorage.Get(request.Promo);
            if (promoInfo == null)
            {
                return MakeResponse(PromoCodeResponseCode.NO_PROMO);
            }

            if (promoInfo.ActivatedByPlayers.Contains(guid))
            {
                return MakeResponse(PromoCodeResponseCode.ALREADY);
            }

            if (!_promoStorage.ActivatePromo(request.Promo))
            {
                return MakeResponse(PromoCodeResponseCode.LIMIT_MAX);
            }

            var resp = new UsePromoResponse
            {
                PromoResponseCode = PromoCodeResponseCode.ACTIVATED,
                Parameter = promoInfo.Parameter,
                Function = promoInfo.Function,
                PromoCode = request.Promo
            };

            return new ServerResponse()
            {
                ResponseCode = ResponseCode.OK,
                Data = MessagePackSerializer.Serialize(resp)
            };
        }

        private static ServerResponse MakeResponse(PromoCodeResponseCode code)
        {
            var resp = new UsePromoResponse { PromoResponseCode = code };
            return new ServerResponse
            {
                ResponseCode = ResponseCode.OK,
                Data = MessagePackSerializer.Serialize(resp)
            };
        }

        private IPromoStorage _promoStorage;
    }
}