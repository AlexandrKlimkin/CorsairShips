using System;
using System.Text;
using Newtonsoft.Json;

namespace PestelLib.UniversalSerializer
{
    public class TextNewtonsoftJsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        public byte[] Serialize<T>(T obj)
        {
            var str = JsonConvert.SerializeObject(obj, Formatting.Indented, _settings);
            return Encoding.UTF8.GetBytes(str);
        }

        public T Deserialize<T>(byte[] bytes)
        {
            var str = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(str, _settings);
        }

        public object Deserialize(Type type, byte[] bytes)
        {
            var str = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject(str, type, _settings);
        }
    }
}