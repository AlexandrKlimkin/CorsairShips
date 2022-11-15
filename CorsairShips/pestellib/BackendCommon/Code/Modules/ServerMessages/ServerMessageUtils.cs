using System;
using S;
using PestelLib.ServerCommon.Db;
using BackendCommon.Code;

namespace ServerLib.Modules.ServerMessages
{
    public class ServerMessageUtils
    { 
        public static bool SendMessage(ServerMessage message, Guid toUser)
        {
            return MessageStorage.SendMessage(message, toUser);
        }

        public static byte[] GetMessages(Guid toUser)
        {
            return MessageStorage.GetMessages(toUser);
        }

        public static void ClearInbox(Guid toUser)
        {
            MessageStorage.ClearInbox(toUser);
        }

        private static IServerMessageStorage MessageStorage
        {
            get
            {
                if (_messageStorage != null)
                    return _messageStorage;
                _messageStorage = MainHandlerBase.ServiceProvider.GetService(typeof(IServerMessageStorage)) as IServerMessageStorage;
                return _messageStorage;
            }
        }
        private static IServerMessageStorage _messageStorage;
    }
}