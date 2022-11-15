using System.Collections.Concurrent;
using System.Collections.Generic;
using MessageServer.Sources.Tcp;

namespace MessageServer
{
    public class MessageServerStats : MessageStatistics
    {
        public long RequestQueueSize;
        public long ContextsListSize;
        public long IdMapSize;

        public Dictionary<int, long> MessageCountByType => new Dictionary<int, long>(_messageCountByType);

        public void NotifySent(int messageType)
        {
            _messageCountByType.AddOrUpdate(messageType, _ => 1, (k, v) => v + 1);
        }

        private ConcurrentDictionary<int , long> _messageCountByType = new ConcurrentDictionary<int, long>();
    }
}
