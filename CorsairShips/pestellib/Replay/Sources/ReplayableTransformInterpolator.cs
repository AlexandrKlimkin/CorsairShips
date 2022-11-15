using System;
using System.Linq;
using PestelLib.Utils;
using UnityDI;
using UnityEngine;

namespace PestelLib.Replay
{
    public class ReplayableTransformInterpolator : InitableMonoBehaviour, IReplayableObject
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

        public bool IsCompleted
        {
            get { return _playbackTime >= Time.time; }
        }

        RingBuffer<ReplayableTransform> _recording = new RingBuffer<ReplayableTransform>(1000); 

        private bool _playback;

        private float _playbackTime;
        private int _nextFrameIndex;

        override protected void SafeStart()
        {
            _replayManager.Add(this);
        }

        private void OnDestroy()
        {
            if (_replayManager != null)
                _replayManager.Remove(this);
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

                _playbackTime += Time.unscaledDeltaTime*_replayManager.Speed;

                for (var i = 0; i < _recording.Capacity && _recording[_nextFrameIndex].Time < _playbackTime; i++)
                {
                    _nextFrameIndex = (_nextFrameIndex + 1) % _recording.Count;
                }

                var currentFrame = InterpolatedTransform;
                transform.localPosition = currentFrame.Position;
                transform.localRotation = currentFrame.Rotation;
            }
            else
            {
                _recording.Add(new ReplayableTransform
                {
                    FrameNumber = Time.frameCount,
                    Time = Time.time,
                    Position = transform.localPosition,
                    Rotation = transform.localRotation
                });
            }
        }

        public void Replay(float duration)
        {
            _playback = true;
            _playbackTime = Time.time - duration;

            for (var i = 0; i < _recording.Count; i++)
            {
                if (_recording[i].Time > _playbackTime)
                {
                    _nextFrameIndex = i;
                    break;
                }
            }
        }

        private int PrevFrameIndex
        {
            get
            {
                if (_nextFrameIndex == 0)
                {
                    if (_recording.IsFull)
                    {
                        return _recording.Count - 1;
                    }
                    return 0;
                }
                return (_nextFrameIndex - 1);
            }
        }

        private ReplayableTransform InterpolatedTransform
        {
            get
            {
                var prevFrame = _recording[PrevFrameIndex];
                var nextFrame = _recording[_nextFrameIndex];
                var totalTime = nextFrame.Time - prevFrame.Time;
                var currentTime = _playbackTime - prevFrame.Time;
                var normalizedTime = currentTime/totalTime;

                return new ReplayableTransform
                {
                    Position = Vector3.Lerp(prevFrame.Position, nextFrame.Position, normalizedTime),
                    Rotation = Quaternion.Lerp(prevFrame.Rotation, nextFrame.Rotation, normalizedTime),
                    Time = normalizedTime,
                    FrameNumber =  nextFrame.FrameNumber
                };
            }
        }

        public void Skip() {}
    }
}
