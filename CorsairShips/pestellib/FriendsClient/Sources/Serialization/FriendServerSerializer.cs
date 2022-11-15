using System;
using MessagePack;

namespace FriendsClient.Sources.Serialization
{
    public static class FriendServerSerializer
    {
        public static byte[] Serialize<T>(T obj)
        {
            return MessagePackSerializer.Serialize(obj, ServerFriendMessagePackFormatResolver.Instance);
        }

        public static T Deserialize<T>(byte[] data)
        {
            return MessagePackSerializer.Deserialize<T>(data, ServerFriendMessagePackFormatResolver.Instance);
        }

        public static object Deserialize(Type type, byte[] bytes)
        {
            var deserializeMethod = typeof(FriendServerSerializer).GetMethod("Deserialize", new[] { typeof(byte[]) });
            var deserializeGenericMethod = deserializeMethod.MakeGenericMethod(type);
            return deserializeGenericMethod.Invoke(null, new object[] { bytes });
        }

        public static T Clone<T>(T obj)
        {
            var data = Serialize(obj);
            return Deserialize<T>(data);
        }
    }
}
