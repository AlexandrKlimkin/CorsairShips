using System;
using MessagePack;
#if SERVER
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
#endif

namespace ClansClientLib
{
    [MessagePackObject()]
#if SERVER
    [BsonIgnoreExtraElements]
#endif
    public class ClanConsumables
    {
#if SERVER
        [BsonId]
        [IgnoreMember]
        public ObjectId Id;
#endif
        [Key(0)]
        public Guid ClanId;
        [Key(1)]
        public int ConsumableId;
        [Key(2)]
        public int Balance;
    }
}
