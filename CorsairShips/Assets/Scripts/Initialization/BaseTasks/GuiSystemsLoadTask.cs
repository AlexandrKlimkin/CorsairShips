using PestelLib.UI;
using UI.FloatingTexts;
using UI.Markers;
using UnityDI;
using UTPLib.Tasks.Base;

namespace Core.Initialization.Base 
{
	public class GuiSystemsLoadTask : AutoCompletedTask 
	{
		protected override void AutoCompletedRun() 
		{
			var gui = ContainerHolder.Container.Resolve<Gui>();
			gui.Show<MarkerCanvas>(GuiScreenType.Permanent);
			gui.Show<FloatingTextsSystem>(GuiScreenType.Permanent);
		}
	}
}