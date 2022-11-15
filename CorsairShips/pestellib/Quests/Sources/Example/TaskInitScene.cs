using PestelLib.SharedLogic;
using PestelLib.SharedLogicBase;
using PestelLib.TaskQueueLib;
using PestelLib.UI;
using S;
using UnityDI;
using UnityEngine;

namespace PestelLib.Quests
{ 
	public class TaskInitScene : Task
	{
	    [Dependency] private Gui _gui;
	    [Dependency] private QuestsDefinitionsContainer _questDefinitionsContainer;

        public override void Run()
	    {
            ContainerHolder.Container.BuildUp(this);

	        var sharedLogic = new SharedLogicDefault<QuestsTestDefinitions>(new UserProfile(), _questDefinitionsContainer.SharedLogicDefs);
	        ContainerHolder.Container.RegisterInstance<ISharedLogic>(sharedLogic);

	        sharedLogic.OnLogMessage += delegate(string s) { Debug.Log("SL: " + s); };

            //_gui.Show<ChestScreen>("ExampleChestScreen", GuiScreenType.Screen);
	        OnComplete(this);
	    }
	}
}