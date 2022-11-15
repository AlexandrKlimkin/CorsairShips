using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PestelLib.ClientConfig;
using PestelLib.SaveSystem;
using PestelLib.ServerClientUtils;
using PestelLib.ServerLogClient;
using PestelLib.ServerShared;
using PestelLib.SharedLogicBase;
using PestelLib.UniversalSerializer;
using S;
using UnityDI;
using UnityEngine;
using UnityEngine.Assertions;
using Command = PestelLib.SharedLogicBase.Command;

namespace PestelLib.SharedLogicClient
{
    public class CommandProcessor : MonoBehaviour
    {
        public Action OnCommandProcessed = () => { };
        public Action OnSyncFinished = () => { };

        [Dependency] private SharedTime _sharedTime; //could be null
        [Dependency] private Config _config; //could be null
        [Dependency] private ISharedLogic _sharedLogic = null;
        [Dependency] private RequestQueue _requestQueue; //could be null
        [Dependency] private IStorage _storage;

        private CommandBatch RAMBuffer = new CommandBatch();

        private float _lastServerSyncTimestamp = 0;

        private bool _commandInProgress = false;

        private bool _syncAllowed = true;
        public bool BlockedUntilGameRestart;

        private RequestQueueItem _syncRequest;
        
        private IEnumerator Start()
        {
            StorageInitializer.TryInitStorage();
            /*
             * CommandProcessor может быть создан раньше ШЛ для
             * управления логом команд, но он не должен начинать чего-либо
             * отсылать до того момента, как ШЛ будет создана
             */
            while (!ContainerHolder.Container.IsRegistred<ISharedLogic>())
            {
                yield return null;
            }

            Init();
        }

        private void Init()
        {
            ContainerHolder.Container.BuildUp(this);

            if (_sharedTime == null) Debug.Log("CommandProcessor: shared time is null - user can cheat with time");
            if (_config == null)
            {
                Debug.Log("CommandProcessor: _config is null, using default");
                _config = new Config() { SharedConfig = new SharedConfig() };
            }

            if (_requestQueue == null) Debug.Log("CommandProcessor: _requestQueue is null, user progress won't be sent to server");

            if (_requestQueue != null)
            {
                _requestQueue.OnFailed += OnFailedRequest;
            }
        }

        private void OnDestroy()
        {
            if (_requestQueue != null)
            {
                _requestQueue.OnFailed -= OnFailedRequest;
            }
        }

        private void OnFailedRequest(Response resp, Exception exception)
        {
            /*
             * При хэш мисматче нам придётся удалить локальный стейт и команды.
             * В противном случае на следущем запуске игры пошлются те же самые команды,
             * которые опять вызовут тот же самый хэш мисматч.
             * Оборотная сторона этого решения: мы теряем часть прогресса пользователя.
             * Расчет на то, что в большинстве случаев откатывать назад будет читеров
             */
            if (resp.ResponseCode == ResponseCode.HASH_CHECK_FAILED || 
                resp.ResponseCode == ResponseCode.WRONG_SESSION || 
                resp.ResponseCode == ResponseCode.LOST_COMMANDS)
            {
                _storage.UserProfile = resp.ActualUserProfile;
                WipeCommandsLogAndRequestsInQueue();
                BlockedUntilGameRestart = true;
            }
        }

        private long CommandTimestamp
        {
            get
            {
                if (_sharedTime != null) return _sharedTime.Now.Ticks;

                return DateTime.UtcNow.Ticks;
            }
        }

