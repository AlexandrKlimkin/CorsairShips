using System;
using System.Diagnostics;

namespace MessageServer.Sources.Tcp
{
    public class PingHandler : IMessageHandler
    {

        public TimeSpan Rtt { get; private set; }
        public bool Busy { get; private set; }
        public TimeSpan BusyTime {
            get { return _stopwatch.Elapsed; }
        }
        private int _tag;
        private Stopwatch _stopwatch = new Stopwatch();

        public void Register(int tag)
        {
            if(Busy)
                return;
            _tag = tag;
            Busy = true;
            _stopwatch.Restart();
        }

        public void Reset()
        {
            Busy = false;
            _stopwatch.Reset();
        }

        public void Handle(int sender, byte[] data, int tag, IAnswerContext answerContext)
        {
            if(_tag != tag)
                return;
            Rtt = _stopwatch.Elapsed;
            Reset();
        }

        public void Error(int tag)
        { }
    }
}