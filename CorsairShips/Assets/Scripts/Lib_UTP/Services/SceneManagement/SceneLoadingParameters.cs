using System.Collections.Generic;
using UTPLib.Tasks;
using PestelLib.TaskQueueLib;

namespace UTPLib.Services.SceneManagement {
    public abstract  class SceneLoadingParameters {

        public abstract List<Task> LoadingTasks { get; }
        public abstract List<Task> UnloadingTasks { get; }

        public virtual void BeforeLoad() { }
        public virtual void AfterLoad() { }
        public virtual void BeforeUnload() { }
        public virtual void AfterUnload() { }
    }
}