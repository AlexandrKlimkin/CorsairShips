using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PestelLib.ServerShared
{
    public class DefsParser
    {
        public static void PopulateObject(string value, object target)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new IntConverter());
            settings.Converters.Add(new FloatConverter());
            settings.Converters.Add(new EnumConverter());
            settings.Converters.Add(new StringConverter());
           settings.Converters.Add(new ArrayConverter<string>());
           settings.Converters.Add(new ArrayConverter<float>());
           settings.Converters.Add(new ArrayConverter<int>());
            JsonConvert.PopulateObject(value, target, settings);
        }
    }
}