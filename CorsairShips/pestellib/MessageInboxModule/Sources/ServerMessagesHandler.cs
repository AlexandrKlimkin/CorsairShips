using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MessagePack;
using Newtonsoft.Json;
using PestelLib.ClientConfig;
using PestelLib.Localization;
using PestelLib.ServerClientUtils;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicBase;
using PestelLib.SharedLogicClient;
using S;
using ServerShared.Messaging;
using UnityDI;
using UnityEngine;

namespace MessageInboxModuleSources
{
    public enum ServerMessagesHandlerState
    {
        Idle,
        PullingMessages,
        ParsingMessages,
    };

    public class ServerMessagesHandler : MonoBehaviour
    {
        [Dependency] private RequestQueue _requestQueue;
        [Dependency] private Config _config;
        [Dependency] private ISharedLogic _sharedLogic;
        [Dependency] private ILocalization _localization;
        private MessageInboxModule _messageInboxModule;
        public bool Muted;
        public ServerMessagesHandlerState State { get; private set; }
        public event Action<string, byte[]> OnMessage = (type, data) => { };
        public HashSet<LocalizedMessage> LocalizedMessages { get; private set; }
        public int Version { get; private set; }
        public bool _checkMessages;
        private long _lastSeenSerialId;

        private IEnumerator Start()
        {
            ContainerHolder.Container.BuildUp(this);

            LocalizedMessages = new HashSet<LocalizedMessage>();
            _messageInboxModule = _sharedLogic.GetModule<MessageInboxModule>();
            _lastSeenSerialId = (long)CommandProcessor.Process<object, MessageInboxModule_GetEarliestMessage>(new MessageInboxModule_GetEarliestMessage()) - 1;
            _messageInboxModule.OnMessagesDeleted.Subscribe(MessagesDeleted);

            if (_config.UseLocalState)
            {
                yield break;
            }

            while (true)
            {
                if (!Muted)
                {
                    State = ServerMessagesHandlerState.PullingMessages;
                    CheckMessagesNow();
                }
                yield return new WaitForSeconds(30);
            }
        }

        void Update()
        {
            if (_checkMessages)
            {
                _checkMessages = false;
                _requestQueue.SendRequest("GetServerInbox", new Request()
                    {
                        GetServerMessagesInbox = new GetServerMessagesInbox()
                        {
                            SystemLanguage = _localization.CurrentLanguage.ToString(),
                            GetBroadcasts = true,
                            LastSeenBroadcastSerialId = _lastSeenSerialId,
                            Sequence = Version,
                            StateBirthday = _messageInboxModule.StateBirthday
                        }
                    },
                    (response, collection) =>
                    {
                        State = ServerMessagesHandlerState.ParsingMessages;
                        OnInboxReceived(response, collection);
                    });
            }
        }

        void OnDestroy()
        {
            _messageInboxModule.OnMessagesDeleted.Unsubscribe(MessagesDeleted);
        }

        private void MessagesDeleted(long[] longs)
        {
            LocalizedMessages.RemoveWhere(_ => longs.Contains(_.Id));
        }

        public void CheckMessagesNow()
        {
            _checkMessages = true;
        }

        private void OnInboxReceived(Response response, DataCollection dataCollection)
        {
            if (dataCollection.Data == null || dataCollection.Data.Length == 0)
            {
                // messages deleted on the server, wipe from state
                if(Version == 0)
                    CommandProcessor.Process<object, MessageInboxModule_ReplaceMessages>(
                        new MessageInboxModule_ReplaceMessages()
                        {
                            serialIds = new long[] {}
                        });
                return;
            }

            var newLocalizedMessages = new List<LocalizedMessage>();
            LocalizedMessage welcomeLetter = null;
            try
            {
                var messages = MessagePackSerializer.Deserialize<ServerMessagesInbox>(dataCollection.Data);

                foreach (var message in messages.Messages)
                {
                    if (message.MessageType == typeof(LocalizedMessage).Name)
                    {
                        var locMessage = LocalizedMessageHelper.FromServerMessage(message);
                        if (locMessage.WelcomeMessage)
                        {
                            welcomeLetter = locMessage;
                        }

                        if (locMessage.Id > _lastSeenSerialId)
                            _lastSeenSerialId = locMessage.Id + 1; // cache messages for current session
                        newLocalizedMessages.Add(locMessage);
                    }

                    try
                    {
                        OnMessage(message.MessageType, message.Data);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("OnMessage failed." + e);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error with server inbox messages: " + e.Message);
            }
            finally
            {
                State = ServerMessagesHandlerState.Idle;
                if (Version == 0 || newLocalizedMessages.Count > 0)
                {
                    ++Version;
                    LocalizedMessages.UnionWith(newLocalizedMessages);
                    CommandProcessor.Process<object, MessageInboxModule_SetWelcomeLetter>(
                        new MessageInboxModule_SetWelcomeLetter()
                        {
                            serialId = welcomeLetter?.Id ?? 0
                        });
                    CommandProcessor.Process<object, MessageInboxModule_ReplaceMessages>(
                        new MessageInboxModule_ReplaceMessages()
                        {
                            serialIds = LocalizedMessages.Select(_ => _.Id).ToArray()
                        });
                }
            }
        }
    }
}
