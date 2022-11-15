using System;
using UnityDI;
using UnityEngine;

namespace PestelLib.Utils
{
    public class UnityEventsProvider : MonoBehaviour
    {
        public Action<bool> OnApplicationPaused;
        public Action<bool> OnApplicationFocused;
        public event Action OnAwake;
        public event Action OnEnableEvent;
        public event Action OnStart;
        public event Action OnUpdate;
        public event Action OnFixedUpdate;
        public event Action OnLateUpdate;
        public event Action OnDisableEvent;
        public event Action OnDestroyEvent;
        public event Action OnGizmos;

        private void OnApplicationPause(bool pauseStatus)
        {
            OnApplicationPaused?.Invoke(pauseStatus);
        }
        private void OnApplicationFocus(bool focuseStatus)
        {
            OnApplicationFocused?.Invoke(focuseStatus);
        }

        private void Awake()
        {
            ContainerHolder.Container.BuildUp(this);
            OnAwake?.Invoke();
        }

        private void OnEnable()
        {
            OnEnableEvent?.Invoke();
        }

        private void Start()
        {
            OnStart?.Invoke();
        }

        private void Update()
        {
            OnUpdate?.Invoke();
        }

        private void FixedUpdate()
        {
            OnFixedUpdate?.Invoke();
        }

        private void LateUpdate()
        {
            OnLateUpdate?.Invoke();
        }
        private void OnDisable()
        {
            OnDisableEvent?.Invoke();
        }
        private void OnDestroy()
        {
            OnDestroyEvent?.Invoke();
        }

        private void OnDrawGizmos()
        {
            OnGizmos?.Invoke();
        }
    }
}