using PestelLib.UI;
using UI.Screens.MenuMain;
using UnityDI;
using UTPLib.Tasks.Base;

namespace Initialization.BaseTasks {
    public class MenuGuiLoadTask : AutoCompletedTask {
        protected override void AutoCompletedRun() {
            var gui = ContainerHolder.Container.Resolve<Gui>();
            gui.Show<MenuMainScreen>(GuiScreenType.Screen);
        }
    }
}