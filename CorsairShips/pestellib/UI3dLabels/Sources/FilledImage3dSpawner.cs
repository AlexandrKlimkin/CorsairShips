using UnityEngine;
using UnityEngine.UI;

namespace PestelLib.UI
{
    public class FilledImage3dSpawner : Widget3dSpawner
    {
        private Image _bgImage;
        private Image _fgImage;

        public Sprite BgSprite
        {
            get { return _bgImage.sprite; }
            set { _bgImage.sprite = value; }
        }

        public Sprite FgSprite
        {
            get { return _fgImage.sprite; }
            set { _fgImage.sprite = value; }
        }

        public void SetColor(Color bgColor, Color fgColor)
        {
            _bgImage.color = bgColor;
            _fgImage.color = fgColor;
        }

        public void Show()
        {
            _bgImage.enabled = _fgImage.enabled = true;
        }

        public void Hide()
        {
            _bgImage.enabled = _fgImage.enabled = false;
        }

        public void SetFillAmount(float value)
        {
            _fgImage.fillAmount = value;
        }

        protected override void ProcessWidget()
        {
            base.ProcessWidget();

            var childs = Widget.GetComponentsInChildren<Image>();

            if (childs.Length != 2)
            {
                Debug.LogError("FilledImage must have foreground and background images only");
                return;
            }

            foreach (Image child in childs)
            {
                if (child.type == Image.Type.Filled)
                {
                    _fgImage = child;
                }
                else
                {
                    _bgImage = child;
                }
            }
        }
    }
}