using System.Collections.Generic;

namespace PestelLib.EditorSharedPreferences
{
    internal class EditorSharedConfigKeyValue
    {
        public string Id;
        public object Value;
    }

    internal class EditorSharedConfig
    {
        public List<EditorSharedConfigKeyValue> Prefs;

        internal bool IsContainsKey(string key)
        {
            for (int i = 0; i < Prefs.Count; i++)
            {
                if (Prefs[i].Id == key)
                    return true;
            }

            return false;
        }

        internal EditorSharedConfigKeyValue GetValue(string key)
        {
            for (int i = 0; i < Prefs.Count; i++)
            {
                if (Prefs[i].Id == key)
                    return Prefs[i];
            }

            return null;
        }

        internal void StoreValue(string key, object value)
        {
            var record = GetValue(key);
            if (record != null)
            {
                record.Value = value;
                return;
            }

            record = new EditorSharedConfigKeyValue()
            {
                Id = key,
                Value = value
            };

            Prefs.Add(record);
        }
    }
}