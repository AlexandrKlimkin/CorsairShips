using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using Newtonsoft.Json;
using PestelLib.ServerClientUtils;
using PestelLib.ServerShared;
using PestelLib.UI;
using S;
using UnityDI;
using UnityEngine;
using UnityEngine.UI;
#pragma warning disable CS0169, CS0649

namespace PestelLib.Leaderboard
{
    public class LeaderboardScreen : MonoBehaviour
    {
        public Action OnRequestChunkComplete = () => {};

        [Dependency] private Gui _gui;
        [Dependency] protected RequestQueue _requestQueue;
        [Dependency] protected LeaderboardToSocialNetworkMediator _socialNetwork;

        [Header("Global")]
        [SerializeField] private LeaderboardScreenItem _playerGlobalItem;
        [SerializeField] private RectTransform _global;
        [SerializeField] private GameObject _globalBlocker;
        //[SerializeField] private LeaderboardScreenItem _itemPrefab;

        [Header("Chunk")]
        [SerializeField] protected LeaderboardScreenItem _playerChunkItem;
        [SerializeField] private RectTransform _chunk;
        [SerializeField] private GameObject _chunkBlocker;

        [Header("Friends")]
        [SerializeField] private LeaderboardScreenItem _playerFriendsItem;
        [SerializeField] private RectTransform _friends;
        [SerializeField] private GameObject _friendsBlocker;

        [Header("Debug")]
        [SerializeField] protected InputField _debugPlayerScore;
        public InputField _debugPlayerName;
        public InputField _debugFacebookId;
        public InputField _debugLeagueId;

        private int? _leagueIndex = null;

        private LeaderboardRecord _playerRecord;
        protected LeaderboardRecord PlayerRecord { get { return _playerRecord; } }
        private LeaderboardRecord _playerRecordInTop;
        protected LeaderboardRecord PlayerRecordInTop { get { return _playerRecordInTop; } }

        public int GlobalPlace;

        [SerializeField] private bool _ignoreFirstTimeOnEnable = true;

        protected virtual void Awake() 
        {
            ContainerHolder.Container.BuildUp(this);
            _debugPlayerName?.onEndEdit.AddListener(OnPlayerNameChanged);
            _debugFacebookId?.onEndEdit.AddListener(OnPlayerSocialIdChanged);
            _debugLeagueId?.onEndEdit.AddListener(OnLeagueIdSetted);
        }

        protected virtual void OnLeagueIdSetted(string leagueId)
        {
            if (Int32.TryParse(leagueId, out var leagueIndex))
            {
                _leagueIndex = leagueIndex;
                UpdateData();
            }
        }

        protected virtual void OnPlayerNameChanged(string newName)
        {
            PlayerPrefs.SetString("PlayerName", newName);
        }

        protected virtual void OnPlayerSocialIdChanged(string id)
        {
            PlayerPrefs.SetString("PlayerSocialID", id);
        }

        void Start()
        {            
            //_itemPrefab.gameObject.SetActive(false);
            //UpdateData();

            if (_socialNetwork.IsDataReady)
            {
                RequestSocialNetworkLeaderboard();
            }

            _socialNetwork.OnDataReady += RequestSocialNetworkLeaderboard;
        }

        protected virtual void OnEnable()
        {
            if (_ignoreFirstTimeOnEnable)
            {
                _ignoreFirstTimeOnEnable = false;
                return;
            }

            UpdateData();
        }

        public void Close()
        {
            _gui.Hide(gameObject);
        }

        void OnDestroy()
        {
            _socialNetwork.OnDataReady -= RequestSocialNetworkLeaderboard;
            _debugFacebookId?.onEndEdit.RemoveAllListeners();
            _debugPlayerName?.onEndEdit.RemoveAllListeners();
            _debugLeagueId?.onEndEdit.RemoveAllListeners();
        }

        protected virtual void UpdateData()
        {
            RequestPlayerRecord();

            RequestGlobalTop();

            RequestChunk();

            RequestSocialNetworkLeaderboard();
        }

        protected void RequestChunk()
        {
            _chunkBlocker.SetActive(true);

            var req = new LeaderboardGetRankTopChunk()
            {
                Type = "HonorPoints"
            };

            if (_leagueIndex.HasValue)
            {
                req.LeagueIndex = _leagueIndex.Value;
                req.UseLeagueIndex = true;
            }

            _requestQueue.SendRequest(
                "LeaderboardRankTopChunk",
                new Request
                {
                    LeaderboardGetRankTopChunk = req
                },
                (response, collection) =>
                {
                    SetupChunkLeaderboard(response, collection);
                    OnRequestChunkComplete();
                }
                );
        }

        protected void RequestGlobalTop()
        {
            _globalBlocker.SetActive(true);

            _requestQueue.SendRequest(
                "LeaderboardRankTop",
                new Request
                {
                    LeaderboardGetRankTop = new LeaderboardGetRankTop
                    {
                        Type = "HonorPoints",
                        Amount = 99
                    }
                },
                SetupGlobalLeaderboard
                );
        }

        protected void RequestPlayerRecord()
        {
            _requestQueue.SendRequest(
                "LeaderboardRank",
                new Request
                {
                    LeaderboardGetRank = new LeaderboardGetRank
                    {
                        Type = "HonorPoints"
                    }
                },
                (response, collection) =>
                {
                    SetupPlayerRecord(collection);              
                }
            );
        }

        protected virtual void SetupPlayerRecord(DataCollection collection)
        {
            var records = MessagePackSerializer.Deserialize<LeaderboardGetRankTopResponse>(collection.Data);
            _playerRecord = records.Records[0];
            if (_socialNetwork.Connected)
            {
                //мы хотим быть уверены, что у игрока отображается именно его имя и социальный профиль
                _playerRecord.FacebookId = _socialNetwork.PlayerSocialId;
                _playerRecord.Name = _socialNetwork.UserName;
            }
            _playerGlobalItem.Data = _playerRecord;      

            GlobalPlace = _playerRecord.Rank+1;
        }

