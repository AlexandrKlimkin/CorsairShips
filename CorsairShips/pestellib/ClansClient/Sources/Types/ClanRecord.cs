using System;
using MessagePack;
#if SERVER
using MongoDB.Bson.Serialization.Attributes;
#endif

namespace ClansClientLib
{
#if SERVER
    [BsonIgnoreExtraElements]
#endif
    [MessagePackObject()]
    public class ClanRecord
    {
#if SERVER
        [BsonId]
#endif
        [Key(0)]
        public Guid Id;
        [Key(1)]
        public ClanDesc Desc;
        [Key(2)]
        public int Level;
        [Key(3)]
        public int Rating;
        [Key(4)]
        public int TreasuryCurrent;
        [Key(5)]
        public int TreasuryTotal;
        [Key(6)]
        public Guid Owner;
        [Key(7)]
        public ClanPlayer[] Members;
        [Key(8)]
        public ClanBooster[] Boosters;
        [Key(9)]
        public int TopPlace;
    }
}