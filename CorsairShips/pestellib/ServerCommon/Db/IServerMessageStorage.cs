using System;
using S;

namespace PestelLib.ServerCommon.Db
{
    public interface IServerMessageStorage
    {
        bool SendMessage(ServerMessage message, Guid toUser);
        byte[] GetMessages(Guid toUser);
        void ClearInbox(Guid toUser);
        void Save(Guid user, ServerMessagesInbox inbox);
        bool IsEmpty();
    }
}
