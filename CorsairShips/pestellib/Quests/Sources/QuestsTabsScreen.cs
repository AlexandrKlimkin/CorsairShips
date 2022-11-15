using PestelLib.UI;
using UnityDI;
using UnityEngine;

namespace PestelLib.Quests
{
    public class QuestsTabsScreen : MonoBehaviour
    {
        [Dependency] protected Gui _gui;

        protected virtual void Start()
        {
            ContainerHolder.Container.BuildUp(this);
        }

        public virtual void Close()
        {
            _gui.GoBack();
        }

        public virtual void SelectTab(string tab)
        {
          
        }
    }
}