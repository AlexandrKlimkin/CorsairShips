using PestelLib.UI;
using UI.Battle;
using UnityDI;
using UTPLib.Tasks.Base;

namespace Game.Initialization.GameTasks {
    public class GUISetupTask : AutoCompletedTask {
        protected override void AutoCompletedRun() {
            var gui = ContainerHolder.Container.Resolve<Gui>();
            gui.Show<ControllsOverlay>(GuiScreenType.Overlay);
        }
    }
}