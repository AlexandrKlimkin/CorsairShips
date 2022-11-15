using System;
using BackendCommon.Code;
using BackendCommon.Code.Leagues;
using BackendCommon.Code.Utils;

namespace Backend.Admin
{
    public partial class LeaguesBan : System.Web.UI.Page
    {
        private LeagueServer _leaguesServer;

        public LeaguesBan()
        {
            _leaguesServer = MainHandlerBase.ServiceProvider.GetService(typeof(LeagueServer)) as LeagueServer;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            btnBan.Click += BtnBanOnClick;
            btnBanById.Click += BtnBanByIdOnClick;
        }

        private void BtnBanByIdOnClick(object sender, EventArgs eventArgs)
        {
            var guid = PlayerIdHelper.FromString(txtPlayerId.Text);
            if (guid == Guid.Empty)
            {
                lblStatus.Text = "Bad player id format.";
                return;
            }

            var result = _leaguesServer.BanById(guid);
            if (result == null)
            {
                lblStatus.Text = $"Player with id {guid} not found.";
            }
            else
            {
                lblStatus.Text = $"Player {result.Name} banned.";
            }
        }

        private void BtnBanOnClick(object sender, EventArgs eventArgs)
        {
            if (!int.TryParse(txtScore.Text, out var score))
            {
                lblStatus.Text = $"Bad score format.";
                return;
            }

            var result = _leaguesServer.BanByScore(score);
            if (result == null)
            {
                lblStatus.Text = $"Player with score {score} not found.";
            }
            else
            {
                lblStatus.Text = $"Player {result.Name} banned.";
            }
        }
    }
}