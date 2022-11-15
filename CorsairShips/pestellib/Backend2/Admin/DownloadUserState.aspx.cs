using System;
using BackendCommon.Code.Data;
using BackendCommon.Code.Utils;

namespace Server.Admin
{
    public partial class DownloadUserState : System.Web.UI.Page
    {
        public static UserStorage _storage = new UserStorage();

        protected void Page_Load(object sender, EventArgs e)
        {
            buttonDownload.Click += ButtonDownloadOnClick;
        }

        private void ButtonDownloadOnClick(object sender, EventArgs eventArgs)
        {
            var playerId = PlayerIdHelper.FromString(userId.Text);
            if (playerId == Guid.Empty)
            {
                statusLine.Text = $"Can't convert '{userId.Text}' to player id.";
                return;
            }

            var userState = _storage.GetUserState(playerId);
            if (userState == null)
            {
                statusLine.Text = $"User {playerId} not found";
                return;
            }

            var bytes = (byte[])userState;

            Response.Clear();
            Response.ClearHeaders();
            Response.ClearContent();
            Response.AddHeader("Content-Disposition", "attachment; filename=rtti_routines_pb.dat");
            Response.AddHeader("Content-Length", bytes.Length.ToString());
            Response.ContentType = "text/plain";
            Response.Flush();
            Response.BinaryWrite(bytes);
            Response.End();
        }
    }
}