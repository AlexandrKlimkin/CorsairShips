using PestelLib.SharedLogic;
using PestelLib.SharedLogicBase;
using PestelLib.TaskQueueLib;
using PestelLib.UI;
using S;
using Submarines;
using UnityDI;
using UnityEngine;

namespace PestelLib.Roulette
{
    public class TaskInitScene : Task
    {
        [Dependency] private Gui _gui;
        [Dependency] private RouletteDefinitionsContainer _rouletteDefinitionsContainer;

        public override void Run()
        {
            ContainerHolder.Container.BuildUp(this);

            var sharedLogic = new SharedLogicDefault<RouletteTestDefinitions>(new UserProfile(), _rouletteDefinitionsContainer.SharedLogicDefs);
            ContainerHolder.Container.RegisterInstance<ISharedLogic>(sharedLogic);

            sharedLogic.OnLogMessage += delegate (string s) { Debug.Log("SL: " + s); };

            _gui.Show<ChestShopScreen>();

            OnComplete(this);
        }
    }
}