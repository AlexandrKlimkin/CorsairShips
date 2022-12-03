using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Initialization;
using PestelLib.TaskQueueLib;
using UnityEngine;

namespace Game.Initialization {
    public class GameInitializer : CommonInitializer {
        protected override List<Task> SpecialTasks => InitializationParameters.Loading_BaseGame_Tasks
            .Concat(InitializationParameters.Loading_DeathMatch_Tasks).ToList();
    }
}
