using System;

namespace PestelLib.Log.Sources
{
    public class LogData
    {
        private string _data;

        public LogData()
        {
            _data = string.Empty;
        }

        public void AddHeader(string header)
        {
            _data = header + Environment.NewLine + _data;
        }

        public void Write(string msg)
        {
            _data += msg;
        }

        public void WriteLine(string msg)
        {
            _data += msg + Environment.NewLine;
        }

        public string GetData()
        {
            return _data;
        }

        public void Clear()
        {
            _data = string.Empty;
        }

        public bool IsEmpty()
        {
            return _data.Length == 0;
        }
    }
}