using PestelLib.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace PestelLib.UI
{
    public class TextClone : MonoBehaviour
    {
        [SerializeField] private Text _text;
        [SerializeField] private RectTransform _textCanvas;
        [SerializeField] private string _textCanvasCustomTag = string.Empty;

        private GameObject _clone;
        private Text _textClone;
        private RectTransform _textCloneRectTransform;
        private RectTransform _originalTextRectTransform;

        private void Start()
        {
            _clone = Instantiate(_text.gameObject);
            Destroy(_clone.GetComponent<TextClone>());

            if (_textCanvas == null)
            {
                _textCanvas = TagRegistry.GetObjectByTag<RectTransform>(_textCanvasCustomTag);
            }

            _textClone = _clone.GetComponent<Text>();
            _textClone.material = null;
            _originalTextRectTransform = _text.GetComponent<RectTransform>();
            _textCloneRectTransform = _clone.GetComponent<RectTransform>();
            _textCloneRectTransform.SetParent(_textCanvas, true);
            
            _text.enabled = false;
        }

        private void LateUpdate()
        {
            _textCloneRectTransform.position = _originalTextRectTransform.position;

            if (_textClone.text != _text.text)
            {
                _textClone.text = _text.text;
            }
        }

        private void OnDisable()
        {
            if (_textClone != null)
            {
                _textClone.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (_textClone != null)
            {
                _textClone.gameObject.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            if (_textClone != null)
            {
                Destroy(_textClone);
            }
        }
    }
}