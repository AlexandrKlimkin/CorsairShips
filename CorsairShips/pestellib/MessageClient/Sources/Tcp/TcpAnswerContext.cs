using System;
using System.Threading;
using log4net;
using MessageServer.Sources.Sockets;

namespace MessageServer.Sources.Tcp
{
    public class TcpAnswerContext : IAnswerContext
    {
        private readonly Func<SocketContext> _socketFunc;
        private static readonly ILog _log = LogManager.GetLogger(typeof(TcpAnswerContext));
        private SocketContext _socketContext;
        private readonly MessageStatistics _statistics;
        private Message _message;

        public TcpAnswerContext(SocketContext socketContext, int tag, MessageStatistics statistics)
        {
            _socketContext = socketContext;
            _statistics = statistics;
            _message = new Message();
            _message.Tag = tag;
            _message.Answer = true;
        }

        public TcpAnswerContext(Func<SocketContext> socketFunc, int tag, MessageStatistics statistics)
            :this(socketFunc(), tag, statistics)
        {
            _socketFunc = socketFunc;
        }

        public bool AnswerSent { get; private set; }

        public bool Answer(int type, byte[] data)
        {
            if (AnswerSent)
                return false;

            _message.Type = type;
            _message.Data = data;
            try
            {
                if (_socketFunc != null)
                    _socketContext = _socketFunc();
                var s = _socketContext.SendMessage(_message);
                AnswerSent = true;
                Interlocked.Add(ref _statistics.BytesSent, s);
                Interlocked.Increment(ref _statistics.AnswerCount);
                return s != 0;
            }
            catch (Exception e)
            {
                _log.Error("Can't send answer.", e);
            }
            return false;
        }
    }
}