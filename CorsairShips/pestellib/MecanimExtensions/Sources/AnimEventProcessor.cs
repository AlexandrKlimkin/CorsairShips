using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PestelLib.MecanimExtensions
{
    [RequireComponent(typeof(Animator))]
    public class AnimEventProcessor : MonoBehaviour
    {
        public Action<TimestampedAnimEvent> OnAnimEvent = e => { };

        [SerializeField] private List<string> _ignoredEvents;

        private List<TimestampedAnimEvent> _eventsQueue = new List<TimestampedAnimEvent>();

        private Animator _animator;

        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        public void AddEvent(AnimationEvent evt, AnimationClip clip)
        {
            if (_ignoredEvents.Contains(evt.functionName)) return;

            _eventsQueue.Add(new TimestampedAnimEvent
            {
                Timestamp = Time.time + evt.time + Time.deltaTime,
                Event = evt.functionName
            });
        }

        public void RemoveEvents(string functionName)
        {
            for (var i = _eventsQueue.Count - 1; i >=0; i--)
            {
                var evt = _eventsQueue[i];
                if (evt.Event == functionName)
                {
                    _eventsQueue.RemoveAt(i);
                }
            }
        }

        public float? TimeToEvent(string functionName)
        {
            var evt = _eventsQueue.FirstOrDefault(x => x.Event == functionName);
            if (evt != null)
            {
                return evt.Timestamp - Time.time;
            }

            return null;
        }

        public void OffsetAllEvents(float offset)
        {
            for (var i = 0; i < _eventsQueue.Count; i++)
            {
                _eventsQueue[i].Timestamp += offset;
            }
        }

        private void Update()
        {
            //TODO: add support for animator.speed

            for (var i = 0; i < _eventsQueue.Count; i++)
            {
                var evt = _eventsQueue[i];
                if (evt.Timestamp <= Time.time)
                {
                    OnAnimEvent(_eventsQueue[i]);
                }
            }

            for (var i = _eventsQueue.Count - 1; i >= 0; i--)
            {
                if (_eventsQueue[i].Timestamp <= Time.time)
                {
                    _eventsQueue.RemoveAt(i);
                }
            }
        }
    }
}
