using System;
using PestelLib.Leagues;
using PestelLib.Localization;
using PestelLib.SharedLogicBase;
using S;
using UnityEngine;
using UnityDI;

public class UiLeagueScreen : MonoBehaviour
{
    [SerializeField]
    private Guid _playerId;
    [SerializeField]
    private string _playerName;
    [SerializeField]
    private string _facebookId;
    [SerializeField]
    private int _topAmount;

    private int _pendingOps;

    [Header("Tops")]
    [SerializeField]
    protected UiLeagueTopScreen _globalTop;
    [SerializeField]
    protected UiLeagueTopScreen _leagueTop;
    [SerializeField]
    protected UiLeagueTopScreen _divisionTop;

    [Dependency] private LeaguesClient _leaguesClient;
    [Dependency] protected ILocalization _localization;
    [Dependency] private ILeaguesConcreteGameInterface _api;

    void Start()
    {
        ContainerHolder.Container.BuildUp(this);

        _leaguesClient.GlobalRanksUpdated += OnGlobalRanksUpdated;
        _leaguesClient.LeagueRanksUpdated += OnLeagueRanksUpdated;
        _leaguesClient.DivisionRanksUpdated += OnDivisionRanksUpdated;
        _leaguesClient.PlayerInfoUpdated += OnPlayerInfoUpdated;
        ++_pendingOps;
        _leaguesClient.Login(_playerId, _playerName, _facebookId);
    }

    public void SetData(Guid playerId, string playerName, string facebookId)
    {
        _playerId = playerId;
        _playerName = playerName;
        _facebookId = facebookId;
    }

    public void Relogin()
    {
        if (_pendingOps == 0 && _playerId != Guid.Empty)
        {
            ++_pendingOps;
            _leaguesClient.Login(_playerId, _playerName, _facebookId);
        }
    }

    public void UpdateData()
    {
        if(_leaguesClient == null)
            return;
        _leaguesClient.Update();
    }

    private void OnPlayerInfoUpdated(LeaguePlayerInfo leaguePlayerInfo)
    {
        --_pendingOps;
        _divisionTop.SetTopName(_localization.Get("DivisionTopName"));
        _divisionTop.SetData(_playerName, leaguePlayerInfo.Score);
        _leagueTop.SetTopName(_api.GetLeagueName(leaguePlayerInfo.LeagueLevel) + " Top " + _topAmount);
        _leagueTop.SetData(_playerName, leaguePlayerInfo.Score);
        _globalTop.SetTopName(_localization.Get("GlobalTopName") + " Top " + _topAmount);
        _globalTop.SetData(_playerName, leaguePlayerInfo.Score);
        for (var i = 0; i < leaguePlayerInfo.UnclaimedRewards.Count; ++i)
        {
            _api.ClaimRewards(leaguePlayerInfo.UnclaimedRewards[i]);
        }
        _leaguesClient.Update();
    }

    void RemoveLeaguesClient()
    {
        _leaguesClient.GlobalRanksUpdated -= OnGlobalRanksUpdated;
        _leaguesClient.LeagueRanksUpdated -= OnLeagueRanksUpdated;
        _leaguesClient.DivisionRanksUpdated -= OnDivisionRanksUpdated;
        _leaguesClient.PlayerInfoUpdated -= OnPlayerInfoUpdated;
        _leaguesClient = null;
    }

    private void OnDivisionRanksUpdated()
    {
        _divisionTop.UpdateRanks(_leaguesClient.DivisionRanks, _leaguesClient.DivisionRank);
    }

    private void OnLeagueRanksUpdated()
    {
        _leagueTop.UpdateRanks(_leaguesClient.LeagueRanks, _leaguesClient.LeagueRank);
    }

    private void OnGlobalRanksUpdated()
    {
        _globalTop.UpdateRanks(_leaguesClient.GlobalRanks, _leaguesClient.GlobalRank);
    }

    protected virtual void Update()
    {
        if (_leaguesClient == null)
            return;

        if (_playerId == Guid.Empty)
        {
            if (_pendingOps == 0)
                RemoveLeaguesClient();
        }
        else if (_leaguesClient.IsInitialized && (_playerId != _leaguesClient.PlayerId ||
                                                  _facebookId != _leaguesClient.FacebookId ||
                                                  _playerName != _leaguesClient.PlayerName))
        {
            if (_pendingOps == 0)
            {
                ++_pendingOps;
                _leaguesClient.Login(_playerId, _playerName, _facebookId);
            }
        }
    }
}
