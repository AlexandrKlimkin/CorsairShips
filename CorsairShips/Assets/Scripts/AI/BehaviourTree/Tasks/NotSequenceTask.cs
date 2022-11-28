
namespace Tools.BehaviourTree
{

    public class NotSequenceTask : Task
    {

        private int _CurrentTaskIndex;

        public override void Init()
        {
        }

        public override void Begin()
        {
            _CurrentTaskIndex = 0;
        }

        public override TaskStatus Run()
        {
            TaskStatus childStatus;
            while (_CurrentTaskIndex < Children.Count)
            {
                childStatus = Children[_CurrentTaskIndex].UpdateTask();
                if (childStatus == TaskStatus.Success)
                {
                    return TaskStatus.Failure;
                }
                if (childStatus == TaskStatus.Failure)
                {
                    _CurrentTaskIndex++;
                }
                if (childStatus == TaskStatus.Running)
                {
                    return TaskStatus.Running;
                }
            }
            return TaskStatus.Success;
        }
    }
}
