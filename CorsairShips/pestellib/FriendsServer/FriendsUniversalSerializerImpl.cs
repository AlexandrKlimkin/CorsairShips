using System;
using FriendsClient.Sources.Serialization;
using PestelLib.UniversalSerializer;

namespace FriendsServer
{
    class FriendsUniversalSerializerImpl : ISerializer
    {
        public byte[] Serialize<T>(T obj)
        {
            return FriendServerSerializer.Serialize(obj);
        }

        public T Deserialize<T>(byte[] bytes)
        {
            return FriendServerSerializer.Deserialize<T>(bytes);
        }

        public object Deserialize(Type type, byte[] bytes)
        {
            return FriendServerSerializer.Deserialize(type, bytes);
        }
    }
}
