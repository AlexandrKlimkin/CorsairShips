using System;
using System.Collections.Generic;
using System.Text;

namespace ClansServerLib
{
    interface INotifyPlayers
    {
        void AskPlayerToUpdateHisClan(Guid playerId);
        void AskPlayerToUpdateRequests(Guid playerId);
        void AskClanToUpdateBoosters(Guid clanId);
        void AskClanToUpdate(Guid clanId);
        void AskClanToUpdateConsumables(Guid clanId);
    }
}
