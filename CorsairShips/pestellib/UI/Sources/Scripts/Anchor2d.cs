using UnityEngine;

namespace PestelLib.UI
{
    public class Anchor2d : MonoBehaviour
    {
        [SerializeField] private RectTransform _target;
        
        private void LateUpdate()
        {
            transform.position = _target.position;
        }
    }
}