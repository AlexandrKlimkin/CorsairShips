using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;
using S;
using ServerShared.Sources.Numeric;

namespace FriendsClient.Sources.Serialization
{
    class ServerFriendMessagePackFormatResolver : IFormatterResolver
    {
        public static readonly ServerFriendMessagePackFormatResolver Instance = new ServerFriendMessagePackFormatResolver();

        private static Dictionary<Type, object> formatters = new Dictionary<Type, object>()
        {
            { typeof(MadId), MadIdFormatter.Instance }
        };

        ServerFriendMessagePackFormatResolver()
        {
        }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        class FormatterCache<T>
        {
            public static readonly IMessagePackFormatter<T> formatter;

            static FormatterCache()
            {
                object o;
                if (formatters.TryGetValue(typeof(T), out o))
                    formatter = (IMessagePackFormatter<T>) o;
                else
                    formatter = MessagePackSerializer.DefaultResolver.GetFormatter<T>();
            }
        }
    }
}