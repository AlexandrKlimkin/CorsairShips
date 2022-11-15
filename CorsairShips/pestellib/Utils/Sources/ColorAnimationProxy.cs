using UnityEngine;

namespace PestelLib.Utils
{
    [RequireComponent(typeof(Renderer))]
    public class ColorAnimationProxy : MonoBehaviour
    {
        [SerializeField] private Color _animatedColor = new Color(1, 1, 1, 1);

        [SerializeField] private string _colorShaderId = "_Color";
        [SerializeField] private Renderer _renderer;
        [SerializeField] private int _materialIdx;

        private Material _material;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();

            if (_materialIdx < 0 || _materialIdx >= _renderer.materials.Length)
            {
                Debug.LogError("Material id is outside of range");
                return;
            }

            _material = _renderer.materials[_materialIdx];

            if (_material != null)
            {
                _material.SetColor(_colorShaderId, _animatedColor);
            }
        }

        private void Update()
        {
            if (_material == null)
                return;

            _material.SetColor(_colorShaderId, _animatedColor);
        }
    }
}