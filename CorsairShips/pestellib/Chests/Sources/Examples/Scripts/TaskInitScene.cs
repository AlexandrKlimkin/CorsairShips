using PestelLib.SharedLogic;
using PestelLib.SharedLogicBase;
using PestelLib.TaskQueueLib;
using PestelLib.UI;
using S;
using UnityDI;
using UnityEngine;

namespace PestelLib.Chests
{ 
	public class TaskInitScene : Task
	{
	    [Dependency] private Gui _gui;
	    [Dependency] private ChestDefinitionsContainer _chestDefinitionsContainer;

        public override void Run()
	    {
            ContainerHolder.Container.BuildUp(this);

	        var sharedLogic = new SharedLogicDefault<ChestsTestDefinitions>(new UserProfile(), _chestDefinitionsContainer.SharedLogicDefs);
	        ContainerHolder.Container.RegisterInstance<ISharedLogic>(sharedLogic);

	        sharedLogic.OnLogMessage += delegate(string s) { Debug.Log("SL: " + s); };

            //_gui.Show<ChestScreen>("ExampleChestScreen", GuiScreenType.Screen);
	        OnComplete(this);
	    }
	}
}