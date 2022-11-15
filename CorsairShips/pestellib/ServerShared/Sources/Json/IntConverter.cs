using System;
using Newtonsoft.Json;

namespace PestelLib.ServerShared
{
    public class IntConverter : JsonConverter {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            writer.WriteValue((int)value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var v = reader.Value.ToString();
            if (string.IsNullOrEmpty(v)) 
                return 0;

            return int.Parse(v);
        }

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(int);
        }
    }
}