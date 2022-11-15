using UnityEngine;
using UnityEngine.UI;

namespace PestelLib.UI
{
    public class Image3dSpawner : Widget3dSpawner
    {
        private Image _image;

        public Sprite Sprite
        {
            get { return _image.sprite; }
            set { _image.sprite = value; }
        }

        public void Show()
        {
            _image.enabled = true;
        }

        public void Hide()
        {
            _image.enabled = false;
        }

        protected override void ProcessWidget()
        {
            base.ProcessWidget();

            _image = Widget.GetComponent<Image>();
        }
    }
}