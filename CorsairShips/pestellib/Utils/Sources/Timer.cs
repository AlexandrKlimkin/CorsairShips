using System;
using UnityEngine;

namespace PestelLib.Utils
{
    public class Timer : IDisposable
    {
        private enum State
        {
            NULL,
            IDLE,
            RUNNING
        }

        private UpdateProvider _updateProvider;

        private float _timeToRun;
        private State _state;

        private Action OnTimeOut = () => { };
        private Action<float, float> OnUpdate = (time, normalizedTime) => { };
        private Action<int> OnSecondTick;

        private bool _isUnscaledTime;

        private float DeltaTime
        {
            get { return (_isUnscaledTime) ? Time.unscaledDeltaTime : Time.deltaTime; }
        }

        public Timer(UpdateProvider updateProvider, float timeToRun)
        {
            _updateProvider = updateProvider;
            _updateProvider.OnUpdate += Update;

            _timeToRun = timeToRun;

            ResetTime();
        }

        public Timer(UpdateProvider updateProvider, float timeToRun, Action onTimeOut) : this(updateProvider, timeToRun)
        {
            _updateProvider = updateProvider;
            _updateProvider.OnUpdate += Update;

            _timeToRun = timeToRun;
            OnTimeOut = onTimeOut;

            ResetTime();
        }

        public Timer(UpdateProvider updateProvider, float timeToRun, Action onTimeOut, Action<float, float> onUpdate) : this(updateProvider, timeToRun)
        {
            _updateProvider = updateProvider;
            _updateProvider.OnUpdate += Update;

            _timeToRun = timeToRun;
            OnTimeOut = onTimeOut;
            OnUpdate = onUpdate;

            ResetTime();
        }

        public void SetCallbacks(Action onTimeOut, Action<float, float> onUpdate = null, Action<int> onSecondTick = null)
        {
            OnTimeOut = onTimeOut;
            OnUpdate = onUpdate;
            OnSecondTick = onSecondTick;
        }

        public void SetUnscaledTime(bool isUnscaled)
        {
            _isUnscaledTime = isUnscaled;
        }

        public void Run()
        {
            _state = State.RUNNING;
        }

        public void Stop()
        {
            _state = State.IDLE;
        }

        public void ResetTime()
        {
            _currentTime = _timeToRun;
            _currentSecond = Mathf.CeilToInt(_timeToRun);
        }

        public void SetNewTime(float time)
        {
            if (_state == State.RUNNING)
            {
                Debug.LogError("You cannot set new time while timer is running");
                return;
            }
            _timeToRun = time;
            ResetTime();
        }

        private float _currentTime;
        private int _currentSecond;
        private void Update()
        {
            if (_state != State.RUNNING) return;

            _currentTime -= DeltaTime;

            int ceilSecond = Mathf.CeilToInt(_currentTime);

            if (ceilSecond != _currentSecond && OnSecondTick != null)
            {
                OnSecondTick(ceilSecond);
            }

            _currentSecond = ceilSecond;

            OnUpdate(_currentTime, _currentTime/_timeToRun);

            if (_currentTime <= 0f)
            {
                _state = State.IDLE;
                OnTimeOut();
            }
        }

        public void Dispose()
        {
            if (_updateProvider != null)
            {
                _updateProvider.OnUpdate -= Update;
            }
        }
    }
}