using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PestelLib.Utils;
using UnityEngine;

namespace PestelLib.Replay
{
    public class ReplayManager : MonoBehaviour
    {
        public Action OnReplayBegin = () => { };
        public Action OnReplayEnd = () => { }; 

        private float _speed = 1f;

        [SerializeField]
        private float _duration = 1f;

        [SerializeField] 
        private AnimationCurve _speedCurve;

        private float _replayBeginTimestamp;

        private float _delay;

        public bool IsPlaying { get; private set; }
        
        private HashSet<IReplayableObject> _objects = new HashSet<IReplayableObject>();

        private bool _skip;

        public void Add(IReplayableObject obj)
        {
            _objects.Add(obj);
            obj.OnComplete += OnReplayComplete;
        }

        public void Remove(IReplayableObject obj)
        {
            _objects.Remove(obj);
            obj.OnComplete -= OnReplayComplete;
        }

        private void Update()
        {
            if (IsPlaying)
            {
                _speed = _speedCurve.Evaluate(TimeFromBeginReplayNormalized);
            }
        }

        private void OnReplayComplete()
        {
            if (IsPlaying && _objects.All(x => x.IsCompleted))
            {
                TimeSwitcher.SetTimescale(TimeSwitcher.TimeType.Game, 1);
                IsPlaying = false;

                OnReplayEnd();
            }
        }

        public float Speed
        {
            get { return _skip ? float.MaxValue : _speed; }
        }

        [ContextMenu("Replay")]
        public void Replay()
        {
            _skip = false;

            TimeSwitcher.SetTimescale(TimeSwitcher.TimeType.Game, 0);

            _replayBeginTimestamp = Time.unscaledTime;

            IsPlaying = true;

            foreach (var replayableObject in _objects)
            {
                replayableObject.Replay(_duration + _delay);
            }

            OnReplayBegin();
        }

        public void StopReplay()
        {
            foreach (IReplayableObject replayableObject in _objects)
            {
                replayableObject.Skip();
            }
            _skip = true;
        }

        public void DelayedReplay(float delay)
        {
            _delay = delay;
            StartCoroutine(DelayedReplayCoroutine());
        }

        private IEnumerator DelayedReplayCoroutine()
        {
            yield return new WaitForSeconds(_delay);
            Replay();
        }

        private float TimeFromBeginReplay
        {
            get { return Time.unscaledTime - _replayBeginTimestamp; }
        }

        public float TimeFromBeginReplayNormalized
        {
            get { return (TimeFromBeginReplay * AverageSpeed) / (_duration + _delay); }
        }

        private float AverageSpeed
        {
            get
            {
                List<float> results = new List<float>(10);
                for (var i = 0.1f; i <= 1f; i += 0.1f)
                {
                    results.Add(_speedCurve.Evaluate(i));
                }
                return results.Average();
            }
        }
    }
}
