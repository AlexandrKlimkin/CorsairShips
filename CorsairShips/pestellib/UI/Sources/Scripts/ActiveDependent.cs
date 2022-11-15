using UnityEngine;

namespace Assets.Plugins.LibsSymlinks.UI
{
    public class ActiveDependent : MonoBehaviour
    {
        [SerializeField] private GameObject _target;
        [SerializeField] private bool _inverse = false;

        void OnEnable()
        {
            _target.SetActive(!_inverse);
        }

        void OnDisable()
        {
            _target.SetActive(_inverse);
        }
    }
}