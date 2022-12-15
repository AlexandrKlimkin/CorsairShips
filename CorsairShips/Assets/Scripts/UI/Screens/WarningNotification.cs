using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI.Battle {
    public class WarningNotification : MonoBehaviour {
        [SerializeField]
        private TextMeshProUGUI _Text;

        public void Setup(string text) {
            _Text.text = text;
        }
    }
}
