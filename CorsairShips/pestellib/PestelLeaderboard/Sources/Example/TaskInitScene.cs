using PestelLib.Leaderboard;
using PestelLib.TaskQueueLib;
using PestelLib.UI;
using UnityDI;
#pragma warning disable CS0169, CS0649

namespace PestelLib.Leaderboard
{ 
	public class TaskInitScene : Task
	{
	    [Dependency] private Gui _gui;

        public override void Run()
	    {
            ContainerHolder.Container.BuildUp(this);
            _gui.Show<LeaderboardScreen>("ExampleLeaderboardScreen", GuiScreenType.Dialog);
	        OnComplete(this);
	    }
	}
}