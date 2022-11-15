using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Text;
using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using System.Reflection;
using log4net;
using Newtonsoft.Json.Utilities;

namespace PestelLib.MatchmakerShared
{

    public class MatchmakerMessageSerializer
    {
        static readonly ILog Log = LogManager.GetLogger(typeof(MatchmakerMessageSerializer));
        static Type[] baseTypes = new[] { typeof(MatchmakerMessage), typeof(MatchmakerRequest), typeof(IMatch), typeof(MatchingStats), typeof(ServerStats) };
        static Type[] knownTypes;
        static JsonSerializerSettings _jsonSerializerSettings
            = new JsonSerializerSettings()
            {
                Converters = new[] { new IsoDateTimeConverter() { DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK" } },
            };
        static Dictionary<string, Type> _lazyTypes = new Dictionary<string, Type>();

        static MatchmakerMessageSerializer()
        {
            var result = new List<Type>();
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (Type t in a.GetTypes())
                    {
                        foreach (var bt in baseTypes)
                        {
                            if (bt.IsAssignableFrom(t))
                                result.Add(t);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
            knownTypes = result.ToArray();
        }

        static Type GetSpecificType(MatchmakerMessage basicMessage)
        {
            Type result;
            if (_lazyTypes.TryGetValue(basicMessage.Type, out result))
                return result;

            var typeAndGenerics = basicMessage.Type.Split('[');
            if (typeAndGenerics.Length > 2)
                throw new Exception("Bad type format '" + basicMessage.Type + "'");
            var mainTypeName = typeAndGenerics[0];
            result = knownTypes.First(t => t.Name == mainTypeName);
            if (typeAndGenerics.Length > 1)
            {
                var genericsNames = typeAndGenerics[1].Replace("]", "").Split(',');
                var generics = new Type[genericsNames.Length];
                for (var i = 0; i < genericsNames.Length; ++i)
                {
                    var g = genericsNames[i];
                    var gt = knownTypes.First(t => t.Name == g);
                    generics[i] = gt;
                }
                result = result.MakeGenericType(generics);
            }
            _lazyTypes[basicMessage.Type] = result;
            return result;
        }

        public byte[] Serialize(MatchmakerMessage message)
        {
            var s = JsonConvert.SerializeObject(message, Formatting.None, _jsonSerializerSettings);
            return Encoding.UTF8.GetBytes(s);
        }

        public MatchmakerMessage Deserialize(byte[] data, int len)
        {
            var s = Encoding.UTF8.GetString(data, 0, len);
            var basicMessage = JsonConvert.DeserializeObject<MatchmakerMessage>(s, _jsonSerializerSettings);
            var specificType = GetSpecificType(basicMessage);
            return (MatchmakerMessage)JsonConvert.DeserializeObject(s, specificType, _jsonSerializerSettings);
        }
    }
}
