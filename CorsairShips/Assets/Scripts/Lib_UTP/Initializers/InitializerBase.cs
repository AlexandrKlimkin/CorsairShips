using System;
using System.Collections.Generic;
using System.Linq;
using UTPLib.Tasks;
using PestelLib.TaskQueueLib;
using UnityEngine;

namespace UTPLib.Initialization {
    public abstract class InitializerBase : MonoBehaviour {
        [NonSerialized] public bool Complete;

        protected abstract List<Task> BaseTasks { get; }
        protected abstract List<Task> SpecialTasks { get; }

        protected static bool _InitializationRequested;
        protected bool _WasInitialized;

        private void Awake() {
            Initialize();
        }

        private void OnEnable() {
            Initialize();
        }

        private void Initialize() {
            if (_InitializationRequested)
                return;
            _WasInitialized = true;
            _InitializationRequested = true;
            BaseTasks
                .Concat(SpecialTasks)
                .ToList()
                .RunTasksListAsQueue(InitializationComplete, InitializationFailed, null);
        }

        protected virtual void InitializationComplete() {
            Debug.Log($"Services initialization complete");
            Complete = true;
        }

        protected virtual void InitializationFailed(Task task, Exception exception) {
            Debug.LogError($"Initialization task failed: {task} exception:{exception}");
        }
    }
}