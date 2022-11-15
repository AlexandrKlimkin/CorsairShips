using PestelLib.SharedLogic;
using PestelLib.SharedLogicBase;
using PestelLib.TaskQueueLib;
using PestelLib.UI;
using S;
using UnityDI;
using UnityEngine;

namespace PestelLib.DailyRewards
{ 
	public class TaskInitScene : Task
	{
	    [Dependency] private Gui _gui;
	    [Dependency] private DailyRewardsDefinitionsContainer _dailyRewardsDefinitionsContainer;

        public override void Run()
	    {
            ContainerHolder.Container.BuildUp(this);

            var userProfile = new UserProfile();
            var sharedLogic = new SharedLogicDefault<DailyRewardsTestDefinitions>(userProfile, _dailyRewardsDefinitionsContainer.SharedLogicDefs);

            ContainerHolder.Container.RegisterInstance<ISharedLogic>(sharedLogic);

            sharedLogic.OnLogMessage += delegate(string s) { Debug.Log("SL: " + s); };

            //_gui.Show<ChestScreen>("ExampleChestScreen", GuiScreenType.Screen);
	        OnComplete(this);
	    }
	}
}