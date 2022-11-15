using System;
using Backend2.Admin;
using MessagePack;
using S;
using BackendCommon.Code.Utils;

namespace Backend.Admin
{
    public partial class ReplaceWithRandomUser : System.Web.UI.Page
    {
        private Random _random = new Random();
        protected void Page_Load(object sender, EventArgs e)
        {
            var count = ProfileViewer._storage.CountRecentUsers();
            DbState.Text = $"DB contains {count} recent users";
            ReplaceButton.Click += ReplaceButtonOnClick;
        }

        private void ReplaceButtonOnClick(object sender, EventArgs eventArgs)
        {
            var myPlayer = PlayerIdHelper.FromString(PlayerIdBox.Text);
            if (!ProfileViewer._storage.UserExist(myPlayer))
            {
                Status.Text += $"{myPlayer} not found<br/>";
                return;
            }

            var minSize = int.Parse(MinStateSize.Text) * 1024;

            var sourcePlayer = GetRandomUser(minSize);
            if (sourcePlayer == Guid.Empty)
            {
                Status.Text += $"Cant find random player<br/>";
                return;
            }

            var rawData = ProfileViewer._storage.GetUserState(sourcePlayer);
            if (rawData == null)
            {
                Status.Text += $"Cant load state for player {sourcePlayer}<br/>";
                return;
            }

            var state = MessagePackSerializer.Deserialize<UserProfile>(rawData);
            state.UserId = myPlayer.ToByteArray();
            state.SharedLogicVersion = 0;
            rawData = MessagePackSerializer.Serialize(state);
            ProfileViewer._storage.SaveRawState(myPlayer, rawData);
            Status.Text += $"{myPlayer} state replaced with user {sourcePlayer}<br/>";
        }

        private Guid GetRandomUser(int minSize)
        {
            var keys = ProfileViewer._storage.GetRecentUsers(10000, minSize);
            if(keys.Length == 0)
                return Guid.Empty;
            var idx = _random.Next(keys.Length);
            return keys[idx];
        }
    }
}