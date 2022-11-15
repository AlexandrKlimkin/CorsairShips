using System.Globalization;
using PestelLib.SharedLogic.Modules;
using UnityDI;
using UnityEngine;
using UnityEngine.UI;
#pragma warning disable CS0169, CS0649

namespace PestelLib.Leaderboard
{
    public class LeaderboardScreenItem : MonoBehaviour
    {
        [Dependency] private LeaderboardLeagueModule _leaderboardLeagueModule;

        [SerializeField] private Texture2D _defaultAvatar;
        [SerializeField] private RawImage _icon;

        [SerializeField] private Text _rank;
        [SerializeField] private Text _name;
        [SerializeField] private Text _score;
        [SerializeField] private Text _league;
        [SerializeField] private Image _leagueIcon;
        [SerializeField] private GameObject _myRecordIndication;

        [SerializeField] private bool _avatarLoadingEnabled = false;
        [SerializeField] private bool _socialNameLoadingEnabled = false;

        public bool IsMyRecord;

        private S.LeaderboardRecord _data;

        public S.LeaderboardRecord Data
        {
            get { return _data; }

            set
            {
                _data = value;

                if (_leaderboardLeagueModule == null)
                {
                    _leaderboardLeagueModule = ContainerHolder.Container.Resolve<LeaderboardLeagueModule>();
                }

                _icon.texture = _defaultAvatar;
                _score.text = "0";
                _rank.text = "0";

                if (_data == null)
                {
                    Debug.LogWarning("Data  is null, skip");
                    return;
                }
                
                _rank.text = (value.Rank + 1).ToString(CultureInfo.InvariantCulture);
                _name.text = value.Name != null ? value.Name.ToString(CultureInfo.InvariantCulture) : "NAME IS NULL";
                _score.text = value.Score.ToString(CultureInfo.InvariantCulture);

                if (_league != null)
                {
                    _league.text = LocalizedLeagueName(_leaderboardLeagueModule.GetLeague(value.Score).Name);
                }

                if(_leagueIcon != null)
                {
                    _leagueIcon.sprite = LeagueIcon();
                }

                if(_myRecordIndication != null)
                {
                    _myRecordIndication.SetActive(IsMyRecord);
                }                

                //_icon.texture = _defaultAvatar;
                LoadSocialData();
            }
        }

        protected virtual Sprite LeagueIcon()
        {
#if UNITY_2018_1_OR_NEWER
            return Sprite.Create(Texture2D.whiteTexture, new Rect(0,0, 8, 8), Vector2.zero);
#else
            return new Sprite();
#endif
        }

        protected virtual string LocalizedLeagueName(string key)
        {
            return key;
        }

        private void OnEnable()
        {
            if (_data == null) return;
            LoadSocialData();
        }

        private void LoadSocialData()
        {
            if (enabled && gameObject.activeInHierarchy)
            {
                var socialNet = ContainerHolder.Container.Resolve<LeaderboardToSocialNetworkMediator>();
                if (!socialNet.Connected) return;

                if (_avatarLoadingEnabled)
                {
                    StartCoroutine(socialNet.LoadUserImage(_data.FacebookId, _icon));
                }

                if (_socialNameLoadingEnabled)
                {
                    StartCoroutine(socialNet.LoadUserName(_data.FacebookId, (n) => { _name.text = n; }));
                }
            }
        }
    }
}
