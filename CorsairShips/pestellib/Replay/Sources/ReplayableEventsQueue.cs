using System;
using System.Collections.Generic;
using System.Linq;
using UnityDI;
using UnityEngine;

namespace PestelLib.Replay
{
    public class ReplayableEventsQueue : InitableMonoBehaviour, IReplayableObject
    {
        [Dependency] 
        private ReplayManager _replayManager;

        //Hack to fix AOT compilation
        private event Action onComplete = () => { };
        public event Action OnComplete
        {
            add { onComplete = (Action)Delegate.Combine(onComplete, value); }
            remove { onComplete = (Action)Delegate.Remove(onComplete, value); }
        }

        private readonly List<ReplayableEvent> _events = new List<ReplayableEvent>();
        private List<ReplayableEvent> _eventsCloneForSkip = new List<ReplayableEvent>();

        private float _playbackTime;

        private bool _playback;

        public bool IsCompleted {
            get { return _playbackTime >= Time.time || !_playback; }
        }

        override protected void SafeStart()
        {
            _replayManager.Add(this);
        }

        private void OnDestroy()
        {
            _replayManager.Remove(this);
        }

        public ReplayableEvent ProcessEvent(Action callback, Action callOnReplayStart, Action skipCallback = null, Action AdditionalActionOnReplay = null)
        {
            var evt = new ReplayableEvent
            {
                Event = callback,
                CallOnReplayStart = callOnReplayStart,
                SkipEvent = skipCallback,
                ReplayAction = AdditionalActionOnReplay,
                FrameNumber = Time.frameCount,
                Time = Time.time
            };
            
            _events.Add(evt);

            callback();

            return evt;
        }

        public void RemoveEvent(ReplayableEvent evt)
        {
            _events.Remove(evt);
        }

        public override void SafeUpdate()
        {
            if (_playback)
            {
                if (IsCompleted)
                {
                    _playback = false;
                    onComplete();
                    return;
                }

                _playbackTime += Time.unscaledDeltaTime * _replayManager.Speed;

                var eventToExecute = _events.Where(evt => evt.Time < _playbackTime);
                foreach (var evt in eventToExecute)
                {
                    evt.Event();
                    if (evt.ReplayAction != null)
                    {
                        evt.ReplayAction();
                    }
                }

                _events.RemoveAll(evt => evt.Time < _playbackTime);
            }
        }

        public void Replay(float duration)
        {
            _playback = true;
            _playbackTime = Time.time - duration;
            
            _eventsCloneForSkip = _events.ToList();

            _events.RemoveAll(evt => evt.Time < _playbackTime);
            _events.ForEach(evt => evt.CallOnReplayStart());
        }

        public void Skip()
        {
            foreach (var replayableEvent in _eventsCloneForSkip)
            {
                if (replayableEvent.SkipEvent != null)
                {
                    replayableEvent.SkipEvent(); 
                }
            }
            _playback = false;
            onComplete();
        }
    }
}
