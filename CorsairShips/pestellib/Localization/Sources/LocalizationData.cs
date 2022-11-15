using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityDI;
using PestelLib.SharedLogic.Defs;
using PestelLib.Utils.Sources;
using UnityEngine;

namespace PestelLib.Localization
{
    public class LocalizationData : ILocalization
    {

        public event Action OnChangeLocale = () => { };

        [Dependency] private List<LocalizationDef> _defs;

        private Dictionary<string, Dictionary<string, string>> _data = new Dictionary<string, Dictionary<string, string>>();

        private string _currentLocale;
        private const string DefaultLanguage = "en_US";

        public bool IsAsian { get; private set; }

        public virtual string CurrentLocale
        {
            get
            {
                return _currentLocale;
            }
            set
            {
                if (_currentLocale == value) return;

                _currentLocale = value;
                PlayerPrefs.SetString("CurrentLocale", value);
                PlayerPrefs.Save();
                IsAsian = value == "ja_JP" || value == "zh_TW" || value == "zh_CN" || value == "ko_KR";
                OnChangeLocale();
            }
        }

        public virtual List<string> Languages
        {
            get
            {
                if (_Languages == null)
                    _Languages = _data.First().Value.Keys.ToList();
                return _Languages;
            }
        }

        public SystemLanguage CurrentLanguage
        {
            get { return CultureInfoHelper.LocaleToSystemLanguage(CurrentLocale, SystemLanguage.English); }
        }

        private List<string> _Languages;

        public virtual List<string> Keys
        {
            get { return _data.Keys.ToList(); }
        }

        public LocalizationData()
        {
            ResolveDependencies();
            ProcessDefinitions();
            ChooseCurrentLocale();
        }

        protected virtual void ResolveDependencies()
        {
            ContainerHolder.Container.BuildUp(this);

            if (_defs == null)
            {
                _defs = new List<LocalizationDef>();
            }
        }

        protected virtual void ProcessDefinitions()
        {
            var list = _defs;
            var defType = typeof(LocalizationDef);
            var defFields =
                defType.GetFields()
                .Where(x => x.Name != "Id" && x.FieldType == typeof(string))
                .ToArray();

            for (var i = 0; i < list.Count; i++)
            {
                var val = list[i];
                var map = new Dictionary<string, string>();
                _data[val.Id] = map;

                for (var j = 0; j < defFields.Length; j++)
                {
                    var field = defFields[j];
                    var fieldValue = (string)field.GetValue(val);
                    _data[val.Id][field.Name] = fieldValue;
                }
            }
        }

        protected virtual void ChooseCurrentLocale()
        {
            var prefferedLang = PlayerPrefs.GetString("CurrentLocale", "IsNotSet");
            if (prefferedLang == "IsNotSet" || (prefferedLang != "IsNotSet" && !Languages.Contains(prefferedLang)))
            {
                var lang = Application.systemLanguage;
                if (lang == SystemLanguage.Russian && Languages.Contains("ru_RU"))
                {
                    CurrentLocale = "ru_RU";
                }
                else if (lang == SystemLanguage.French && Languages.Contains("fr_FR"))
                {
                    CurrentLocale = "fr_FR";
                }
                else if (lang == SystemLanguage.Indonesian && Languages.Contains("id_ID"))
                {
                    CurrentLocale = "id_ID";
                }
                else
                {
                    CurrentLocale = DefaultLanguage;
                }
            }
            else
            {
                CurrentLocale = prefferedLang;
            }
        }

        public virtual string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                return key;

            if (_data.ContainsKey(key) && _data[key].ContainsKey(CurrentLocale))
            {
                return _data[key][CurrentLocale];
            }
            else
            {
                return key;
            }
        }

        public virtual string Format(string key, params object[] args)
        {
            var localizedString = Get(key);
            if (localizedString == key) return key;

            return string.Format(localizedString, args);
        }
    }
}
