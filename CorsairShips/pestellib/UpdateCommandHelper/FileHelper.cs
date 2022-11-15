using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UpdateCommandHelper
{
    static class FileHelper
    {
        public static readonly Regex RgxLineEndings = new Regex("(?<!\r)\n");

        public static void ReplaceBlockInFile(ref string content,string startTag, string endTag, string filePath, string newContent)
        {
            var begin = content.IndexOf(startTag, StringComparison.Ordinal);
            var end = content.IndexOf(endTag, StringComparison.Ordinal);
            if (begin == -1 || end == -1)
            {
                Console.WriteLine("Can't find begin/end mark in " + filePath + "");
                return;
            }
            content = content.Remove(begin, (end - begin) + endTag.Length);
            content = content.Insert(begin,
                string.Format("{0}{1}{2}", startTag, newContent, endTag));
            File.WriteAllText(filePath, content);
        }

        public static void ReplaceBlockInFile(string startTag, string endTag, string filePath, string newContent)
        {
            var file = File.ReadAllText(filePath);
            ReplaceBlockInFile(ref file, startTag, endTag, filePath, newContent);
        }

        public static void ReplaceRegion(string filePath, string regionName, string newContent)
        {
            var file = File.ReadAllText(filePath);
            if (file.IndexOf(regionName) == -1)
            {
                Console.WriteLine("Can't find region " + regionName + " in file " + filePath);
                return;
            }

            var beginIndex = file.IndexOf(regionName) + regionName.Length;
            var endIndex = file.IndexOf("#endregion", beginIndex);

            if (endIndex == -1)
            {
                Console.WriteLine("Can't find end of region " + regionName + " in file " + filePath);
                return;
            }

            file = file.Remove(beginIndex, endIndex - beginIndex);
            file = file.Insert(beginIndex, "\r\n" + newContent);
            File.WriteAllText(filePath, file);
            
            /*
            var anyLine = "(.*?\r\n)";
            var rgx = new Regex($"(?<start>(.*?\r\n)*?\\s*#region {regionName}\\s*\r\n)({anyLine}*?)\\s*(?<end>#endregion\\s*\r\n(.*?\r\n)*)");
            var file = File.ReadAllText(filePath);

            

            file = RgxLineEndings.Replace(file, "\r\n"); //convert \n to \r\n
            var m = rgx.Match(file);
            var beg = m.Groups["start"];
            var end = m.Groups["end"];

            File.WriteAllText(filePath, beg + newContent + end);
            */
        }
    }
}
