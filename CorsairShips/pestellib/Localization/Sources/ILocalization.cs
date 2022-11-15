using System.Collections.Generic;
using System;
using UnityEngine;

namespace PestelLib.Localization
{
    public interface ILocalization
    {
        event Action OnChangeLocale;
        string CurrentLocale { get; set; }
        SystemLanguage CurrentLanguage { get; }
        List<string> Languages { get; }
        string Get(string key);
        string Format(string key, params object[] args);
    }
}