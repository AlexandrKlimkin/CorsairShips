using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PestelLib.Utils;
using UnityDI;
using UnityEngine;
using UnityEngine.UI;

namespace GlobalConflictModule.Example
{
    public class UiSelectPlayer : MonoBehaviour
    {
        [SerializeField] private Dropdown _ddPlayers;
        [SerializeField] private Button _btn_Login;
        [SerializeField] private Button _btn_New;
        [SerializeField] private GameObject[] _makeActive;
        private List<string> _players;

        public static string PlayerId;
        public static readonly string PrefsKey = "GC_Example_Players";
        void Start()
        {
            if (_makeActive == null)
                return;
            foreach (var o in _makeActive)
            {
                o.SetActive(false);
            }

            _ddPlayers.ClearOptions();
            _players = PlayerPrefs.GetString(PrefsKey, "").Split(new [] { ';'}, StringSplitOptions.RemoveEmptyEntries).ToList();

            _ddPlayers.AddOptions(_players);
            _ddPlayers.value = 0;
            _btn_Login.interactable = _players.Count > 0;

            _btn_Login.onClick.AddListener(OnLogin);
            _btn_New.onClick.AddListener(OnNew);
        }

        private void Continue()
        {
            gameObject.SetActive(false);
            if (_makeActive == null)
                return;
            foreach (var o in _makeActive)
            {
                o.SetActive(true);
            }
        }

        private void OnNew()
        {
            var newPlayer = Guid.NewGuid();
            PlayerId = newPlayer.ToString();
            _players.Add(PlayerId);
            var data = string.Join(";", _players.ToArray());
            PlayerPrefs.SetString(PrefsKey, data);
            Continue();
        }

        private void OnLogin()
        {
            PlayerId = _ddPlayers.options[_ddPlayers.value].text;
            Continue();
        }
    }
}
