using System;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using S;

namespace FriendsServer.Db.Mongo
{
    class MadIdSerializer : IBsonSerializer<MadId>
    {
        public static readonly MadIdSerializer Instance = new MadIdSerializer();
        MadIdSerializer()
        {
        }

        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var data = (uint) context.Reader.ReadInt32();
            return new MadId(data);
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, MadId value)
        {
            context.Writer.WriteInt32((int) (uint) value);
        }

        public MadId Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var data = (uint)context.Reader.ReadInt32();
            return new MadId(data);
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            Serialize(context, args, (MadId)value);
        }

        public Type ValueType => typeof(MadId);
    }

    class MadIdArraySerializer : ArraySerializer<MadId>
    {
        public static MadIdArraySerializer Instance = new MadIdArraySerializer();

        private MadIdArraySerializer()
            :base(MadIdSerializer.Instance)
        {
        }
    }
}