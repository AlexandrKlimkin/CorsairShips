using System;

namespace PestelLib.ServerShared
{
    public interface IPromoValidator
    {
        bool Validate(string promoCode, string function, string parameter, Guid userId);
    }
}