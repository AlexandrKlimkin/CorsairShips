using System;
using System.Text;
using Newtonsoft.Json;
using PestelLib.UniversalSerializer;

namespace S
{
    public class CommandDataConverter : JsonConverter
    {
        public override bool CanWrite
        {
            get
            {
                return Serializer.Implementation != null &&
                       Serializer.Implementation.GetType() == typeof(TextNewtonsoftJsonSerializer);
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(byte[]) &&
                   Serializer.Implementation != null && 
                   Serializer.Implementation.GetType() == typeof(TextNewtonsoftJsonSerializer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            { 
                byte[] ret = null;
                
                var obj = serializer.Deserialize(reader);
                var serializedObj = JsonConvert.SerializeObject(obj);
                ret = Encoding.UTF8.GetBytes(serializedObj);

                return ret;
            }
            else
            {
                throw new JsonSerializationException("Unexpected token!");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var byteArray = (byte[])value;

            var commandObject = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(byteArray));
            serializer.Serialize(writer, commandObject);
        }
    }
}