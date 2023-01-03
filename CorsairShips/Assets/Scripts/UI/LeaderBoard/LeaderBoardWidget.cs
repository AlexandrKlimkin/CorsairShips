using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI.Battle.LeaderBoard {
    public class LeaderBoardWidget : MonoBehaviour {
        [SerializeField]
        private TextMeshProUGUI _Place;
        [SerializeField]
        private TextMeshProUGUI _Nickname;
        [SerializeField]
        private TextMeshProUGUI _Points;
        [SerializeField]
        private Color _LocalPlayerColor;
        [SerializeField]
        private Color _UsualColor;

        public byte PlayerId { get; private set; }
        
        public void Setup(string nickname, string points, bool isLocalPlayer, byte playerId) {
            PlayerId = playerId;
            _Nickname.text = nickname;
            _Points.text = points;
            _Nickname.color = isLocalPlayer ? _LocalPlayerColor : _UsualColor;
            _Points.color = isLocalPlayer ? _LocalPlayerColor : _UsualColor;
            _Place.color = isLocalPlayer ? _LocalPlayerColor : _UsualColor;
            _Nickname.fontStyle = isLocalPlayer ? FontStyles.Bold : FontStyles.Normal;
            _Points.fontStyle = isLocalPlayer ? FontStyles.Bold : FontStyles.Normal;
            _Place.fontStyle = isLocalPlayer ? FontStyles.Bold : FontStyles.Normal;
        }

        public void RefreshPlace(int place) {
            _Place.text = $"{place}.";
        }
    }
}
