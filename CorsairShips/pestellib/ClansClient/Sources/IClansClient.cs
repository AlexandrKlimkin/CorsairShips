using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClansClientLib
{
    public interface IClansClient : IClanDB
    {
        // если происходит отключение от сервера, вызовется этот ивент с false
        // при подлючении вызовется с true
        // необходима запретить любые действия пользователя по отношению к кланам при их недоступности
        // если ShareTime десинкается то в этот ивент также придёт false, это также значит что нужно прекратить учитывать бусты клана
        event Action<bool> AvailabilityChange;
        bool Available { get; }

        event Action AskUpdateRequests;
        event Action AskUpdateBoosters;
        event Action AskUpdateClan;
        event Action AskUpdateConsumables;

        Task<ClanRequests> GetRequests(Guid clanId, Guid playerId, bool clanOwner);

        Guid AuthToken { get; set; }

    }
}
