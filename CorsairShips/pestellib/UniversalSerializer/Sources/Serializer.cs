using System;

namespace PestelLib.UniversalSerializer
{
    public class Serializer
    {
        public static ISerializer Implementation = new BinaryMessagePackSerializer();

        public static void SetBinaryMode()
        {
            Implementation = new BinaryMessagePackSerializer();
        }

        public static void SetTextMode()
        {
            Implementation = new TextNewtonsoftJsonSerializer();
        }

        public static byte[] Serialize<T>(T obj)
        {
            return Implementation.Serialize<T>(obj);
        }

        public static T Deserialize<T>(byte[] bytes)
        {
            return Implementation.Deserialize<T>(bytes);
        }

        public static object Deserialize(Type type, byte[] bytes)
        {
            return Implementation.Deserialize(type, bytes);
        }
    }
}