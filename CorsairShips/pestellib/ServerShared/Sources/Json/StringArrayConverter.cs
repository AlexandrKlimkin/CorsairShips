using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ArrayConverter<T> : JsonConverter {
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {

        T[] list = (T[])value;
        if (list.Length == 1) {
            value = list[0];
        }
        serializer.Serialize(writer, value);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {

       JToken token = JToken.Load(reader);
       var str = token.ToString();
       return JsonConvert.DeserializeObject<T[]>(str);
    }

    public override bool CanConvert(Type objectType) {
        return objectType == typeof(T[]);
    }
}