        protected void RequestSocialNetworkLeaderboard()
        {
            if (!_socialNetwork.IsDataReady) return;

            _friendsBlocker.SetActive(true);

            Debug.Log("Request social network friends: " + JsonConvert.SerializeObject(_socialNetwork.Friends.Select(x => x.Uid).ToList()));

            _requestQueue.SendRequest("LeaderboardGetFacebookFriendsTop",
                new Request
                {
                    LeaderboardGetFacebookFriendsTop = new LeaderboardGetFacebookFriendsTop
                    {
                        Friends = _socialNetwork.Friends.Select(x => x.Uid).ToList()
                    }
                },
                SocialNetworkRecordsReceived
            );
        }

        void SocialNetworkRecordsReceived(Response response, DataCollection collection)
        {
            var resp = MessagePackSerializer.Deserialize<LeaderboardGetRankTopResponse>(collection.Data);

            Debug.Log("Facebook friends received records: " + JsonConvert.SerializeObject(resp.Records));

            if (_playerRecord != null)
            {
                resp.Records.Add(_playerRecord);
            }

            var records = resp.Records.OrderByDescending(x => x.Score).ToList();

            for (var i = 0; i < records.Count; i++)
            {
                var record = records[i];
                record.Rank = i;
            }

            SetupFriendsLeaderboard(records);

            Debug.Log("Facebook friends top processed. " + JsonConvert.SerializeObject(resp.Records));

            _friendsBlocker.SetActive(false);
        }

        protected virtual void SetupChunkLeaderboard(Response response, DataCollection collection)
        {
            var records = MessagePackSerializer.Deserialize<LeaderboardGetRankTopResponse>(collection.Data);
            SetupLeaderboardView(_playerChunkItem, _chunk, records.Records);
            _chunkBlocker.SetActive(false);
        }

        void SetupGlobalLeaderboard(Response response, DataCollection collection)
        {
            var records = MessagePackSerializer.Deserialize<LeaderboardGetRankTopResponse>(collection.Data);
            SetupLeaderboardView(_playerGlobalItem, _global, records.Records);
            _globalBlocker.SetActive(false);
        }

        void SetupFriendsLeaderboard(List<LeaderboardRecord> records)
        {
            SetupLeaderboardView(_playerFriendsItem, _friends, records);
        }

        void SetupLeaderboardView(LeaderboardScreenItem playerResult, RectTransform container,
            List<LeaderboardRecord> records)
        {
            LeaderboardRecord playerRecord = null;

            //используем по возможности ид социальной сети т.к. в лидерборде могут быть несколько рекордов одного
            //пользователя, заходившего разными аккаунтами соц. сети
            if (!string.IsNullOrEmpty(_socialNetwork.PlayerSocialId))
            {
                playerRecord = records.FirstOrDefault(x => x.FacebookId == _socialNetwork.PlayerSocialId);
            }

            //если не нашли игрока по аккаунту социальной сети, пробуем найти по игровому ид
            if (playerRecord == null)
            {
                playerRecord = records.FirstOrDefault(x => new Guid(x.UserId) == _requestQueue.PlayerId);
            }

            //в чанке и друзьях обязан быть рекорд игрока. В глобальном лидерборде это отдельно происходит (метод RequestPlayerRecord())
            if (playerRecord != null)
            {
                if (_socialNetwork.Connected)
                {
                    playerRecord.FacebookId = _socialNetwork.PlayerSocialId;
                    playerRecord.Name = _socialNetwork.UserName;
                }
                playerResult.Data = playerRecord;
            }

            //remove old elements
            for (var i = container.childCount - 1; i >= 0; i--)
            {
                if (container.GetChild(i).name.Contains("Clone"))
                {
                    Destroy(container.GetChild(i).gameObject);
                }
            }

            CreateItems(playerResult, container, records);
        }

        protected virtual void CreateItems(LeaderboardScreenItem playerResult, RectTransform container,
            List<LeaderboardRecord> records)
        { 
            //create new
            for (var i = 0; i < records.Count; i++)
            {
                var item = Instantiate(playerResult);
                item.transform.SetParent(container, false);
                item.gameObject.SetActive(true);

                if (_socialNetwork.Connected && records[i].FacebookId == _socialNetwork.PlayerSocialId)
                {
                    item.IsMyRecord = true; //в списке друзей могут быть те же ид, что и у текущего игрока если игрок входил в игру с разных фейсбук аккаунтов
                }
                else if (!_socialNetwork.Connected && new Guid(records[i].UserId) == _requestQueue.PlayerId)
                {
                    item.IsMyRecord = true;
                }
                
                item.Data = records[i];

                if(i % 2 > 0)
                    item.GetComponent<Image>().color = new Color(1,1,1,0.2f);
                else
                    item.GetComponent<Image>().color = new Color(1,1,1,0.4f);
            }
        }

        public void PostPlayerScore()
        {
            int score = 0;

            int.TryParse(_debugPlayerScore.text, out score);

            _requestQueue.SendRequest(
                "LeaderboardAdd",
                new Request
                {
                    LeaderboardRegisterRecord = new LeaderboardRegisterRecord
                    {
                        Type = "HonorPoints",
                        Score = score,
                        Name = _socialNetwork.UserName,
                        FacebookId = _socialNetwork.PlayerSocialId
                    }
                },
                (response, collection) =>
                {
                    Debug.Log("Record registred");
                    UpdateData();
                }
            );
        }
    }
}