using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MessagePack;
using PestelLib.ServerShared;
using PestelLib.SharedLogicBase;
using UnityDI;

namespace PestelLib.SaveSystem.FileSystemStorage
{
    public class Storage : IStorage
    {
#pragma warning disable 649
        [Dependency] private ILog _log;
        [Dependency] private IPlayerPrefs _playerPrefs;
#pragma warning restore 649

        private readonly string _savePath;

        public Storage(string savePath)
        {
            ContainerHolder.Container.BuildUp(this);
            _savePath = savePath;
        }

        public static Storage MakeDefaultStorage()
        {
            ContainerHolder.Container.RegisterInstance<ILog>(new UnityLog());
            ContainerHolder.Container.RegisterInstance<IPlayerPrefs>(new UnityPlayerPrefs());
            return new Storage(UnityEngine.Application.persistentDataPath);
        }

        public void Dispose() { }

        public void BeginTransaction()
        {
            _log.Log("FileSystemStorage.Storage doesn't support transactions");
        }

        public void Commit()
        {
            _log.Log("FileSystemStorage.Storage doesn't support transactions");
        }

        public int RemoveCommandsRange(int serialFrom, int serialTo)
        {
            if (serialFrom != FirstCommandSerial)
            {
                _log.LogError("FileSystemStorage.Storage doesn't support removing not from the first command");
            }

            var serialCmdFound = false;
            int commandsLogPosition = 0;

            if (File.Exists(DiskLogPath))
            {
                try
                {
                    using (var reader = new BinaryReader(File.Open(DiskLogPath, FileMode.Open)))
                    {
                        while (reader.BaseStream.Position != reader.BaseStream.Length
                               && reader.BaseStream.Position < MaxBytesPerRequest)
                        {
                            var cmdBytesSize = reader.ReadInt32();
                            var serializedCommand = reader.ReadBytes(cmdBytesSize);
                            var commandCrc = reader.ReadUInt32();
                            var actualCrc = Crc32.Compute(serializedCommand);
                            if (commandCrc != actualCrc)
                            {
                                _log.ServerLogError("SaveSystem", "Broken command in log, wrong CRC " + commandCrc + " " + actualCrc);
                                throw new CommandLogException(CommandLogException.ExceptionType.WrongCRC);
                            }

                            var commandBatch =
                                LZ4MessagePackSerializer.Deserialize<CommandBatch>(serializedCommand);

                            if (commandBatch.commandsList == null) continue;

                            var cmd = commandBatch.commandsList[0];
                            commandsLogPosition += sizeof(Int32) + cmdBytesSize + sizeof(UInt32);
                            if (cmd.SerialNumber == serialTo)
                            {
                                serialCmdFound = true;
                                break;
                            }
                        }
                    }

                    if (serialCmdFound)
                    {
                        RemoveFirstNBytesFromFile(DiskLogPath, commandsLogPosition);
                    }
                    else
                    {
                        _log.LogError("Can't find command with serialTo = " + serialTo);
                    }
                }
                catch (CommandLogException e)
                {
                    if (e.Type == CommandLogException.ExceptionType.WrongCRC)
                    {
                        /*
                         * locally saved data corrupted - we can do nothing with it, but delete it
                         */
                        RemoveCachedFiles();

                        //RequestQueueEventProcessor.OnSharedLogicExceptionEvent("File commands.bin corrupted! Please restart the game.");
                        _log.ServerLogError("SaveSystem", "CommandProcessor: File commands.bin corrupted!Please restart the game.");
                    }
                }
                catch (Exception e)
                {
                    //RequestQueueEventProcessor.OnSharedLogicExceptionEvent("Can't restore serialized SL commands from disk. " + e.Message);
                    _log.ServerLogError("SaveSystem", "CommandProcessor: Can't restore serialized SL commands from disk. " + e.Message);
                }
            }

            return serialTo - serialFrom;
        }

        private void RemoveCachedFiles()
        {
            TryDeleteFile(DiskLogPath);
            TryDeleteFile(SavePath);
            TryDeleteFile(SavePathBak);
            TryDeleteFile(JsonSavePath);
        }

