using UnityEngine;
using UnityEngine.UI;

namespace PestelLib.UI
{
    public class GenericMessageBoxView : MonoBehaviour, IMessageBoxView
    {
        public bool HaveActiveBtn { get { return (_buttonA != null && _buttonA.activeInHierarchy) || (_buttonB != null && _buttonB.activeInHierarchy); } }

        [SerializeField] protected Text _caption;
        [SerializeField] protected Text _description;
        [SerializeField] protected Text _buttonLabelA;
        [SerializeField] protected Text _buttonLabelB;
        [SerializeField] protected Image _icon;
        [SerializeField] protected Sprite[] _sprites;

        [SerializeField] protected GameObject _buttonA;
        [SerializeField] protected GameObject _buttonB;

        protected string _sprite;

        public virtual string Caption 
        {
            set { _caption.text = value; } 
        }

        public virtual string Description
        {
            set { _description.text = value; }
        }

        public virtual string Icon
        {
            set { _sprite = value; }
        }

        public virtual string ButtonA 
        {
            set
            {
                _buttonLabelA.text = value;                
            }
        }

        public virtual string ButtonB
        {
            set
            {
                _buttonLabelB.text = value;
                _buttonB.SetActive(!string.IsNullOrEmpty(value));
            }
        }

        private void OnDestroy()
        {
            if (CantClose)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
#if UNITY_2018_3_OR_NEWER
                Application.Quit(1);
#else
                Application.Quit();
#endif
#endif
            }
        }

        public bool CantClose { get; set; }

        public virtual void UpdateView()
        {
            var success = false;
            for (int i = 0; i < _sprites.Length; i++)
            {
                if (_sprites[i].name == _sprite)
                {
                    if (_icon != null)
                    {
                        _icon.gameObject.SetActive(true);
                        _icon.overrideSprite = _sprites[i];
                    }
                    success = true;
                }
            }

            if (!success)
            {
                if (_icon != null)
                {
                    _icon.gameObject.SetActive(false);
                }
            }

            _buttonA.SetActive(!string.IsNullOrEmpty(_buttonLabelA.text));
            _buttonB.SetActive(!string.IsNullOrEmpty(_buttonLabelB.text));
        }

        public virtual void ShowDescription(bool show = true)
        {
            _description.gameObject.SetActive(show);
        }
    }
}