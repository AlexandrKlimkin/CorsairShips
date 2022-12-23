using System.Collections.Generic;
using System.Linq;
using Game.Initialization;
using PestelLib.TaskQueueLib;
using UTPLib.Services.SceneManagement;

namespace Initialization.SceneLoading {
    public class MenuLoadingParameters : SceneLoadingParameters {
        public override List<Task> LoadingTasks => InitializationParameters.Loading_Menu_Tasks;
        public override List<Task> UnloadingTasks => InitializationParameters.Unloading_Menu_Tasks;
    }
}