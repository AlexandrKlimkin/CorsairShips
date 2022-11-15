using System;
using PestelLib.ServerCommon.Db;
using PestelLib.ServerShared;
using ServerLib.Modules;

namespace ServerLib.Code.PromoValidator
{
    public class PromoValidator : IPromoValidator
    {
        public PromoValidator(IPromoStorage promoStorage)
        {
            _promoStorage = promoStorage;
        }

        public bool Validate(string promoCode, string function, string parameter, Guid userId)
        {
            if (_promoStorage.UsedByPlayer(promoCode, userId))
            {
                //награда уже выдавалась этому пользователю
                return false;
            }

            var promoInfo = _promoStorage.Get(promoCode);

            if (promoInfo.Function != function) return false; //изменена функция промокода (читом)

            if (promoInfo.Parameter != parameter) return false; //изменён параметр промокода (читом)

            //если дошли до сюда, то все хорошо
            //регистрируем, что награда данному пользователю выдана
            return _promoStorage.BindPlayerToPromo(promoCode, userId);
        }

        private IPromoStorage _promoStorage;
    }
}