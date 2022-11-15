using UnityDI;
using UnityEngine;
using UnityEngine.UI;

namespace PestelLib.Localization
{
    public class LocalizeText : MonoBehaviour
    {
        [Dependency] private ILocalization _localizationData;
        [SerializeField] private Text _text;
        [SerializeField] private string _key;

        private void Awake()
        {
            ContainerHolder.Container.BuildUp(this);
            if (_text == null)
            {
                _text = GetComponent<Text>();
            }

            _localizationData.OnChangeLocale += OnChangeLocale;
            OnChangeLocale();
        }

        private void OnDestroy()
        {
            if (_localizationData != null)
            {
                _localizationData.OnChangeLocale -= OnChangeLocale;
            }
        }

        private void OnChangeLocale()
        {
            if (_text != null && _localizationData != null)
            {
                _text.text = _localizationData.Get(_key);
            }
        }

        public string Key
        {
            get { return _key; }
            set
            {
                _key = value;
                OnChangeLocale();
            }
        }
    }
}
