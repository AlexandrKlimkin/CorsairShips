using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PestelLib.SaveSystem;
using PestelLib.SaveSystem.SQLiteStorage;

namespace SaveSystemTest
{
    [TestClass]
    public class SQLiteStorageTest
    {
        private const string DatabaseFilename = "database.sqlite";
        

        [TestInitialize]
        public virtual void Initialize()
        {
            if (File.Exists(DatabaseFilename))
            {
                File.Delete(DatabaseFilename);
            }
        }

        protected virtual IStorage MakeStorage()
        {
            return new Storage(DatabaseFilename);
        }

        [TestMethod]
        public void IsEmptyOnStart()
        {
            using (var storage = MakeStorage())
            {
                Assert.IsTrue(storage.IsStorageEmpty);
            }
        }

        [TestMethod]
        public void AddUserProfile()
        {
            using (var storage = MakeStorage())
            {
                var testData = new byte[] {0, 1, 2};
                storage.UserProfile = testData;
                var data = storage.UserProfile;
                Assert.IsTrue(data.SequenceEqual(testData));
                Assert.IsFalse(storage.IsStorageEmpty);
            }
        }

        [TestMethod]
        public void TestCommands()
        {
            using (var storage = MakeStorage())
            {
                TestAddCommands(storage);
                TestRemoveCommands(storage);
            }
        }

        [TestMethod]
        public void TestPlayerPrefs()
        {
            using (var storage = MakeStorage())
            {
                Assert.IsTrue(storage.GetString("PlayerId") == "");
                Assert.IsTrue(storage.GetString("PlayerId", "hello") == "hello");

                var uid = Guid.NewGuid();
                storage.SetString("PlayerId", uid.ToString());
                Assert.IsTrue(storage.GetString("PlayerId") == uid.ToString());
            }
        }

        private void TestRemoveCommands(IStorage storage)
        {
            Assert.IsTrue(storage.LastCommandSerial - storage.FirstCommandSerial == 1);
            storage.RemoveCommandsRange(1, 1);
            Assert.IsTrue(storage.LastCommandSerial - storage.FirstCommandSerial == 0);
            Assert.IsTrue(storage.FirstCommandSerial == 2);
            Assert.IsTrue(storage.LastCommandSerial == 2);
        }

        private static void TestAddCommands(IStorage storage)
        {
            var testData1 = new byte[] {0, 1, 2};
            var testData2 = new byte[] {0, 1, 2};

            storage.AddCommand(new PestelLib.SaveSystem.Command
            {
                SerialNumber = 1,
                Hash = null,
                DefinitionsVersion = 123,
                SerializedCommandData = testData1,
                SharedLogicCrc = 456
            });

            Assert.IsTrue(storage.FirstCommandSerial == 1);
            Assert.IsTrue(storage.LastCommandSerial == 1);
            Assert.IsTrue(storage.GetCommands(1, 1).First().SerializedCommandData.SequenceEqual(testData1));

            storage.AddCommand(new PestelLib.SaveSystem.Command
            {
                SerialNumber = 2,
                Hash = 111,
                DefinitionsVersion = 123,
                SerializedCommandData = testData2,
                SharedLogicCrc = 456
            });

            Assert.IsTrue(storage.FirstCommandSerial == 1);
            Assert.IsTrue(storage.LastCommandSerial == 2);
            Assert.IsTrue(storage.GetCommands(2, 2).First().SerializedCommandData.SequenceEqual(testData2));

            Assert.IsTrue(storage.GetCommands(1, 2).Count == 2);

            Assert.IsTrue(storage.GetCommands(2, 2).First().DefinitionsVersion == 123);
            Assert.IsTrue(storage.GetCommands(2, 2).First().Hash == 111);
            Assert.IsTrue(storage.GetCommands(2, 2).First().SharedLogicCrc == 456);
        }
    }
}