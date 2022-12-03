using System.Collections.Generic;
using System.Linq;
using PestelLib.TaskQueueLib;
using UTPLib.Services.SceneManagement;

namespace Game.Initialization.Parameters {
    public class DeathMatchLoadingParameters : SceneLoadingParameters {
        public override List<Task> LoadingTasks =>
            InitializationParameters.Loading_BaseGame_Tasks
                .Concat(InitializationParameters.Loading_DeathMatch_Tasks).ToList();

        public override List<Task> UnloadingTasks => InitializationParameters.Unloading_DeathMatch_Tasks
            .Concat(InitializationParameters.Unloading_BaseGame_Tasks).ToList();
    }
}