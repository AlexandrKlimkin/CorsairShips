using System;
using System.Collections.Generic;
using PestelLib.Serialization;
using PestelLib.SharedLogicBase;
using UnityDI;
using System.Globalization;

namespace PestelLib.SharedLogic.Modules
{
    [System.Reflection.Obfuscation(ApplyToMembers=false)]
    public class SettingsModuleBase : SharedLogicModule<SettingsModuleState>
    {
        [GooglePageRef("Settings")] [Dependency]
        protected List<SettingDef> _settingsDefs;

        [Dependency] protected Dictionary<string, SettingDef> _settingsDict;
        protected Dictionary<string, object> _cache = new Dictionary<string, object>();

        public int LeaguesTopAmountGlobal
        {
            get { return GetValue("LeaguesTopGlobalAmount", 500); }
        }

        public int LeaguesTopAmountDivision
        {
            get { return GetValue("LeaguesTopDivisionAmount", 500); }
        }

        public T GetValue<T>(string key, T defaultValue = default(T), bool required = false)
        {
            var t = typeof(T);
            var cacheKey = key + t.FullName;
            object result;
            if (_cache.TryGetValue(cacheKey, out result))
                return (T) result;

            SettingDef d;
            if (!_settingsDict.TryGetValue(key, out d))
                if (required)
                    throw new KeyNotFoundException("Setting '" + key + "' not found");
                else
                    return defaultValue;

            result = Convert<T>(d);
            _cache[cacheKey] = result;
            return (T) result;
        }

        public static T Convert<T>(SettingDef def)
        {
            if (typeof(T) == typeof(string))
            {
                return (T) System.Convert.ChangeType(def.Value, typeof(string));
            }

            var m = typeof(T).GetMethod("Parse", new Type[] {typeof(string), typeof(IFormatProvider)});
            if (m != null)
            {
                return (T) m.Invoke(null, new object[] {def.Value, CultureInfo.InvariantCulture});
            }

            m = typeof(T).GetMethod("Parse", new Type[] { typeof(string) });
            return (T) m.Invoke(null, new[] {def.Value});
        }
    }
}