using System.Collections;
using System.Collections.Generic;
using Game.Initialization;
using PestelLib.TaskQueueLib;
using UnityEngine;

namespace Game.Initialization {
    public class GameInitializer : CommonInitializer {
        protected override List<Task> SpecialTasks => InitializationParameters.LoadingGameTasks;
    }
}
