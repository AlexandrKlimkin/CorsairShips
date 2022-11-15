using System;

namespace ClansServerLib
{
    class ClanTreasuryAuditRecord
    {
        public Guid ClanId; // index 1
        public Guid PlayerId;
        public DateTime Date; // index 1
        public int Amount;
        public int Reason;
        public int ConsumableId;
        public bool IsConsumable;
    }
}