        public T ExecuteCommand<T>(byte[] commandData, Action onProcessingFinished = null)
        {
            if (BlockedUntilGameRestart) return default(T);

            if (_requestQueue != null && _requestQueue.IsFrozen)
            {
                var msg = "CRITICAL EXCEPTION IN SHARED LOGIC: request queue is frozen";
                OnCriticalError(msg);
                return default(T);
            }

            if (_sharedLogic == null)
                Init();

            if (_commandInProgress)
            {
                throw new Exception("You can't call Shared Logic command during execution of another command");
            }

            _commandInProgress = true;

            var baseCommand = new Command
            {
                Timestamp = CommandTimestamp,
                SerialNumber = _sharedLogic.CommandSerialNumber + 1,
                SerializedCommandData = commandData
                //AutoCommands = new AutoCommands()
            };

            //SetupCommandData(concreteCommand, baseCommand);
            //SetupCommandData(concreteCommand, baseCommand.AutoCommands);

            var result = default(T);

            try
            {
                result = ProcessCommand<T>(baseCommand);
            }
            catch (AssertionException e)
            {
#if UNITY_EDITOR
                var userMessageField = e.GetType().GetField("m_UserMessage", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var userMessage = userMessageField.GetValue(e).ToString();

                if (userMessage != "null")
                {
                    UnityEditor.EditorUtility.DisplayDialog(userMessage, e.Message + "\n" + e.StackTrace,
                        "OK");
                }
                else
                {
                    UnityEditor.EditorUtility.DisplayDialog("Shared logic assert", e.Message + "\n" + e.StackTrace,
                        "OK");
                }
#endif
                var msg = "CRITICAL EXCEPTION IN SHARED LOGIC: " + e.InnerException + "\n" + e.Message + "\n" +
                          e.StackTrace;
                OnCriticalError(msg);
                throw new Exception(_sharedLogic.PlayerId.ToString(), e);
            }
            catch (Exception e)
            {
                var msg = "CRITICAL EXCEPTION IN SHARED LOGIC: " + e.InnerException + "\n" + e.Message + "\n" +
                          e.StackTrace;
                OnCriticalError(msg);
                throw new Exception(_sharedLogic.PlayerId.ToString(), e);
            }

            _commandInProgress = false;

            if (_sharedLogic.OnExecuteScheduled != null)
            {
                _sharedLogic.OnExecuteScheduled();
            }
            _sharedLogic.ExecuteOrderedScheduledActions();            

            if (onProcessingFinished != null)
            {
                onProcessingFinished();
            }

            if (_config != null && _config.DontPackMultipleCommands)
            {
                LateUpdate();
            }

            return result;
        }

        private T ProcessCommand<T>(Command command)
        {
            RAMBuffer.commandsList.Add(command);
            RAMBuffer.SharedLogicCrc = _config.SharedConfig.SharedLogicCrc;
            RAMBuffer.DefinitionsVersion = _config.SharedConfig.DefinitionsVersion;

            T result = _sharedLogic.Process<T>(command);

            OnCommandProcessed();

            return result;
        }

        private string LogPath
        {
            get
            {
                string path = Application.persistentDataPath + "/commands.txt";

#if UNITY_IPHONE
                UnityEngine.iOS.Device.SetNoBackupFlag(path);
#endif
                return path;
            }
        }

        private float TimeSinceLastSync
        {
            get { return Time.realtimeSinceStartup - _lastServerSyncTimestamp; }
        }

        private bool IsTimeToSync
        {
            get { return TimeSinceLastSync > _config.CommandsSyncTimeout; }
        }

        //set to FALSE during battle to avoid commands serialization
        public bool SyncAllowed
        {
            get { return _syncAllowed; }
            set
            {
                _syncAllowed = value;
                if (_syncAllowed)
                {
                    LateUpdate();
                }
            }
        }

        private void LateUpdate()
        {
            if (BlockedUntilGameRestart) return;

            if (_sharedLogic == null) return;

            if (!SyncAllowed) return;

            if (!IsTimeToSync) return;

            if (HaveDataInRAM || _storage.IsHaveAnyCommands)
            {
                _lastServerSyncTimestamp = Time.realtimeSinceStartup;

                if (DebugModeDisabled)
                {
                    if (_config.EnableOfflineSharedLogic && RAMBuffer.commandsList.Count > 0)
                    {
                        try
                        {
                            /*
                             * Нам нужно согласованно менять лог комманд и профиль игрока, поэтому операция добавления команд в лог
                             * объединена в одну транзакцию вместе с обновлением сохранённого профиля игрока.
                             * Несогласованные изменения, когда команды например успевают обновиться, а профиль нет - приводят к разным логическим ошибкам,
                             * которые обычно влекут за собой в конечном итоге HASH_MISMATCH
                             */
                            _storage.BeginTransaction();

                            //сохраняем команды на диск - т.к. не известно, может быть именно запрос с последними командами не сможет дойти
                            //и игрок выйдет из игры раньше времени
                            for (var i = 0; i < RAMBuffer.commandsList.Count; i++)
                            {
                                var command = RAMBuffer.commandsList[i];
                                var isLastCommand = i == (RAMBuffer.commandsList.Count - 1);
                                _storage.AddCommand(new SaveSystem.Command
                                {
                                    SerializedCommandData = command.SerializedCommandData,
                                    SerialNumber = command.SerialNumber,
                                    Timestamp = command.Timestamp,
                                    DefinitionsVersion = _config.SharedConfig.DefinitionsVersion,
                                    SharedLogicCrc = _config.SharedConfig.SharedLogicCrc,
                                    Hash = isLastCommand ? _sharedLogic.GetStateControlHash() : (int?) null
                                });
                            }

                            RAMBuffer.commandsList.Clear();

                            _storage.UserProfile = _sharedLogic.SerializedState;

                            _storage.Commit();
                        }
                        catch (Exception e)
                        {
                            /*
                             * Мы не должны сюда попадать, но если уж попадём, хочется явно заблокировать клиент, что бы не вышло
                             * что игрок, у которого не сохранились команды, продолжает играть, а при след входе в игру у него откатится
                             * прогресс
                             */
                            var message = "Something went really wrong during operating with storage: " + e.Message + " " + e.StackTrace;
                            OnCriticalError(message);
                            return;
                        }

                        OnSyncFinished();

                        ServerLog.LogInfo("SaveSystem", string.Format("Commands written to disk: {0}", string.Join(", ", RAMBuffer.commandsList.Select(x => x.SerialNumber.ToString()).ToArray())));
                    }

                    /*
                    * нет смысла класть в очередь больше одного запроса
                    */
                    if (NoRequestInProgress)
                    {
                        if (_config.EnableOfflineSharedLogic)
                        {
                            //считываем и новые команды, и те которые сохранились с прошлых вызовов
                            var commands = _storage.GetCommands(_storage.FirstCommandSerial, _storage.LastCommandSerial);

                            var batch = new CommandBatch();
                            batch.commandsList = new List<Command>();
                            for (var i = 0; i < commands.Count; i++)
                            {
                                var command = commands[i];


                                SaveSystem.Command prevCommand;
                                if (i > 0)
                                {
                                    prevCommand = commands[i - 1];
                                    if (prevCommand.SharedLogicCrc != command.SharedLogicCrc ||
                                        prevCommand.DefinitionsVersion != command.DefinitionsVersion)
                                    {
                                        /*
                                         * команды до команды с индексом i были записаны в одной версии ШЛ, а после - с другой.
                                         * Поэтому их необходимо отправлять в разных запросах
                                         */
                                        if (!prevCommand.Hash.HasValue)
                                        {
                                            OnCriticalError("Command version changed, but I can't find command hash in prev command: #" + i);
                                            return;
                                        }

                                        batch.controlHash = prevCommand.Hash.Value;
                                        break;
                                    }
                                }

                                batch.SharedLogicCrc = command.SharedLogicCrc;
                                batch.DefinitionsVersion = command.DefinitionsVersion;
                                batch.IsEditor = Application.isEditor;

                                var isLastCommand = i == commands.Count - 1;
                                if (isLastCommand)
                                {
                                    if (!command.Hash.HasValue)
                                    {
                                        OnCriticalError("Can't find command hash in last command: #" + i);
                                        return;
                                    }
                                    batch.controlHash = command.Hash.Value;
                                }

                                batch.commandsList.Add(new Command
                                {
                                    SerializedCommandData = command.SerializedCommandData,
                                    SerialNumber = command.SerialNumber,
                                    Timestamp = command.Timestamp
                                });
                            }

                            ServerLog.LogInfo("SaveSystem", string.Format("Commands read from disk: {0}", string.Join(", ", batch.commandsList.Select(x => x.SerialNumber.ToString()).ToArray())));

                            SendCommandsToServer(batch, batch.controlHash) ;
                        }
                        else
                        {
                            SendCommandsToServer(RAMBuffer, _sharedLogic.GetStateControlHash());

                            if (RAMBuffer.commandsList.Count > 0)
                            {
                                _storage.UserProfile = _sharedLogic.SerializedState;
                                OnSyncFinished();
                            }

                            RAMBuffer.commandsList.Clear();
                        }
                    }
                }
                else
                {
                    _storage.UserProfile = _sharedLogic.SerializedState;
                    RAMBuffer.commandsList.Clear();
                    OnSyncFinished();
                }
            }
        }

        private void OnCriticalError(string message)
        {
            RequestQueueEventProcessor.OnSharedLogicExceptionEvent(message);
            ServerLog.LogException("SaveSystem", "CommandProcessor: " + message);
        }

        private bool NoRequestInProgress
        {
            get { return _syncRequest == null || _syncRequest.Finished; }
        }

        private bool DebugModeDisabled
        {
            get { return _config != null && !_config.UseLocalState; }
        }

        private bool HaveDataInRAM
        {
            get { return RAMBuffer.commandsList.Count >= _config.CommandsBatchSize; }
        }

        private void SendCommandsToServer(CommandBatch batch, int hash)
        {
            if (batch.commandsList.Count == 0)
            {
                ServerLog.LogInfo("SaveSystem", "CommandProcessor: I don't have anything to send. Have you got some exceptions in read/write commands log?");
                return;
            }

            byte[] requestData = Serializer.Serialize(batch);

            var request = new Request
            {
                ProcessCommandsBatchRequest = new ProcessCommandsBatchRequest
                {
                    hashCode = hash,
                    CommandCount = batch.commandsList.Count,
                    IsEditor = Application.isEditor || Debug.isDebugBuild,
                    Integrity = APKIntegrityChecker.Check()
                }
            };

            if (_requestQueue != null)
            {
                _syncRequest = _requestQueue.SendRequest("ProcessCommandsBatchRequest",
                    request,
                    (response, container) =>
                    {
                        if (_config.EnableOfflineSharedLogic)
                        {
                            if (response.ResponseCode == ResponseCode.OK)
                            {
                                try
                                {
                                    ServerLog.LogInfo("SaveSystem", string.Format("Commands sent to server: {0}", string.Join(", ", batch.commandsList.Select(x => x.SerialNumber.ToString()).ToArray())));
                                    //удаляем отправленные команды из лога
                                    _storage.RemoveCommandsRange(
                                        batch.commandsList.First().SerialNumber,
                                        batch.commandsList.Last().SerialNumber
                                    );
                                }
                                catch (Exception e)
                                {
                                    ServerLog.LogError("SaveSystem", string.Format("Can't remove log bytes exception {0} {1}",
                                        e.Message,
                                        e.StackTrace));
                                }
                            }
                        }
                    },
                    requestData,
                    _sharedLogic.SerializedState
                );
            }
        }

        public void WipeCommandsLogAndRequestsInQueue()
        {
            ServerLog.LogInfo("SaveSystem", "CommandProcessor: WipeCommandsLogAndRequestsInQueue()");

            _storage.RemoveCommandsRange(_storage.FirstCommandSerial, _storage.LastCommandSerial);

            if (_syncRequest != null)
            {
                _requestQueue.CancelRequest(_syncRequest);
                _syncRequest = null;
            }
        }
        
        public static void ApplyCommandsFromLogToState(ISharedLogic tmpSharedLogic)
        {
            var storage = ContainerHolder.Container.Resolve<IStorage>();
            var commands = storage.GetCommands(storage.FirstCommandSerial, storage.LastCommandSerial);
            var slCommands = new List<Command>();
            foreach (var command in commands)
            {
                slCommands.Add(new Command()
                {
                    SerializedCommandData = command.SerializedCommandData,
                    SerialNumber = command.SerialNumber,
                    Timestamp = command.Timestamp
                });
            }
            tmpSharedLogic.ApplyCommands(slCommands);
        }

        public static int FirstCommandSerialNumber()
        {
            var storage = ContainerHolder.Container.Resolve<IStorage>();
            return storage.FirstCommandSerial;
        }

        public static int LastCommandSerialNumber()
        {
            var storage = ContainerHolder.Container.Resolve<IStorage>();
            return storage.LastCommandSerial;
        }

        private static CommandProcessor _instance;

        //мы не можем в библиотеках использовать сейчас генерированные классы, такие как SharedLogicCommand.
        //поэтому вызовы получаются более многословными через этот метод 
        public static TResult Process<TResult, TCmd>(TCmd command)
        {
            if (_instance == null)
            {
                _instance = ContainerHolder.Container.Resolve<CommandProcessor>();
            }

            var cmd = new CommandContainer
            {
                CommandId = CommandCode.CodeByName(typeof(TCmd).Name),
                CommandData = Serializer.Serialize(command)
            };
            var cmdBytes = Serializer.Serialize(cmd);
            return _instance.ExecuteCommand<TResult>(cmdBytes);
        }
    }
}

