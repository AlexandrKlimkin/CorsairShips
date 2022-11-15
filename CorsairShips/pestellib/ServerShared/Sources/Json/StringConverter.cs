using System;
using Newtonsoft.Json;

public class StringConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue((string)value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return "";

            return reader.Value.ToString();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
    }