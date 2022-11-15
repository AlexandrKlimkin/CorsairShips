using UnityEditor;
using UnityEngine;

namespace PestelLib.GoogleSpreadsheet.Editor
{
    public class EditorProgressBar
    {
        private int _tasksCount;
        private int _tasksDone;

        private string _title;

        public EditorProgressBar(string title)
        {
            _title = title;
        }

        public void AddTask()
        {
            _tasksCount++;
        }

        public void SetTaskTitle(string taskTitle)
        {
            EditorUtility.DisplayProgressBar(_title, taskTitle, _tasksDone / (float)_tasksCount);
        }

        public void TaskDone()
        {
            _tasksDone++;

            if (_tasksCount == 0 || _tasksDone >= _tasksCount)
            {
                EditorUtility.ClearProgressBar();
            }
        }

        public void Hide()
        {
            EditorUtility.ClearProgressBar();
        }
    }
}