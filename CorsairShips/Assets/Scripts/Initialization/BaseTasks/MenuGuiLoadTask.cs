using Menu.Camera;
using PestelLib.UI;
using UI.Screens.MenuMain;
using UnityDI;
using UTPLib.Tasks.Base;

namespace Initialization.BaseTasks {
    public class MenuGuiLoadTask : AutoCompletedTask {
        protected override void AutoCompletedRun() {
            var gui = ContainerHolder.Container.Resolve<Gui>();
            gui.Show<MenuCameraInputPanel>(GuiScreenType.Background);
            gui.Show<MenuMainScreen>(GuiScreenType.Screen);
            gui.Show<CurrencyOverlay>(GuiScreenType.Overlay);
        }
    }
}