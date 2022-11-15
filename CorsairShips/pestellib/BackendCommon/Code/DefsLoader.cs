using System;
using System.Collections;
using System.IO;
using System.Reflection;
using GoogleSpreadsheet;
using PestelLib.Serialization;
using PestelLib.ServerShared;
using S;
using ServerLib;

namespace BackendCommon.Code
{
    public class DefsLoader
    {
        public static DefsData DefsData;

        public static T Load<T>() where T : IGameDefinitions, new()
        {
            var defs = new T();
            return (T)Load((object) defs);
        }

        public static IGameDefinitions Load(Type defType)
        {
            var defs = Activator.CreateInstance(defType);
            return Load(defs);
        }

        private static IGameDefinitions Load(object defs)
        {
            DefsData = new DefsData();

            //string path = HostingEnvironment.MapPath("/App_Data/");
            string path = AppDomain.CurrentDomain.BaseDirectory + "/App_Data/";

            var defType = defs.GetType();

            FIllInDefinitions(defType, path, defs);
            var result = (IGameDefinitions)defs;
            result.OnAfterDeserialize();
            return result;
        }

        private static void FIllInDefinitions(Type defType, string path, object defs)
        {
            foreach (
                FieldInfo fieldInfo in
                defType.GetFields(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public |
                                  BindingFlags.NonPublic))
            {
                if (typeof(IDictionary).IsAssignableFrom(fieldInfo.FieldType)) continue;

                if (Attribute.IsDefined(fieldInfo, typeof(GooglePageRefAttribute)))
                {
                    var attr =
                        (GooglePageRefAttribute)
                        Attribute.GetCustomAttribute(fieldInfo, typeof(GooglePageRefAttribute));

                    var newInstance = Activator.CreateInstance(fieldInfo.FieldType);
                    fieldInfo.SetValue(defs, newInstance);

                    var fileName = string.Format("{0}/{1}.json", path, attr.PageName);
                    string fileData = "{}";
                    if (!AppSettings.Default.SkipMissingDefs || File.Exists(fileName))
                    {
                        fileData = File.ReadAllText(fileName);
                        DefsParser.PopulateObject(fileData, newInstance);
                    }

                    DefsData.Json.Add(fileData);
                    DefsData.PageName.Add(attr.PageName);
                }
            }
        }
    }
}