using PestelLib.UI;
using UI.Battle;
using UI.Screens;
using UnityDI;
using UTPLib.Tasks.Base;

namespace Game.Initialization.GameTasks {
    public class GUISetupTask : AutoCompletedTask {
        protected override void AutoCompletedRun() {
            var gui = ContainerHolder.Container.Resolve<Gui>();
            gui.Show<ShipSelectionScreen>();
            //gui.Show<ControlsOverlay>(GuiScreenType.Overlay);
        }
    }
}