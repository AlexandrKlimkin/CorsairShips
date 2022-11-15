using MessagePack;

namespace ServerShared
{
    public static class MessagePackExtension
    {
        /// <summary>
        /// T must have MessagePackObject attribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <returns></returns>
        public static T Clone<T>(this T original)
        {
            var ser = MessagePackSerializer.Serialize(original);
            return MessagePackSerializer.Deserialize<T>(ser);
        }
    }
}
