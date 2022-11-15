using UnityEngine;
using UnityEngine.UI;

namespace PestelLib.Utils
{
    [RequireComponent(typeof(UnityEngine.UI.Graphic))]
    public class UIColorAnimationProxy : MonoBehaviour
    {
        [SerializeField] private Color _animatedColor = new Color(1, 1, 1, 1);
        private Graphic _target;

        private void Awake()
        {
            _target = GetComponent<Graphic>();
        }

        private void LateUpdate()
        {
            _target.color = _animatedColor;
        }
    }
}
