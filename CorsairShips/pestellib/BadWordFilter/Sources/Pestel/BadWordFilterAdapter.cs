using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Crosstales.BWF;
using Crosstales.BWF.Model;
using PestelLib.ChatClient;
using PestelLib.ChatCommon;
using UnityEngine;

namespace BadWordFilter
{
    public class BadWordFilterAdapter: MonoBehaviour, IBadWordFilter
    {
        private string[] _defaultSources = new[]
        {
            "_global",
            "_emoji",
            "iana",
            ""
        };

        protected Regex _rgxTags;

        private static Regex rgxSpace = new Regex("[\\W\\d\\s_№!@#$%^&*]");

        private string[] GetSources(SystemLanguage language)
        {
            _defaultSources[3] = language.ToString().ToLower();
            return _defaultSources;
        }

        protected virtual void Awake()
        {
            var obj = Resources.Load("BadWordFilter");
            Instantiate(obj, transform);

            Crosstales.BWF.Manager.BadWordManager.isReplaceLeetSpeak = false;
            _rgxTags = new Regex("<.*?>");
        }

        private FilterReport FilterInt(ref string message, SystemLanguage language)
        {
            var sources = GetSources(language);
            return FilterInt(ref message, sources);
        }

        private FilterReport FilterInt(ref string message, string[] sources)
        {
            var result = new FilterReport();
            var count = BWFManager.GetAll(message, ManagerMask.BadWord, sources).Count;
            if (count > 0)
                result[BanReason.BadWord] = count;
            count = BWFManager.GetAll(message, ManagerMask.Domain, sources).Count;
            if (count > 0)
                result[BanReason.Links] = count;
            count = _rgxTags.Matches(message).Count;
            if (count > 0)
            {
                result[BanReason.RichText] = count;
                message = _rgxTags.Replace(message, "");
            }

            message = BWFManager.ReplaceAll(message, sourceNames: sources);
            return result;
        }

        private void FilterIntSpaces(ref string message, SystemLanguage language, FilterReport result)
        {
            int extraMatches;
            do
            {
                extraMatches = 0;
                var matches = new List<string>();
                foreach (Match match in rgxSpace.Matches(message))
                {
                    matches.Add(match.Value);
                }
                var parts = rgxSpace.Split(message);

                for (var i = 0; i < parts.Length; ++i)
                {
                    for (var j = i + 1; j <= parts.Length; ++j)
                    {
                        var str = string.Join("", parts.Skip(i).Take(j - i).ToArray());
                        var r = FilterInt(ref str, language);
                        if (r.Count > 0)
                        {
                            extraMatches += r.Count;
                            foreach (var kv in r)
                            {
                                if (!result.ContainsKey(kv.Key))
                                    result[kv.Key] = kv.Value;
                                else
                                    result[kv.Key] += kv.Value;
                            }
                            var partsWithSpace = parts.Take(i).Select((p, idx) => p + (matches.Count > idx ? matches[idx] : "")).ToArray();
                            var before = string.Join("", partsWithSpace);
                            partsWithSpace = parts.Skip(i).Take(j - i).Select((p, idx) => p + (matches.Count > idx ? matches[idx] : "")).ToArray();
                            var badWord = string.Join("", partsWithSpace);
                            partsWithSpace = parts.Skip(j).Select((p, idx) => p + (matches.Count > idx ? matches[idx] : "")).ToArray();
                            var after = string.Join("", partsWithSpace);
                            message = before + string.Join("", Enumerable.Repeat("*", badWord.Length).ToArray()) + after;
                            break;
                        }
                    }
                    if (extraMatches > 0)
                        break;
                }
            } while (extraMatches > 0);
        }

        public FilterReport Filter(ref string message, SystemLanguage language)
        {
            var result = FilterInt(ref message, language);

            return result;
        }

        public FilterReport Filter(ref string message)
        {
            var result = FilterInt(ref message, null);
            return result;
        }
    }
}
