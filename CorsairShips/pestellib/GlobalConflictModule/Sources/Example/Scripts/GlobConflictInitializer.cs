using System;
using System.Collections;
using System.Collections.Generic;
using PestelLib.TaskQueueLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GlobalConflict.Example
{
    public class GlobConflictInitializer : MonoBehaviour
    {
        private static TaskQueue _taskQueue = new TaskQueue();

        public static bool Initialized { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            if (SceneManager.GetActiveScene().name != "GlobalConflictExamplePlayer")
                return;

            _taskQueue.OnQueueComplete += () => Initialized = true;
            _taskQueue.AddTask(new GlobConflictTaskInitDependencyInjection());
            _taskQueue.AddTask(new GlobConflictTaskInitSL());

            _taskQueue.OnQueueComplete += () => { Debug.Log("Initialization done."); };

            _taskQueue.RunQueue();
        }
    }
}
