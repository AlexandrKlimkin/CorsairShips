using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite4Unity3d;
using UnityEngine;
using PestelLib.ServerLogClient;
using PestelLib.ServerShared;
using UnityDI;
using PestelLib.Utils;
using Object = UnityEngine.Object;

namespace PestelLib.SaveSystem.SQLiteStorage
{
    public class Storage : IStorage
    {
        private static IStorage _storage = null;

        private readonly SQLiteConnection _connection;

        //path example: Application.persistentDataPath + "/database.sqlite"
        public Storage(string path)
        {
            _connection = new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
            _connection.CreateTable<Command>();
            _connection.CreateIndex("Command", "SerialNumber", true);

            _connection.CreateTable<UserProfile>();

            _connection.CreateTable<StringPair>();
            _connection.CreateIndex("StringPair", "Key", true);
        }

        public static IStorage MakeDefaultStorage()
        {
            return new Storage(Application.persistentDataPath + "/database.sqlite");
        }

        public static IStorage MakeDefaultStorage(string pathSuffix)
        {
            return new Storage(Application.persistentDataPath + string.Format("/database{0}.sqlite", pathSuffix));
        }

        public static void TryRegisterDefaultSqLiteStorage()
        {
            if (!ContainerHolder.Container.IsRegistred<IStorage>())
            {
                ContainerHolder.Container.RegisterCustom<IStorage>(() =>
                {
                    if (_storage == null)
                    {
                        if (Application.isEditor)
                        {
                            _storage = Storage.MakeDefaultStorage(Crc32.Compute(Application.dataPath).ToString());
                        }
                        else
                        {
                            _storage = Storage.MakeDefaultStorage();
                        }

                        var go = new GameObject("IStorage");
                        Object.DontDestroyOnLoad(go);
                        var handler = go.AddComponent<DisposableHandler>();
                        handler.Content = _storage;
                    }

                    return _storage;
                });
            }
        }

        public void BeginTransaction()
        {
            try
            {
                _connection.BeginTransaction();
            }
            catch (SQLiteException ex)
            {
                ServerLog.LogException("SQLiteStorage.BeginTransaction", ex.Message);
            }
        }

        public void Commit()
        {
            try
            {
                _connection.Commit();
            }
            catch (SQLiteException ex)
            {
                ServerLog.LogException("SQLiteStorage.Commit", ex.Message);
            }
        }

        public void AddCommand(SaveSystem.Command cmd)
        {
            try
            {
                var sqLiteCommand = (Command) cmd;
                _connection.Insert(sqLiteCommand);
            }
            catch (SQLiteException ex)
            {
                ServerLog.LogException("Storage", "AddCommand: " + ex.Message);
            }
        }

        public List<SaveSystem.Command> GetCommands(int serialFrom, int serialTo)
        {
            var result = _connection.Table<Command>().Where(x => serialFrom <= x.SerialNumber && x.SerialNumber <= serialTo);
            return result.ToList().ConvertAll(x => (SaveSystem.Command) x);
        }

        public bool IsHaveAnyCommands
        {
            get
            {
                var cmd = _connection.CreateCommand("select count(SerialNumber) from Command");
                return cmd.ExecuteScalar<int>() > 0;
            }
        }

        public int FirstCommandSerial {
            get
            {
                if (!IsHaveAnyCommands)
                {
                    //avoid null reference exception in ExecuteScalar
                    return 0;
                }

                var cmd = _connection.CreateCommand("select min(SerialNumber) from Command");
                return cmd.ExecuteScalar<int>();
            }
        }
        public int LastCommandSerial {
            get
            {
                if (!IsHaveAnyCommands)
                {
                    //avoid null reference exception in ExecuteScalar
                    return 0;
                }

                var cmd = _connection.CreateCommand("select max(SerialNumber) from Command");
                return cmd.ExecuteScalar<int>();
            }
        }

        public int RemoveCommandsRange(int serialFrom, int serialTo)
        {
            if (serialFrom == 0 && serialTo == 0)
            {
                //nothing to do
                return 0;
            }

            try
            {
                return DeleteRange<Command>(serialFrom, serialTo);
            }
            catch (SQLiteException ex)
            {
                ServerLog.LogException("Storage", "RemoveCommand: " + ex.Message);
            }
            return 0;
        }

        private int DeleteRange<T>(int from, int to)
        {
            var map = _connection.GetMapping(typeof(T));
            var pk = map.PK;
            if (pk == null)
            {
                throw SQLiteException.New(SQLite3.Result.Error, "DeleteRange: Cannot delete " + map.TableName + ": it has no PK");
            }
            
            var q = string.Format("delete from \"{0}\" where \"{1}\" >= ? and \"{1}\" <= ?", map.TableName, pk.Name);
            return _connection.Execute(q, from, to);
        }

        public bool IsUserProfileCorrupted()
        {
            try
            {
                var userState = _connection.Find<UserProfile>(1);
                if (userState?.Data == null || !MessageCoder.CheckSignature(userState.Data))
                {
                    return true;
                }
            }
            catch (SQLiteException exception)
            {
                ServerLog.LogException("Storage", "UserProfile.set" + exception.Message);
                return true;
            }
            return false;
        }

        public bool IsStringCorrupted(string key)
        {
            try
            {
                var result = _connection.Find<StringPair>(key);
                if (result?.Value == null || !MessageCoder.CheckStringSignature(result.Value, out var val))
                {
                    return true;
                }
            }
            catch (SQLiteException exception)
            {
                ServerLog.LogException("Storage", "GetString" + exception.Message);
                return true;
            }

            return false;
        }

        public byte[] UserProfile
        {
            get
            {
                try
                {
                    var userState = _connection.Find<UserProfile>(1);
                    if (userState?.Data != null)
                    {
                        if(MessageCoder.CheckSignature(userState.Data))
                            return MessageCoder.GetData(userState.Data);
                        //Debug.LogError("Profile corrupted.");
                    }
                }
                catch (SQLiteException exception)
                {
                    ServerLog.LogException("Storage", "UserProfile.set" + exception.Message);
                }

                return null;
            }
            set
            {
                if (value == null)
                    return;

                try
                {
                    var data = MessageCoder.AddSignature(value);
                    _connection.InsertOrReplace(new UserProfile
                    {
                        Id = 1,
                        Data = data
                    });
                }
                catch (SQLiteException exception)
                {
                    ServerLog.LogException("Storage", "UserProfile.set" + exception.Message);
                }
            }
        }

        public bool IsStorageEmpty {
            get { return UserProfile == null; }
        }

        public void SetString(string key, string val)
        {
            if (GetString(key) == val) return;

            try
            {
                _connection.InsertOrReplace(new StringPair
                {
                    Key = key,
                    Value = MessageCoder.SignString(val)
                });
            }
            catch (SQLiteException exception)
            {
                ServerLog.LogException("Storage", "SetString" + exception.Message);
            }
        }

        public string GetString(string key, string defaultValue = "")
        {
            try
            {
                var result = _connection.Find<StringPair>(key);
                if (result?.Value != null)
                {
                    if(MessageCoder.CheckStringSignature(result.Value, out var val))
                        return val;
                    //Debug.LogError("Player id bad signature.");
                }
            }
            catch (SQLiteException exception)
            {
                ServerLog.LogException("Storage", "GetString" + exception.Message);
            }

            return defaultValue;
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Dispose();
            }
        }
    }
}