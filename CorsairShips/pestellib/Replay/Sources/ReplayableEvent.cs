using System;

namespace PestelLib.Replay
{
    public class ReplayableEvent
    {
        public int FrameNumber;
        public float Time;
        public Action Event;
        public Action CallOnReplayStart;
        public Action SkipEvent;
        public Action ReplayAction;
    }
}
