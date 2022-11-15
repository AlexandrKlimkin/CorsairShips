using System;
using Newtonsoft.Json;

namespace PestelLib.ServerShared
{
    public class EnumConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsEnum;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var v = reader.Value.ToString();
            if (string.IsNullOrEmpty(v))
            {
                foreach (var e in Enum.GetValues(objectType))
                    return e;
            }

            return Enum.Parse(objectType, v);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}
