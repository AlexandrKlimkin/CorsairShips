using UnityEngine.UI;

namespace PestelLib.UI
{
    public class Text3dSpawner : Widget3dSpawner
    {
        private string textToSet = "";

        public string Text
        {
            set
            {
                textToSet = value;
                if (widgetGameObj != null)
                {
                    Text label = widgetGameObj.GetComponent<Text>();
                    if (label != null)
                    {
                        label.text = textToSet;
                    }
                }
            }
            get { return textToSet; }
        }

        protected override void ProcessWidget()
        {
            base.ProcessWidget();
            
            Text label = Widget.GetComponent<Text>();
            label.text = textToSet;
        }
    }
}