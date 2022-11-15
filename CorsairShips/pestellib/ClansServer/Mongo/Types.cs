using System;
using ClansClientLib;
using MongoDB.Bson.Serialization.Attributes;

namespace ClansServerLib.Mongo
{
    [BsonIgnoreExtraElements]
    class ClanBoosterRecord
    {
        [BsonId]
        public Guid Id;

        public ClanBooster Booster;
    }
}
