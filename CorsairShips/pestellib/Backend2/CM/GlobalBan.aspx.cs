using System;
using System.Web.UI;
using BackendCommon.Code;
using BackendCommon.Code.Auth;
using Guid = System.Guid;
using BackendCommon.Code.Utils;
using ClansClientLib;

namespace Backend.Admin
{
    public partial class GlobalBan : Page
    {
        private IGameBanStorage _banStorage;
        private ClansBackendClient _clansClient;

        public GlobalBan()
        {
            _banStorage = MainHandlerBase.ServiceProvider.GetService(typeof(IGameBanStorage)) as IGameBanStorage;
            _clansClient = MainHandlerBase.ServiceProvider.GetService(typeof(ClansBackendClient)) as ClansBackendClient;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            btnBan.Click += BtnBanOnClick;
            btnUnban.Click += BtnUnbanOnClick;

            DrawBans();
        }

        private void DrawBans()
        {
            lblGlobalBanList.Text = "Current ban list:</br>";
            foreach (var ban in _banStorage.ListBans())
            {
                lblGlobalBanList.Text += $"{ban.PlayerId} {ban.Reason}</br>";
            }
        }

        private Guid GetUserId()
        {
            var guid = PlayerIdHelper.FromString(tbUserId.Text);
            if (guid != Guid.Empty)
                return guid;
            lblStatus.Text = "Invalid user id.";
            return Guid.Empty;
        }

        private void BtnUnbanOnClick(object sender, EventArgs eventArgs)
        {
            var userId = GetUserId();
            if(userId == Guid.Empty) return;
            var r = _banStorage.Unban(userId);
            if (!r)
                lblStatus.Text = $"Cant unban user {userId}. Not banned.";
            else
                DrawBans();
        }

        private void BtnBanOnClick(object sender, EventArgs eventArgs)
        {
            var userId = GetUserId();
            if (userId == Guid.Empty) return;
            var r = _banStorage.Ban(userId, tbReason.Text);
            if (r)
            {
                _clansClient?.NotifyUserBanned(userId);
                lblStatus.Text = $"{userId} banned.";
                DrawBans();
            }
        }
    }
}