        public void AddCommand(Command cmd)
        {
            var batch = new CommandBatch();
            batch.commandsList.Add(new SharedLogicBase.Command
            {
                SerializedCommandData = cmd.SerializedCommandData,
                SerialNumber = cmd.SerialNumber,
                Timestamp = cmd.Timestamp
            });

            if (batch.commandsList.Count == 0) return;


            if (cmd.Hash.HasValue)
            {
                batch.controlHash = cmd.Hash.Value;
            }

            batch.DefinitionsVersion = cmd.DefinitionsVersion;
            batch.SharedLogicCrc = cmd.SharedLogicCrc;

            try
            {
                if (!File.Exists(DiskLogPath))
                {
                    using (var stream = File.Create(DiskLogPath))
                    {
                    }
                }

                using (var binWriter = new BinaryWriter(File.Open(DiskLogPath, FileMode.Append)))
                {
                    var serializedCompressedCommandBatch = LZ4MessagePackSerializer.Serialize(batch);
                    binWriter.Write(serializedCompressedCommandBatch.Length);
                    binWriter.Write(serializedCompressedCommandBatch);
                    binWriter.Write(Crc32.Compute(serializedCompressedCommandBatch));
                    binWriter.Flush();

                    var totalSize = sizeof(int) + serializedCompressedCommandBatch.Length + sizeof(uint);
                    _log.ServerLogInfo("SaveSystem", String.Format("CommandProcessor: Added {0} bytes to log", totalSize));
                }
            }
            catch (Exception e)
            {
                _log.ServerLogError("SaveSystem", String.Format("CommandProcessor: Can't append commands to binary log {0} exception: {1} {2}", DiskLogPath, e.Message,
                    e.StackTrace));
            }
        }

        public List<Command> GetCommands(int serialFrom, int serialTo)
        {
            var allCommands = ReadAllCommandsFromDisk();
            var range = allCommands.Where(x => serialFrom <= x.SerialNumber && x.SerialNumber <= serialTo);
            return range.ToList();
        }

        public bool IsHaveAnyCommands {
            get { return FileSize(DiskLogPath) > 0; }
        }

        public int FirstCommandSerial {
            get
            {
                var commands = ReadAllCommandsFromDisk();
                if (commands.Count > 0)
                {
                    return commands[0].SerialNumber;
                }

                return 0;
            }
        }

        public int LastCommandSerial {
            get
            {
                var commands = ReadAllCommandsFromDisk();
                if (commands.Count > 0)
                {
                    return commands[commands.Count - 1].SerialNumber;
                }
                return 0;
            }
        }

        public byte[] UserProfile {
            get
            {
                if (!File.Exists(SavePath)) return null;

                try
                {
                    var localStateBytes = File.ReadAllBytes(SavePath);
                    var loadedDataCrc = Crc32.Compute(localStateBytes);
                    var savedStateCrc = (uint)_playerPrefs.GetInt(SaveCrcKey, 0);

                    if (savedStateCrc != 0 && loadedDataCrc != savedStateCrc)
                    {
                        _log.ServerLogError("SaveSystem", "FileSystemStorage.Storage: file with UserProfile has wrong CRC, returning null!");
                        RemoveCachedFiles();
                    }

                    return localStateBytes;
                }
                catch (Exception)
                {
                    _log.ServerLogError("SaveSystem", "FileSystemStorage.Storage: can't read UserProfile bytes from file, returning null");
                    RemoveCachedFiles();
                    return null;
                }
            }
            set
            {
                SaveSerializedStateOnDisk(value);
            }
        }

        public bool IsStorageEmpty {
            get { return !File.Exists(SavePath); }
        }

        public void SetString(string key, string val)
        {
            _playerPrefs.SetString(key, val);
        }

        public string GetString(string key, string defaultValue = "")
        {
            return _playerPrefs.GetString(key, defaultValue);
        }

        public bool IsUserProfileCorrupted()
        {
            return false;
        }

        public bool IsStringCorrupted(string key)
        {
            return false;
        }

