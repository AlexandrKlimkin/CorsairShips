using System;
using MessageServer.Sources;

namespace MessageServer.Server
{
    class MessageHandlerContext
    {
        public IMessageHandler Handler;
        public int Type;
        public DateTime CreateTime = DateTime.UtcNow;
    }
}
