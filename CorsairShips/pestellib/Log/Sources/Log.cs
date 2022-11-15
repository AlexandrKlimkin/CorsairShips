using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PestelLib.Log.Sources
{
    public class Log
    {
        private string _logCategory;

        private LogData _logData;

        private string Separator
        {
            get { return "-----------|------------|------------"; }
        }

        public Log(string logCategory)
        {
            _logData = new LogData();

            _logCategory = logCategory;
        }

        public string GetPathToLogFolder()
        {
            return Application.persistentDataPath + Path.DirectorySeparatorChar
                   + "Logs" + Path.DirectorySeparatorChar
                   + _logCategory + Path.DirectorySeparatorChar;
        }

        public void Message(string msg)
        {
#if UNITY_EDITOR
            string result = String.Format("{0, -10} | {1, -10} | {2}", Time.frameCount, (int)Time.realtimeSinceStartup, msg);
            _logData.WriteLine(result);
#endif
        }

        public void Dump()
        {
            if (_logData.IsEmpty())
                return;

            string path = GetPathToLogFolder();
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Can't dump log: " + ex.Message);
                    return;
                }
            }

            path += _logCategory + "_" + DateTime.Now.ToString("yyMMdd_HHmmss") + ".txt";

            try
            {
                using (var sw = new StreamWriter(path))
                {
                    string header = String.Format("{0, -10} | {1, -10} | {2}", "Frame", "Time", "Message") + Environment.NewLine;
                    _logData.AddHeader(header + Separator);

                    sw.Write(_logData.GetData());

                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to write log info: " + ex.Message);
            }

#if UNITY_IPHONE
            UnityEngine.iOS.Device.SetNoBackupFlag(path);
#endif

            _logData.Clear();
        }
    }
}