using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PestelLib.UI
{
    public class StateIcon : MonoBehaviour
    {
        [SerializeField] private Image3dSpawner _spawner;
        [SerializeField] private Sprite[] _images;

        public void Show(string iconName)
        {
            _spawner.Sprite = _images.First(i => i.name == iconName);
            _spawner.Show();
        }

        public void Hide()
        {
            _spawner.Hide();
        }
    }
}