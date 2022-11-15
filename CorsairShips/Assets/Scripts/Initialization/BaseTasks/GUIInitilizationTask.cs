using PestelLib.UI;
using UnityDI;
using UTPLib.Tasks.Base;

public class GUIInitilizationTask : AutoCompletedTask
{
    protected override void AutoCompletedRun()
    {
        ContainerHolder.Container.RegisterUnitySingleton<Gui>(null, true);
        ContainerHolder.Container.RegisterCustom(() => (IGui)ContainerHolder.Container.Resolve<Gui>());
        ContainerHolder.Container.Resolve<Gui>();
        
        // GenericMessageBoxScreen.DefaultPrefabOverride = ResourcePath.Main.DefaultMessageBox;
        // Purchaser.ProcessingPaymentPrefab = GenericMessageBoxScreen.DefaultPrefabOverride;
        
        // gui.gameObject.AddComponent<UIManager>();
    }
}