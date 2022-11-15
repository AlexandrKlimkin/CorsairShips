using System.Collections;
using System.Collections.Generic;
using PestelLib.TaskQueueLib;
using UnityEngine;
using UTPLib.Initialization;

namespace Game.Initialization {
    public abstract class CommonInitializer : InitializerBase {
        protected override List<Task> BaseTasks => InitializationParameters.BaseTasks;
    }
}
