using System.Linq;
using UnityEngine;

namespace PestelLib.UI
{
    public class StateIconProgress : MonoBehaviour
    {
        [SerializeField]
        private FilledImage3dSpawner _spawner;
        [SerializeField]
        private Sprite[] _images;

        public void Show(string iconName)
        {
            _spawner.BgSprite = _images.First(i => i.name == iconName);
            _spawner.FgSprite = _images.First(i => i.name == iconName);

            _spawner.Show();
        }

        public void SetColors(Color bgColor, Color fgColor)
        {
            _spawner.SetColor(bgColor, fgColor);
        }

        public void SetAmount(float val)
        {
            _spawner.SetFillAmount(val);
        }

        public void Hide()
        {
            _spawner.Hide();
        } 
    }
}