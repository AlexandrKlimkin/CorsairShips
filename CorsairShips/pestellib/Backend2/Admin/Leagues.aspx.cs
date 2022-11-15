using System;
using System.Globalization;
using BackendCommon;
using BackendCommon.Code;
using BackendCommon.Code.Leagues;
using ServerLib;
using BackendCommon.Code.Utils;

namespace Backend.Admin
{
    public partial class Leagues : System.Web.UI.Page
    {
        private LeagueServer _leaguesServer;
        private LeagueLeaderboardConfig _config;
        private LeagueStateCache _state;
        private LeagueDefHelper _defs;

        public Leagues()
        {
            _leaguesServer = MainHandlerBase.ServiceProvider.GetService(typeof(LeagueServer)) as LeagueServer;
            _config = new LeagueLeaderboardConfig();
            _state = MainHandlerBase.ServiceProvider.GetService(typeof(LeagueStateCache)) as LeagueStateCache; ;
            _defs = new LeagueDefHelper(BackendInitializer.LeagueDefProvider);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if(_leaguesServer == null)
                return;
            UpdateLabel();
            btnRecalcBots.Click += BtnRecalcBotsOnClick;
            lblSeasonId.Text = _state.Season.ToString();
            lblCycleTime.Text = _defs.CycleTime + " hour(s)";
            lblSeasonStart.Text = _state.SeasonStarts.ToString(CultureInfo.InvariantCulture);
            lblSeasonEnd.Text = _state.SeasonEnds.ToString(CultureInfo.InvariantCulture);
            btnEndSeason.Click += BtnEndSeasonOnClick;
            btnAddScore.Click += BtnAddScoreOnClick;
            btnRemovePlayer.Click += BtnRemovePlayerOnClick;

            var top = _leaguesServer.GlobalTop(Guid.Empty, 50).Result;
            lblGlobalTop.Text = "Global Top 50<br/>";
            foreach (var item in top.Ranks)
            {
                lblGlobalTop.Text += $"{item.PlayerId} {item.Name} {item.Score}<br/>";
            }
        }

        private void BtnRemovePlayerOnClick(object sender, EventArgs eventArgs)
        {
            Guid playerId = PlayerIdHelper.FromString(txtPlayerId.Text);
            if (playerId == Guid.Empty)
            {
                lblStatus.Text += "Wrong player id.</br>";
                return;
            }

            var pi = _leaguesServer.Remove(playerId);
            if (pi == null)
            {
                lblStatus.Text += $"Player not found.<br/>";
                return;
            }
            lblStatus.Text += $"Player {pi.Name}:{pi.PlayerId} removed.<br/>";
        }

        private void BtnAddScoreOnClick(object sender, EventArgs eventArgs)
        {
            Guid playerId = PlayerIdHelper.FromString(txtPlayerId.Text);
            if (playerId == Guid.Empty)
            {
                lblStatus.Text += "Wrong player id.</br>";
                return;
            }

            long score;
            if (!long.TryParse(txtScoreAmount.Text, out score))
            {
                lblStatus.Text += "Wrong score.</br>";
                return;
            }

            _leaguesServer.Score(playerId, score);
            _leaguesServer.SeasonController.Update();

            lblStatus.Text += $"{playerId} scored {score} pts.<br/>";
        }

        private void BtnEndSeasonOnClick(object sender, EventArgs eventArgs)
        {
            if (!AppSettings.Default.LeagueDebug)
                return;

            _state.SeasonEnds = DateTime.UtcNow - TimeSpan.FromSeconds(1);
            _leaguesServer.SeasonController.Update();
        }

        private void UpdateLabel()
        {
            lblBotsUpdateInfo.Text = 
                "Last bots update: " + 
                _leaguesServer.SeasonController.LastBotUpdateTime.ToString("u") + 
                " Next: " + 
                _leaguesServer.SeasonController.NextBotUpdateTime.ToString("u");
        }

        private void BtnRecalcBotsOnClick(object sender, EventArgs eventArgs)
        {
            _leaguesServer.SeasonController.UpdateBots(true);
            UpdateLabel();
        }
    }
}