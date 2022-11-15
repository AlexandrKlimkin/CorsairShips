using PestelLib.TaskQueueLib;

namespace UTPLib.Tasks.Base {
    public abstract class AutoCompletedTask : Task {
        public sealed override void Run() {
            AutoCompletedRun();
            OnComplete.Invoke(this);
        }

        protected abstract void AutoCompletedRun();
    }
}