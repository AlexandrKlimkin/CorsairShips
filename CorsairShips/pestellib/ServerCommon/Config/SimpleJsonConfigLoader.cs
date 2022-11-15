using System;
using System.IO;
using Newtonsoft.Json;
using ServerLib;
using PestelLib.ServerShared;

namespace PestelLib.ServerCommon.Config
{
    public static class SimpleJsonConfigLoader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="saveDefault">если конфига по указанному пути нет, то попытаться создать его</param>
        /// <returns></returns>
        public static T LoadConfig<T>(bool saveDefault = false) where T : new()
        {
            var filePath = AppDomain.CurrentDomain.BaseDirectory + "App_Data" + Path.DirectorySeparatorChar + typeof(T).Name + ".json";
            return LoadConfigFromFile<T>(filePath, saveDefault);
        }

        public static T LoadConfigFromFile<T>(string filePath, bool saveDefault) where T : new()
        {
            uint h;
            return LoadConfigFromFile<T>(filePath, saveDefault, out h);
        }

        public static T LoadConfigFromFile<T>(string filePath, bool saveDefault, out uint hash) where T : new()
        {
            if (File.Exists(filePath))
            {
                var fileContent = File.ReadAllText(filePath);
                hash = Crc32.Compute(fileContent);
                return JsonConvert.DeserializeObject<T>(fileContent);
            }
            else
            {
                var result = new T();
                var data = JsonConvert.SerializeObject(result, Formatting.Indented);
                hash = Crc32.Compute(data);
                if (saveDefault)
                {
                    File.WriteAllText(filePath, data);
                }
                return result;
            }
        }

        public static T LoadConfigFromRedis<T>(string key, bool saveDefault) where T : new()
        {
            uint h;
            return LoadConfigFromRedis<T>(key, saveDefault, out h);
        }

        public static T LoadConfigFromRedis<T>(string key, bool saveDefault, out uint hash) where T : new()
        {
            var v = RedisUtils.Cache.StringGet(key);
            if (v.HasValue)
            {
                hash = Crc32.Compute(v.ToString());
                return JsonConvert.DeserializeObject<T>(v);
            }
            else
            {
                var result = new T();
                var data = JsonConvert.SerializeObject(result);
                hash = Crc32.Compute(data);
                if (saveDefault)
                {
                    RedisUtils.Cache.StringSet(key, data);
                }
                return result;
            }
        }
    }
}