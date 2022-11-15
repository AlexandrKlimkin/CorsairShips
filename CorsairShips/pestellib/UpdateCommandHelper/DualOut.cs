using System;
using System.IO;
using System.Text;

namespace UpdateCommandHelper
{
    public static class DualOut
    {
        private const string OutputFileName = "UpdateCommandHelperOutput.txt";

        private static TextWriter _current;

        private class OutputWriter : TextWriter
        {
            public override Encoding Encoding
            {
                get
                {
                    return _current.Encoding;
                }
            }

            public override void WriteLine(string value)
            {
                _current.WriteLine(value);
                File.AppendAllLines(OutputFileName, new string[] { value });
            }
        }

        public static void Init()
        {
            _current = Console.Out;
            if (File.Exists(OutputFileName))
            {
                File.Delete(OutputFileName);
            }
            Console.SetOut(new OutputWriter());
        }
    }
}