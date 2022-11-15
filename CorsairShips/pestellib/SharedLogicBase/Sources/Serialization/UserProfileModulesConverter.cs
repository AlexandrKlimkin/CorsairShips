using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using PestelLib.UniversalSerializer;

namespace S
{
    public class UserProfileModulesConverter : JsonConverter
    {
        public override bool CanWrite { get {
                return  Serializer.Implementation != null && 
                        Serializer.Implementation.GetType() == typeof(TextNewtonsoftJsonSerializer);
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dictionary<string, byte[]>) && 
                   Serializer.Implementation != null &&
                   Serializer.Implementation.GetType() == typeof(TextNewtonsoftJsonSerializer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                reader.Read();
                if (reader.TokenType == JsonToken.EndArray)
                    return new Dictionary<string, string>();
                else
                    throw new JsonSerializationException("Non-empty JSON array does not make a valid Dictionary!");
            }
            else if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                Dictionary<string, byte[]> ret = new Dictionary<string, byte[]>();
                reader.Read();
                while (reader.TokenType != JsonToken.EndObject)
                {
                    if (reader.TokenType != JsonToken.PropertyName)
                        throw new JsonSerializationException("Unexpected token!");
                    string key = (string)reader.Value;
                    reader.Read();
                    if (reader.TokenType != JsonToken.StartObject)
                        throw new JsonSerializationException("Unexpected token!");


                    var obj = serializer.Deserialize(reader);
                    var serializedObj = JsonConvert.SerializeObject(obj);
                    ret.Add(key, Encoding.UTF8.GetBytes(serializedObj));
                    reader.Read();
                }
                return ret;
            }
            else
            {
                throw new JsonSerializationException("Unexpected token!");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dict = (Dictionary<string, byte[]>) value;

            writer.WriteStartObject();

            foreach (var kv in dict)
            {
                writer.WritePropertyName(kv.Key);
                var moduleStateObj = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(kv.Value));
                serializer.Serialize(writer, moduleStateObj);
            }

            writer.WriteEndObject();
        }
    }
}