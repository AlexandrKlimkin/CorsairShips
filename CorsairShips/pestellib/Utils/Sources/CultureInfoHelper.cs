using System;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace PestelLib.Utils.Sources
{
    public static class CultureInfoHelper
    {
        /// <summary>
        /// </summary>
        /// <param name="month">1-12</param>
        /// <param name="lang">Culture name or UnityEngine.SystemLanguage</param>
        /// <param name="abbreviated">if true when returnds short month name (eg. "jan" for January)</param>
        /// <returns>null if localized name not found</returns>
        public static string GetMonthName(int month, string lang, bool abbreviated)
        {
            CultureInfo cult;
            try
            {
                if(lang == SystemLanguage.ChineseTraditional.ToString())
                    cult = CultureInfo.GetCultureInfo("zh-Hant");
                else if(lang == SystemLanguage.ChineseSimplified.ToString())
                    cult = CultureInfo.GetCultureInfo("zh-Hans");
                else if (lang == "SerboCroatian")
                    cult = CultureInfo.GetCultureInfo("sr");
                else if (lang == "Hugarian")
                    cult = CultureInfo.GetCultureInfo("hu-HU");
                else
                    cult = CultureInfo.GetCultureInfo(lang);
            }
            catch
            {   
                cult = CultureInfo.GetCultures(CultureTypes.AllCultures).FirstOrDefault(_ => _.EnglishName == lang);
            }

            if (cult == null)
                return null;

            if (abbreviated)
                return cult.DateTimeFormat.GetAbbreviatedMonthName(month);
            return cult.DateTimeFormat.GetMonthName(month);
        }

        private static SystemLanguage FromCulture(CultureInfo cult)
        {
            SystemLanguage sysLang;
            var parts = cult.EnglishName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
                sysLang = (SystemLanguage)Enum.Parse(typeof(SystemLanguage), parts[0]);
            else
                sysLang = (SystemLanguage)Enum.Parse(typeof(SystemLanguage), cult.EnglishName);
            return sysLang;
        }

        public static SystemLanguage LocaleToSystemLanguage(string locale, SystemLanguage fallback = default(SystemLanguage))
        {
            locale = locale.Replace('_', '-');

            try
            {
                var cult = CultureInfo.GetCultureInfo(locale);
                return FromCulture(cult);
            }
            catch
            {
            }

            var cults = CultureInfo.GetCultures(CultureTypes.AllCultures);
            for (var i = 0; i < cults.Length; ++i)
            {
                if(!locale.Contains(cults[i].IetfLanguageTag))
                    continue;
                return FromCulture(cults[i]);
            }

            return fallback;
        }
    }
}
