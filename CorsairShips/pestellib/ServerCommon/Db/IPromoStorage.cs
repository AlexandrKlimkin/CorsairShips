using System;

namespace PestelLib.ServerCommon.Db
{
    public class PromoInfo
    {
        public string Id;
        public string Function;
        public string Parameter;
        public int ActivateMax;
        public int ActivateCount;
        public Guid[] ActivatedByPlayers = new Guid[] { };
    }

    public interface IPromoStorage
    {
        PromoInfo Get(string promoId);
        bool UsedByPlayer(string promoId, Guid playerId);
        /// <summary>
        /// Инкрементит ActivateCount если он меньше ActivateMax
        /// </summary>
        bool ActivatePromo(string promoId);
        /// <summary>
        /// Добавляет пользователя ActivatedByPlayers, если его там не было возвращает true
        /// </summary>
        bool BindPlayerToPromo(string promoId, Guid playerId);
        bool Create(string promoId, string func, string param, int count);
        bool Create(PromoInfo info);
    }
}
