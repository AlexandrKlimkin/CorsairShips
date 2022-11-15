using System;

namespace PestelLib.SharedLogic.Modules
{
    public class CachedSettingsValue<T>
    {
        public CachedSettingsValue(SettingsModuleBase settingsModule, string key, T defaultValue = default, bool required = false)
        {
            _value = new Lazy<T>(() =>
            {
                return settingsModule.GetValue(key, defaultValue, required);
            });
        }

        public T GetValue()
        {
            return _value.Value;
        }

        private Lazy<T> _value;
    }
}
