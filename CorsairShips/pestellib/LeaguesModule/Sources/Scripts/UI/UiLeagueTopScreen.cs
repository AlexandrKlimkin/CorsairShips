using System.Net.Mime;
using PestelLib.UI;
using S;
using UnityEngine;
using UnityEngine.UI;

public class UiLeagueTopScreen : MonoBehaviour
{
    class UiLeagueTopScreenData
    {
        public LeaguePlayerInfo[] Ranks;
        public int MyRank;
    }

    private object _sync = new object();
    private UiLeagueTopScreenData _data;

    [SerializeField] private GameObject _itemContainer;
    [SerializeField] private UiLeagueTopItem _itemPrefab;
    [SerializeField] private GameObject _separatorPrefab;
    [SerializeField] private bool _showMyRank;
    [SerializeField] private Text _topName;

    private string _playerName;
    private long _score;

    public void SetTopName(string name)
    {
        _topName.text = name;
    }

    public void SetData(string playerName, long score)
    {
        _playerName = playerName;
        _score = score;
    }

    public void UpdateRanks(LeaguePlayerInfo[] ranks, int myRank)
    {
        lock (_sync)
        {
            _data = new UiLeagueTopScreenData()
            {
                Ranks = ranks,
                MyRank = myRank
            };
        }
    }

    void Update()
    {
        if (_data == null)
            return;

        lock (_sync)
        {
            Clean();
            if(_data.Ranks == null)
                return;
            var myRankShown = false;
            for (var i = 0; i < _data.Ranks.Length; ++i)
            {
                var r = _data.Ranks[i];
                var o = Instantiate(_itemPrefab, _itemContainer.transform).GetComponent<UiLeagueTopItem>();
                var rank = i + 1;
                var self = r.Name == _playerName && rank == _data.MyRank;
                if (!myRankShown && self)
                    myRankShown = true;
                o.SetData(rank, r.Name, r.Score, self);
            }

            if (!myRankShown && _data.Ranks.Length > 0 && _showMyRank)
            {
                Instantiate(_separatorPrefab, _itemContainer.transform);
                var o = Instantiate(_itemPrefab, _itemContainer.transform).GetComponent<UiLeagueTopItem>();
                o.SetData(_data.MyRank, _playerName, _score, true);
            }

            _data = null;
        }
    }

    void Clean()
    {
        foreach (Transform t in _itemContainer.transform)
        {
            Destroy(t.gameObject);
        }
    }
}
