using System;
using MessagePack;

namespace PestelLib.UniversalSerializer
{
    public class BinaryMessagePackSerializer : ISerializer
    {
        public byte[] Serialize<T>(T obj)
        {
            return MessagePackSerializer.Serialize(obj);
        }

        public T Deserialize<T>(byte[] bytes)
        {
            return MessagePackSerializer.Deserialize<T>(bytes);
        }

        public object Deserialize(Type type, byte[] bytes)
        {
            var deserializeMethod = typeof(MessagePackSerializer).GetMethod("Deserialize", new[] { typeof(byte[]) });
            var deserializeGenericMethod = deserializeMethod.MakeGenericMethod(type);
            return deserializeGenericMethod.Invoke(this, new object[] { bytes });
        }
    }
}