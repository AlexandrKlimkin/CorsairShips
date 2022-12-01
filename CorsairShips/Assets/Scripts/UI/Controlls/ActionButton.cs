using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Game.SeaGameplay.UI {
    public class ActionButton : MonoBehaviour {
        [SerializeField]
        private Image _CDImage;
        [SerializeField]
        private Color _ReloadingColor;
        [SerializeField]
        private Color _ReadyColor;

        private Func<float> _GetProgressFunc;

        public void Setup(Func<float> getProgress) {
            _GetProgressFunc = getProgress;
        }

        public void Clear() {
            _GetProgressFunc = null;
        }
        
        private void Update() {
            UpdateCD();
        }

        private void UpdateCD() {
            if(_GetProgressFunc == null)
                return;
            var progress = _GetProgressFunc();
            _CDImage.fillAmount = progress;
            _CDImage.color = progress < 1 ? _ReloadingColor : _ReadyColor;
        }
    }
}
