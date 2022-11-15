namespace MessageServer.Sources.Tcp
{
    public class MessageStatistics
    {
        public long BytesReceived;
        public long BytesSent;
        public long ConnectionsTotal;
        public long ConnectionsCurrent;
        public long NotifyCount;
        public long RequestCount;
        public long AnswerCount;
        public long MessagesCount;
    }
}