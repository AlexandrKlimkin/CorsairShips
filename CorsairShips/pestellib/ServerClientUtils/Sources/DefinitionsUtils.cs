using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GoogleSpreadsheet;
using PestelLib.ServerShared;
using S;
using UnityEngine;
using PestelLib.Serialization;

namespace PestelLib.ServerClientUtils
{
    public class DefinitionsUtils
    {
        public static void UpdateDefinitions(DefsData defData, IGameDefinitions defs)
        {
            var defDict = new Dictionary<string, string>();
            for (var i = 0; i < defData.PageName.Count; i++)
            {
                //Debug.Log(defData.PageName[i]);
                defDict[defData.PageName[i]] = defData.Json[i];
            }

            foreach (
                FieldInfo fieldInfo in
                defs.GetType().GetFields(BindingFlags.Default | BindingFlags.Instance |
                                              BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (typeof(IDictionary).IsAssignableFrom(fieldInfo.FieldType)) continue;

                if (Attribute.IsDefined(fieldInfo, typeof(GooglePageRefAttribute)))
                {
                    var attr =
                        (GooglePageRefAttribute)
                        Attribute.GetCustomAttribute(fieldInfo, typeof(GooglePageRefAttribute));

                    //Debug.Log("Page: " + attr.PageName);

                    if (!defDict.ContainsKey(attr.PageName))
                    {
                        Debug.Log("Can't find page: attr.PageName " + attr.PageName);
                        continue;
                    }

                    var fileData = defDict[attr.PageName];

                    var newInstance = Activator.CreateInstance(fieldInfo.FieldType);
                    fieldInfo.SetValue(defs, newInstance);

                    DefsParser.PopulateObject(fileData, newInstance);
                }
            }

            defs.OnAfterDeserialize();
        }
    }
}