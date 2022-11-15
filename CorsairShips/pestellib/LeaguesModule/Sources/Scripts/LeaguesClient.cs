using System;
using System.Collections.Generic;
using MessagePack;
using PestelLib.ServerClientUtils;
using PestelLib.SharedLogic.Modules;
using S;
using UnityDI;

namespace PestelLib.Leagues
{
    public class LeaguesClient
    {
        enum RanksType
        {
            Global,
            Division,
            League
        }

        [Dependency] private RequestQueue _requestQueue;
        [Dependency] private SettingsModuleBase _settingsModule;
        public readonly int _topAmountGlobal;
        public readonly int _topAmountDivision;

        Dictionary<RanksType, Action<LeagueTopResponse>> _ranksProcessors =
            new Dictionary<RanksType, Action<LeagueTopResponse>>();

        public int GlobalRank { get; private set; }
        public int LeagueRank { get; private set; }
        public int DivisionRank { get; private set; }

        public LeaguePlayerInfo[] GlobalRanks { get; private set; }
        public LeaguePlayerInfo[] LeagueRanks { get; private set; }
        public LeaguePlayerInfo[] DivisionRanks { get; private set; }

        public event Action GlobalRanksUpdated = () => { };
        public event Action LeagueRanksUpdated = () => { };
        public event Action DivisionRanksUpdated = () => { };
        public event Action<LeaguePlayerInfo> PlayerInfoUpdated = (pi) => { };

        public bool IsInitialized { get { return PlayerId != Guid.Empty; } }
        public Guid PlayerId { get; private set; }
        public string PlayerName { get; private set; }
        public string FacebookId { get; private set; }
        public int CurrentSeason { get; private set; }
        public DateTime SeasonStart { get; private set; }
        public DateTime SeasonEnd { get; private set; }
        public LeaguePlayerInfo leaguePlayerInfo { get; private set; }

        public LeaguesClient()
        {
            ContainerHolder.Container.BuildUp(this);
            _topAmountGlobal = _settingsModule.LeaguesTopAmountGlobal;
            _topAmountDivision = _settingsModule.LeaguesTopAmountDivision;
            _ranksProcessors = new Dictionary<RanksType, Action<LeagueTopResponse>>()
            {
                {RanksType.Division, ProcessDivisionRanks},
                {RanksType.Global, ProcessGlobalRanks},
                {RanksType.League, ProcessLeagueRanks}
            };
        }

        public void Login(Guid playerId, string name, string facebookId, bool shouldClean = true)
        {
            if (shouldClean)
                Clean();

            PlayerId = playerId;
            PlayerName = name;
            FacebookId = facebookId;
            _requestQueue.SendRequest(
                "LeagueRegisterRequest",
                new Request()
                {
                    LeagueRegister = new LeagueRegisterRequest() 
                    {
                        PlayerId = PlayerId,
                        Name = PlayerName,
                        FacebookId = FacebookId
                    }
                },
                (resp, data) =>
                {
                    var r = MessagePackSerializer.Deserialize<LeagueRegisterResponse>(data.Data);
                    CurrentSeason = r.CurrentSeason;
                    SeasonStart = r.SeasonStart;
                    SeasonEnd = r.SeasonEnd;
                    leaguePlayerInfo = r.PlayerInfo;
                    PlayerInfoUpdated(r.PlayerInfo);
                },
                async:true
            );
        }

        public void Update()
        {
            UpdateDivisionRanks();
            UpdateLeagueRanks();
            UpdateGlobalRanks();
        }

        private void Clean()
        {
            GlobalRank = 0;
            LeagueRank = 0;
            GlobalRanks = null;
            LeagueRanks = null;
            DivisionRanks = null;
            GlobalRanksUpdated();
            LeagueRanksUpdated();
            DivisionRanksUpdated();
        }

        private void UpdateDivisionRanks()
        {
            _requestQueue.SendRequest(
                "LeagueDivisionRanksRequest",
                new Request()
                {
                    LeagueDivisionRanks = new LeagueDivisionRanksRequest()
                    {
                        PlayerId = PlayerId
                    }
                },
                (resp, data) => UpdateRanks_Result(data, RanksType.Division), async: true
            );
        }

        private void UpdateLeagueRanks()
        {
            _requestQueue.SendRequest(
                "LeagueTopRequest",
                new Request()
                {
                    LeagueTop = new LeagueTopRequest()
                    {
                        PlayerId = PlayerId,
                        Amount = _topAmountDivision
                    }
                },
                (resp, data) => UpdateRanks_Result(data, RanksType.League), async: true
            );
        }

        private void UpdateGlobalRanks()
        {
            _requestQueue.SendRequest(
                "LeagueGlobalTopRequest",
                new Request()
                {
                    LeagueGlobalTop = new LeagueGlobalTopRequest()
                    {
                        PlayerId = PlayerId,
                        Amount = _topAmountGlobal
                    }
                },
                (resp, data) => UpdateRanks_Result(data, RanksType.Global), async: true
            );
        }

        private void UpdateRanks_Result(DataCollection data, RanksType ranksType)
        {
            var top = MessagePackSerializer.Deserialize<LeagueTopResponse>(data.Data);
            _ranksProcessors[ranksType](top);
        }

        private void ProcessDivisionRanks(LeagueTopResponse top)
        {
            DivisionRanks = top.Ranks;
            DivisionRank = top.PlayerRank;
            DivisionRanksUpdated();
        }

        private void ProcessLeagueRanks(LeagueTopResponse top)
        {
            LeagueRank = top.PlayerRank;
            LeagueRanks = top.Ranks;
            LeagueRanksUpdated();
        }

        private void ProcessGlobalRanks(LeagueTopResponse top)
        {
            GlobalRank = top.PlayerRank;
            GlobalRanks = top.Ranks;
            GlobalRanksUpdated();
        }
    }
}
