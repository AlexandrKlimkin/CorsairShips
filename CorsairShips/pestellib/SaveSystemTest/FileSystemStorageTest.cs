using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PestelLib.SaveSystem;
using PestelLib.SaveSystem.FileSystemStorage;
using UnityDI;

namespace SaveSystemTest
{
    [TestClass]
    public class FileSystemStorageTest : SQLiteStorageTest
    {
        const string directory = "FileSystemStorage";

        [TestInitialize]
        public override void Initialize()
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }

            Directory.CreateDirectory(directory);

            ContainerHolder.Container.RegisterInstance<ILog>(new MockLog());
            ContainerHolder.Container.RegisterInstance<IPlayerPrefs>(new MockPrefs());
        }

        protected override IStorage MakeStorage()
        {
            return new Storage(directory);
        }

        private class MockPrefs : IPlayerPrefs
        {
            private Dictionary<string, string> strs = new Dictionary<string, string>();
            private Dictionary<string, int> ints = new Dictionary<string, int>();

            public void SetString(string key, string val)
            {
                strs[key] = val;
            }

            public string GetString(string key, string defaultValue = "")
            {
                return strs.ContainsKey(key) ? strs[key] : defaultValue;
            }

            public void SetInt(string key, int val)
            {
                ints[key] = val;
            }

            public int GetInt(string key, int defaultValue = 0)
            {
                return ints.ContainsKey(key) ? ints[key] : defaultValue;
            }
        }

        private class MockLog : ILog
        {
            public void LogWarning(object message) {}

            public void LogError(object message) {}

            public void Log(object message) {}

            public void ServerLogError(string tag, string message) {}

            public void ServerLogInfo(string tag, string message) {}
        }
    }
}