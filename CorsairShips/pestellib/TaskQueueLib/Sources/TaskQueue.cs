using System;
using System.Collections.Generic;

namespace PestelLib.TaskQueueLib
{
    public class TaskQueue
    {
        public Action<Task> OnBeforeTaskStart = _ => { };
        public Action OnQueueComplete = () => {} ;
        public Action<Task, Exception> OnQueueFailed; 

        private readonly List<Task> _tasks = new List<Task>();

        public void AddTask(Task task)
        {
            _tasks.Add(task);
        }

        public void RunQueue()
        {
            RunNextTask();
        }

        private void RunNextTask(Task task = null)
        {
            if (task != null)
            {
                task.OnComplete -= RunNextTask;
                task.OnFailed -= OnFailed;
            }

            if (_tasks.Count == 0)
            {
                OnQueueComplete();
                return;
            }

            var current = _tasks[0];
            _tasks.RemoveAt(0);

            OnBeforeTaskStart.Invoke(current);
            current.OnComplete += RunNextTask;
            current.OnFailed += OnFailed;
            current.Run();
        }

        private void OnFailed(Task task, Exception error)
        {
            task.OnComplete -= RunNextTask;
            task.OnFailed -= OnFailed;
            OnQueueFailed.Invoke(task, error);
        }
    }
}
