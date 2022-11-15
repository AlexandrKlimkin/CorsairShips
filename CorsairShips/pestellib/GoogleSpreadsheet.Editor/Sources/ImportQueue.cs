using System;
using System.Collections;

namespace PestelLib.GoogleSpreadsheet.Editor
{
    internal class ImportQueue : IDisposable
    {
        public Action OnComplete = () => { };

        private readonly Queue _queue;
        private readonly EditorProgressBar _progressBar;
        private readonly Func<ImportPageData, IEnumerator> _action;

        private EditorCoroutine _queueRoutine;

        private static ImportQueue _instance;

        internal static ImportQueue CreateQueue(Func<ImportPageData, IEnumerator> action, EditorProgressBar bar)
        {
            if (_instance != null)
            {
                _instance.Dispose();
            }

            _instance = new ImportQueue(action, bar);
            return _instance;
        }

        private ImportQueue(Func<ImportPageData, IEnumerator> action, EditorProgressBar bar)
        {
            _progressBar = bar;
            _action = action;
            _queue = new Queue();
        }

        internal void AddTask(ImportPageData data)
        {
            _queue.Enqueue(data);
            _progressBar.AddTask();

            if (_queueRoutine == null)
            {
                Run();
            }
        }

        private void Run()
        {
            _queueRoutine = EditorCoroutine.start(DoTasks());
        }

        private IEnumerator DoTasks()
        {
            while (_queue.Count != 0)
            {
                var activeElement = (ImportPageData) _queue.Dequeue();

                var enumerator = _action(activeElement);
                _progressBar.SetTaskTitle("Importing " + activeElement.PageName + "...");

                while (enumerator.MoveNext())
                    yield return null;

                _progressBar.TaskDone();
            }

            OnComplete();
            _queueRoutine = null;
        }

        public void Dispose()
        {
            if (_queueRoutine != null)
            {
                _queueRoutine.stop();
                _queueRoutine = null;
            }
            _progressBar.Hide();
        }
    }
}