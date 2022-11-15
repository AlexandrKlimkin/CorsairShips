using System;
using System.Collections.Generic;
using PestelLib.TaskQueueLib;
using UnityDI;

namespace UTPLib.Tasks {
    public static class TasksExtensions {
        public static TaskQueue RunTasksListAsQueue(this List<Task> tasks, Action onQueueComplete, Action<Task, Exception> onQueueFailed, Action<Task> onTaskCompleted) {
            var queue = new TaskQueue();
            var container = ContainerHolder.Container;
            tasks?.ForEach(_ => {
                queue.AddTask(_);
            });
            queue.OnBeforeTaskStart = _ => {
                container.BuildUp(_.GetType(), _);
                _.OnComplete += onTaskCompleted;
            };
            queue.OnQueueComplete = onQueueComplete;
            queue.OnQueueFailed = onQueueFailed;
            queue.RunQueue();
            return queue;
        }
        public static TaskQueue RunTasksListAsQueue(this IEnumerable<Task> tasks, Action onQueueComplete, Action<Task, Exception> onQueueFailed, Action<Task> onTaskCompleted) {
            var queue = new TaskQueue();
            var container = ContainerHolder.Container;
            tasks?.ForEach(_ => {
                queue.AddTask(_);
            });
            queue.OnBeforeTaskStart = _ => {
                container.BuildUp(_.GetType(), _);
                _.OnComplete += onTaskCompleted;
            };
            queue.OnQueueComplete = onQueueComplete;
            queue.OnQueueFailed = onQueueFailed;
            queue.RunQueue();
            return queue;
        }

        public static void RunTask(this Task task, Action onComplete, Action<Task, Exception> onFail) {
            void done(Task t) {
                task.OnComplete -= done;
                task.OnFailed -= fail;
                onComplete?.Invoke();
            }
            void fail(Task t, Exception e) {
                task.OnComplete -= done;
                task.OnFailed -= fail;
                onFail?.Invoke(t, e);
            }
            task.OnComplete += done;
            task.OnFailed += fail;
            ContainerHolder.Container.BuildUp(task.GetType(), task);
            task.Run();
        }

        public static void RunTask(this Task task, Action<Task> onComplete, Action<Task, Exception> onFail) {
            void done(Task t) {
                task.OnComplete -= done;
                task.OnFailed -= fail;
                onComplete?.Invoke(task);
            }
            void fail(Task t, Exception e) {
                task.OnComplete -= done;
                task.OnFailed -= fail;
                onFail?.Invoke(t, e);
            }
            task.OnComplete += done;
            task.OnFailed += fail;
            ContainerHolder.Container.BuildUp(task.GetType(), task);
            task.Run();
        }
    }
}
