using PestelLib.ServerShared;
using PestelLib.SharedLogicBase;
using S;

namespace PestelLib.SharedLogic.Modules
{
    public class PromoModule : SharedLogicModule<PromoModuleState>
    {
        [SharedCommand]
        internal void UsePromo(string promocode, string function, string parameter)
        {
            //это будет что-то делать только на сервере, где будет зарегистрирован IPromoValidator
            //валидатор так же регистрирует этот промокод использованным
            //при ошибке валидации - эксепшн, до выдачи бонуса дело не доходит
            ServerCheck(promocode, function, parameter);

            GivePromoBonusToPlayer(function, parameter);
        }

        //для фактической выдачи игроку чего-либо, нужно заоверрайдить эту функцию в классе наследующем PromoModule
        protected virtual void GivePromoBonusToPlayer(string function, string parameter)
        {
            Log("GivePromoBonusToPlayer: " + function + ": " + parameter);
            //например:
            /*
            switch (function)
            {
                case "add_gold":
                    var amount = int.Parse(parameter);
                    MoneyModule.AddGold(amount);
                    //тут можно ещё добавить ScheduledAction, для отображения пользователю окна - вы получили столько-то денег
                    break;
                case "add_ship":
                    ShipModule.GiveShip(parameter);
                    break;
            }*/
        }

        private void ServerCheck(string promoCode, string function, string parameter)
        {
            var promoValidator = Container.Resolve<IPromoValidator>();
            if (promoValidator != null && !promoValidator.Validate(promoCode, function, parameter, SharedLogic.PlayerId))
            {
                throw new SharedLogicException(SharedLogicException.SharedLogicExceptionType.PROMO_CODE_ERROR, "Can't validate promo code: " + promoCode);
            }
        }
    }
}