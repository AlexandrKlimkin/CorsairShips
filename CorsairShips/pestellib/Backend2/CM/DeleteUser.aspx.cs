using System;
using System.Threading.Tasks;
using BackendCommon.Code;
using BackendCommon.Code.Data;
using BackendCommon.Code.Leagues;
using ServerLib;
using ShortPlayerId.Storage;
using UnityDI;
using BackendCommon.Code.Utils;
using ClansClientLib;

namespace Server.Admin
{
    public partial class DeleteUser : System.Web.UI.Page
    {
        private LeagueServer _leaguesServer;
        private ClansBackendClient _clansClient;
        private ShortPlayerIdStorage _shortPlayerIdStorage;

        public DeleteUser()
        {
            _leaguesServer = MainHandlerBase.ServiceProvider.GetService(typeof(LeagueServer)) as LeagueServer;
            _clansClient = MainHandlerBase.ServiceProvider.GetService(typeof(ClansBackendClient)) as ClansBackendClient;
            _shortPlayerIdStorage = ContainerHolder.Container.Resolve<ShortPlayerIdStorage>();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            deleteButton.Click += DeleteButtonOnClick;
            
        }

        private void DeleteButtonOnClick(object sender, EventArgs eventArgs)
        {
            var id = PlayerIdHelper.FromString(guid.Text);

            if (id != Guid.Empty)
            {
                if (StateLoader.Storage.UserExist(id))
                {
                    StateLoader.Storage.Delete(id);
                    RedisUtils.Cache.SortedSetRemove("HonorPoints", id.ToString());
                    statusLine.Text = "User '" + id + "' deleted!";

                    var lastUsedDeviceId = StateLoader.GetLastUsedDeviceId(id);
                    if (lastUsedDeviceId != null)
                    {
                        StateLoader.Storage.DeleteDeviceId(lastUsedDeviceId);
                    }
                }
                else
                {
                    statusLine.Text = "User '" + id + "' not found!";
                }

                _leaguesServer?.Remove(id);
                _clansClient?.NotifyUserStateDelete(id);
            }
            else
            {
                statusLine.Text = "Cannot parse player id";
            }

            if (!string.IsNullOrEmpty(googlePlayId.Text))
            {
                if (StateLoader.Storage.DeleteFacebookId(googlePlayId.Text))
                {
                    statusLine.Text += "; google play binding removed";
                }
                else
                {
                    statusLine.Text += "; google play binding not found";
                }
            }

            if (!string.IsNullOrEmpty(deviceId.Text))
            {
                if (StateLoader.Storage.DeleteDeviceId(deviceId.Text))
                {
                    statusLine.Text += "; device id binding removed";
                }
                else
                {
                    statusLine.Text += "; device id binding not found";
                }
            }
        }
    }
}