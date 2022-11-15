using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SmartFormat;

namespace UpdateCommandHelper
{
    class GooglePageEntry
    {
        public string GooglePage;
        public string CsType;

        public override int GetHashCode()
        {
            return (GooglePage + CsType).GetHashCode();
        }

        private bool Equals(GooglePageEntry entry)
        {
            if (entry == null)
                return false;

            return GetHashCode() == entry.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GooglePageEntry);
        }
    }

    static class ModuleIntegrator
    {
        private static readonly Regex RgxModuleMatch = new Regex(".*class\\s+([\\w\\d_]+)\\s*:\\s*SharedLogicModule", RegexOptions.Multiline);
        private static readonly Regex RgxGooglePages = new Regex("\\[GooglePageRef\\(\"(?<page>[\\w\\d_]+)\"\\)\\]\\s*(\r\n)?\\s*\\[Dependency.*?\\].*?(\r\n.*?)?List\\<(?<type>[\\w\\d_]+)\\>");
        private static readonly Regex RgxGooglePagesDict = new Regex("\\[GooglePageRef\\(\"(?<page>[\\w\\d_]+)\"\\)\\]\\s*(\r\n)?\\s*\\[Dependency.*?\\]([\\s\\w](\r\n)?)*Dictionary\\<.*?, (?<type>[\\w\\d_]+)\\>");
        private static readonly Regex RgxClass = new Regex(".*class\\s+(?<className>[\\w\\d_]+)\\s*(:\\s+[\\w\\d_]+\\s*)?");
        private static readonly Regex RgxStringId = new Regex("(?<hasId>.*?public\\s+string\\sId[\\s;=].*\r\n)");
        private static readonly Regex RgxChildClass = new Regex(".*class\\s+(?<className>[\\w\\d_]+)(\\s*\\:\\s*(?<baseClass>[\\w\\d_]+)).*?\r\n");

        public static void Generate(Options options, string modulesPath)
        {
            var modules = Directory.GetFiles(modulesPath, "*.cs", SearchOption.AllDirectories);
            var googlePages = new HashSet<GooglePageEntry>();
            var moduleTypes = new StringBuilder();
            var classesWithId = new HashSet<string>();
            var lateDefLookup = new List<(string className, string baseClass)>();
            foreach (var module in modules)
            {
                var data = File.ReadAllText(module);
                data = FileHelper.RgxLineEndings.Replace(data, "\r\n");

                var m1 = RgxGooglePages.Matches(data).OfType<Match>();
                var m2 = RgxGooglePagesDict.Matches(data).OfType<Match>();
                var ms = m1.Union(m2);
                foreach (Match match in ms)
                {
                    if (!match.Success)
                        continue;

                    var googlePageEntry = new GooglePageEntry()
                    {
                        GooglePage = match.Groups["page"].Value,
                        CsType = match.Groups["type"].Value
                    };
                    googlePages.Add(googlePageEntry);
                }

                var m = RgxClass.Match(data);
                var mId = RgxStringId.Match(data);
                if (mId.Groups["hasId"].Success && m.Success)
                    classesWithId.Add(m.Groups["className"].Value);
                else
                {
                    m = RgxChildClass.Match(data);
                    if (m.Success)
                        lateDefLookup.Add(ValueTuple.Create(m.Groups["className"].Value, m.Groups["baseClass"].Value));
                }

                m = RgxModuleMatch.Match(data);
                if(!m.Success)
                    continue;

                var sharedLogicModule = m.Groups[1];
                var gen = Smart.Format(moduleTypeTemplate, new {sharedLogicModule});
                moduleTypes.Append(gen);
            }

            var before = 0;
            var afterCount = 0;
            do
            {
                before = lateDefLookup.Count;
                foreach (var tuple in lateDefLookup)
                {
                    if (!classesWithId.Contains(tuple.baseClass))
                        continue;
                    classesWithId.Add(tuple.className);
                }
                lateDefLookup.RemoveAll(t => classesWithId.Contains(t.className));
                afterCount = lateDefLookup.Count;
            } while (before != afterCount);

            var s = moduleTypes.ToString();
            //FileHelper.ReplaceRegion(options.SharedLogicCorePath, "AUTO_GENERATED_MODULE_TYPES", s);

            WriteDefinitions(options, googlePages, classesWithId);
        }

        static string GenerateName(string baseName, ISet<string> usedNames)
        {
            string result;
            var count = 0;
            do
            {
                result = baseName + (count++ > 0 ? count.ToString() : "");
            } while (usedNames.Contains(result));
            usedNames.Add(result);

            return result;
        }

        static void WriteDefinitions(Options options, IEnumerable<GooglePageEntry> pages, ISet<string> classesWithId)
        {
            var varNames = new HashSet<string>();
            var coreDefDi = new StringBuilder();
            var definitions = new StringBuilder();
            var dictInit = new StringBuilder();
            var defHashes = new StringBuilder();
            var cacheDefHashes = new StringBuilder();
            var defHashesEquality = new StringBuilder();

            foreach (var googlePageEntry in pages)
            {
                var listName = GenerateName(googlePageEntry.CsType + "s", varNames);
                var defHashName = listName + "Hash";
                var gen = Smart.Format(definitionTemplate, new { googlePageEntry.GooglePage, googlePageEntry.CsType, listName });
                definitions.Append(gen);

                gen = Smart.Format(definitionHashTemplate, new { defHashName });
                defHashes.Append(gen);

                gen = Smart.Format(hashEqualityTemplate, new {defHashName, listName});
                defHashesEquality.Append(gen);

                gen = Smart.Format(cacheDefinitionHashTemplate, new { defHashName, listName});
                cacheDefHashes.Append(gen);

                gen = Smart.Format(coreDefDiTemplate, new {varName = listName});
                coreDefDi.Append(gen);

                if (classesWithId.Contains(googlePageEntry.CsType))
                {
                    var dictName = GenerateName(googlePageEntry.CsType + "Dict", varNames);
                    gen = Smart.Format(definitionDictTemplate, new { googlePageEntry.GooglePage, googlePageEntry.CsType, dictName});
                    definitions.Append(gen);

                    gen = Smart.Format(coreDefDiTemplate, new {varName = dictName});
                    coreDefDi.Append(gen);

                    gen = Smart.Format(definitionDicsFillTemplate, new {dictName, listName});
                    dictInit.Append(gen);
                }
            }

            var coreDefDiStr = coreDefDi.ToString();
            var definitionsStr = definitions.ToString();
            var dictInitStr = dictInit.ToString();

            defHashes.Append(hashMethodsTemplate).Replace("{hashEqualityConditions}", defHashesEquality.ToString());

            FileHelper.ReplaceRegion(options.DefinitionsFilePath, "AUTO_GENERATED_DEFINITIONS", definitionsStr);
            FileHelper.ReplaceRegion(options.DefinitionsFilePath, "AUTO_GENERATED_DICT_INIT", dictInitStr);
            FileHelper.ReplaceRegion(options.DefinitionsFilePath, "AUTO_GENERATED_DEF_HASHES", defHashes.ToString());
            FileHelper.ReplaceRegion(options.DefinitionsFilePath, "AUTO_GENERATED_CACHE_DEF_HASHES", cacheDefHashes.ToString());

            //used reflection instead of this
            //FileHelper.ReplaceRegion(options.SharedLogicCorePath, "AUTO_GENERATED_DEFINITIONS_DI", coreDefDiStr);
        }

        private const string moduleTypeTemplate = "            typeof({sharedLogicModule}),\r\n";
        private const string coreDefDiTemplate = "            container.RegisterInstance(definitions.{varName});\r\n";
        private const string definitionTemplate = "        [GooglePageRef(\"{GooglePage}\")]\r\n" +
                                                  "        public List<{CsType}> {listName} = new List<{CsType}>();\r\n";
        private const string definitionDictTemplate = "        [GooglePageRef(\"{GooglePage}\")]\r\n" + 
                                                      "        public Dictionary<string, {CsType}> {dictName} = new Dictionary<string, {CsType}>();\r\n";
        private const string definitionDicsFillTemplate = "            {dictName}.Clear();\r\n" +
                                                          "            for (int i = 0; i < {listName}.Count; i++)\r\n" +
                                                          "            {{\r\n" +
                                                          "                var def = {listName}[i];\r\n" +
                                                          "                {dictName}.Add(def.Id, def);\r\n" +
                                                          "            }}\r\n";

        private const string definitionHashTemplate = "        private string {defHashName};\r\n";
        private const string cacheDefinitionHashTemplate = "            {defHashName} = GetHash({listName});\r\n";
        private const string hashMethodsTemplate = "\r\n" + 
                                                   "        private string GetHash<T>(List<T> data)\r\n" +
                                                   "        {\r\n" +
                                                   "            return ServerShared.Md5.MD5string(System.Text.Encoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(data)));\r\n" +
                                                   "        }\r\n" +
                                                   "\r\n" +
                                                   "        public bool IsDefsCorrect()\r\n" +
                                                   "        {\r\n" +
                                                   "{hashEqualityConditions}" +
                                                   "\r\n" +
                                                   "            return true;\r\n" +
                                                   "        }\r\n";
        private const string hashEqualityTemplate = "            if (!{defHashName}.Equals(GetHash({listName}))) return false;\r\n";
    }
}