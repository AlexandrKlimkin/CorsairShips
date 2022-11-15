using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PestelLib.DeadlyFastFSM {
    public class DeadlyFastFsmMediator : MonoBehaviour
    {
        [Serializable]
        public class StoredEvent : ISerializationCallbackReceiver
        {
            public Type TargetType;
            public string EventName;

            [HideInInspector]
            [SerializeField]
            private string _targetTypeName;

            public void OnBeforeSerialize()
            {
                if (TargetType != null)
                {
                    _targetTypeName = TargetType.AssemblyQualifiedName;
                }
            }

            public void OnAfterDeserialize()
            {
                TargetType = Type.GetType(_targetTypeName);
            }
        }

        public MonoBehaviour Fsm;

        private IDeadlyFastFsm FastFsm
        {
            get { return (IDeadlyFastFsm) Fsm; }
        }

        [HideInInspector] public List<StoredEvent> Events = new List<StoredEvent>();

        private void Start()
        {
            foreach (var storedEvent in Events)
            {
                RegisterEvent(storedEvent);
            }
        }

        public void RegisterEvent(StoredEvent storedEvent)
        {
            if (string.IsNullOrEmpty(storedEvent.EventName) || storedEvent.TargetType == null)
                return;

            var eventInfo = storedEvent.TargetType.GetEvent(storedEvent.EventName,
                BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
            if (eventInfo == null)
            {
                Debug.LogWarning(string.Format("Event was '{0}' not found", storedEvent.EventName));
                return;
            }

            string fsmEventName = storedEvent.EventName;

            var target = GetComponent(storedEvent.TargetType);
            if (target == null)
            {
                Debug.LogWarning("Can't find component " + storedEvent.TargetType.Name);
                return;
            }

            //public void ProcessEvent(CharacterFSM.FsmEvent evt)

            Action pointer = () => FastFsm.ProcessEvent(fsmEventName);
            //eventInfo.AddEventHandler(target, pointer);
            var add = eventInfo.GetAddMethod();
            add.Invoke(target, new object[] {pointer});
        }
    }
}
