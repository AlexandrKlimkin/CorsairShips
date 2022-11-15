using System;
using UnityDI;
using UnityEngine;

namespace PestelLib.Replay
{
    public class ReplayableObject : InitableMonoBehaviour, IReplayableObject
    {
        //Hack to fix AOT compilation
        private event Action onComplete = () => { };
        public event Action OnComplete
        {
            add { onComplete = (Action) Delegate.Combine(onComplete, value); }
            remove { onComplete = (Action) Delegate.Remove(onComplete, value); }
        }

        [Dependency] private ReplayManager _replayManager;
        private Animator _animator;

        private float _duration = 1f;
        private bool _inProgress = false;
        private Vector3 _storedPosition;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _animator.StartRecording(1500);

            //Debug.Log("Start recording on frame" + Time.frameCount);
        }

        override protected void SafeStart()
        {
            _replayManager.Add(this);
        }

        private void OnDestroy()
        {
            _replayManager.Remove(this);
        }


        private float ClampedStartTime
        {
            get { return Mathf.Max(_animator.recorderStopTime - _duration, _animator.recorderStartTime); }
        }

        public void Replay(float duration)
        {
            _storedPosition = transform.position;
            _duration = duration;
            _animator.StopRecording();
            _animator.StartPlayback();

            //Debug.Log("_animator.recorderStartTime " + _animator.recorderStartTime +  " " + _animator.recorderStopTime);

            _animator.playbackTime = ClampedStartTime + Time.deltaTime * 4;

            _inProgress = true;
        }

        public void Skip() { }

        public override void SafeUpdate()
        {
            if (_animator.recorderMode == AnimatorRecorderMode.Playback)
            {
                if (!IsCompleted)
                {
                    _animator.playbackTime = CurrentTime;
                }
                else
                {
                    _inProgress = false;

                    _animator.playbackTime = _animator.recorderStopTime;
                    _animator.StopPlayback();
                    _animator.StartRecording(300);

                    transform.position = _storedPosition;

                    onComplete();
                }
            }
        }

        public bool IsCompleted
        {
            get
            {
                return !_inProgress || CurrentTime >= _animator.recorderStopTime;
            }
        }

        private float CurrentTime
        {
            get { return _animator.playbackTime + Time.unscaledDeltaTime * _replayManager.Speed; }
        }
    }
}