        private List<Command> ReadAllCommandsFromDisk()
        {
            var result = new List<Command>();
            if (File.Exists(DiskLogPath))
            {
                try
                {
                    using (var reader = new BinaryReader(File.Open(DiskLogPath, FileMode.Open)))
                    {
                        while (reader.BaseStream.Position != reader.BaseStream.Length
                               && reader.BaseStream.Position < MaxBytesPerRequest)
                        {
                            var cmdBytesSize = reader.ReadInt32();
                            var serializedCommand = reader.ReadBytes(cmdBytesSize);
                            var commandCrc = reader.ReadUInt32();
                            var actualCrc = Crc32.Compute(serializedCommand);
                            if (commandCrc != actualCrc)
                            {
                                _log.ServerLogError("SaveSystem", "Broken command in log, wrong CRC " + commandCrc + " " + actualCrc);
                                throw new CommandLogException(CommandLogException.ExceptionType.WrongCRC);
                            }

                            var commandBatch =
                                LZ4MessagePackSerializer.Deserialize<CommandBatch>(serializedCommand);
                            
                            if (commandBatch.commandsList == null) continue;

                            for (var i = 0; i < commandBatch.commandsList.Count; i++)
                            {
                                var cmd = commandBatch.commandsList[i];
                                var lastCmdInPacket = i == commandBatch.commandsList.Count - 1;

                                result.Add(new Command
                                {
                                    DefinitionsVersion = commandBatch.DefinitionsVersion,
                                    SharedLogicCrc = commandBatch.SharedLogicCrc,
                                    SerialNumber = cmd.SerialNumber,
                                    SerializedCommandData = cmd.SerializedCommandData,
                                    Hash = lastCmdInPacket ? commandBatch.controlHash : (int?) null,
                                    Timestamp = cmd.Timestamp
                                });
                            }
                        }
                    }
                }
                catch (CommandLogException e)
                {
                    if (e.Type == CommandLogException.ExceptionType.WrongCRC)
                    {
                        /*
                         * locally saved data corrupted - we can do nothing with it, but delete it
                         */
                        RemoveCachedFiles();

                        //RequestQueueEventProcessor.OnSharedLogicExceptionEvent("File commands.bin corrupted! Please restart the game.");
                        _log.ServerLogError("SaveSystem", "CommandProcessor: File commands.bin corrupted!Please restart the game.");
                    }
                }
                catch (Exception e)
                {
                    //RequestQueueEventProcessor.OnSharedLogicExceptionEvent("Can't restore serialized SL commands from disk. " + e.Message);
                    _log.ServerLogError("SaveSystem", "CommandProcessor: Can't restore serialized SL commands from disk. " + e.Message);
                }
            }

            return result;
        }

        private static void TryDeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public string SavePathBak
        {
            get { return SavePath + ".bak"; }
        }

        public string SavePath
        {
            get
            {
                return _savePath + "/saveRealtime.bin";
            }
        }

        public string JsonSavePath
        {
            get
            {
                return _savePath + "/saveRealtime.json";
            }
        }

        public string DiskLogPath
        {
            get { return _savePath + "/commands.bin"; }
        }

        public const int MaxBytesPerRequest = 10 * 1024;
        public const string SaveCrcKey = "SaveCRC";

        private void RemoveFirstNBytesFromFile(string path, long n)
        {
            var oldArray = File.ReadAllBytes(path);
            byte[] newArray = new byte[oldArray.Length - n];
            Buffer.BlockCopy(oldArray, (int)n, newArray, 0, newArray.Length);
            File.WriteAllBytes(path, newArray);
        }

        public void SaveSerializedStateOnDisk(byte[] data)
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    File.Copy(SavePath, SavePathBak, true);
                }

                File.WriteAllBytes(SavePath, data);
#if UNITY_IOS
                UnityEngine.iOS.Device.SetNoBackupFlag(SavePath);
#endif
            }
            catch (Exception e)
            {
                _log.LogWarning("Error writing save: " + e.Message);
            }

            var crc = Crc32.Compute(data);
            _playerPrefs.SetInt(SaveCrcKey, (int) crc);

#if UNITY_IOS
            UnityEngine.iOS.Device.SetNoBackupFlag(SavePath);
#endif
        }

        private long FileSize(string path)
        {
            try
            {
                if (!File.Exists(path)) return 0;
                return new FileInfo(path).Length;
            }
            catch (Exception e)
            {
                _log.ServerLogError("SaveSystem", string.Format("Can't get file size from " + path + " exception: " + e.Message + " " + e.StackTrace));
            }
            return 0;
        }
    }
}