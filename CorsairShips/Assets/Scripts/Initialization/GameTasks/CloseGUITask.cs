using PestelLib.UI;
using UnityDI;
using UTPLib.Tasks.Base;

namespace Game.Initialization.GameTasks {
    public class CloseGUITask : AutoCompletedTask {
        protected override void AutoCompletedRun() {
            var gui = ContainerHolder.Container.Resolve<Gui>();
            gui.CloseAllWindows();
        }
    }
}