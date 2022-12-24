using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class IconLabel : MonoBehaviour {
        [SerializeField]
        private Image _Icon;
        [SerializeField]
        private TextMeshProUGUI _Label;

        public void SetIcon(Sprite sprite) {
            _Icon.sprite = sprite;
        }

        public void SetLabel(string text) {
            _Label.text = text;
        }

        public void SetIconColor(Color color) {
            _Icon.color = color;
        }

        public void SetLabelColor(Color color) {
            _Label.color = color;
        }
    }
}
