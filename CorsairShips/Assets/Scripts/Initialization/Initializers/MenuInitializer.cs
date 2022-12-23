using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Initialization;
using PestelLib.TaskQueueLib;
using UnityEngine;

namespace Menu.Initialization {
    public class MenuInitializer : CommonInitializer {
        protected override List<Task> SpecialTasks => InitializationParameters.Loading_Menu_Tasks;
    }
}
