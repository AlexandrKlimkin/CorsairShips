using System;
using Newtonsoft.Json;

namespace PestelLib.ServerShared
{
    public class FloatConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(float);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var v = reader.Value.ToString();
            if (string.IsNullOrEmpty(v))
                return default(float);

            return float.Parse(v);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue((float)value);
        }
    }
}
