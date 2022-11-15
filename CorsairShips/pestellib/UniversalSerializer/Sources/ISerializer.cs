using System;

namespace PestelLib.UniversalSerializer
{
    public interface ISerializer
    {
        byte[] Serialize<T>(T obj);
        T Deserialize<T>(byte[] bytes);
        object Deserialize(Type type, byte[] bytes);
    }
}