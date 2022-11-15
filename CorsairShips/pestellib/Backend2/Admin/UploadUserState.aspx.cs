using System;
using BackendCommon.Code.Data;
using BackendCommon.Code.Utils;

namespace Backend.Admin
{
    public partial class UploadUserState : System.Web.UI.Page
    {
        public static UserStorage _storage = new UserStorage();

        protected void Page_Load(object sender, EventArgs e)
        {
            buttonUpload.Click += ButtonUploadOnClick;
        }

        private void ButtonUploadOnClick(object sender, EventArgs eventArgs)
        {
            var playerId = PlayerIdHelper.FromString(userId.Text);
            if (playerId == Guid.Empty)
            {
                statusLine.Text = $"Can't convert '{userId.Text}' to player id.";
                return;
            }

            if (!FileUploadForm.HasFile)
            {
                statusLine.Text = $"Please select file";
                return;
            }

            var fileBytes = FileUploadForm.FileBytes;
            var deviceId = StateLoader.GetLastUsedDeviceId(playerId);
            StateLoader.Save(playerId, fileBytes, deviceId);
            _storage.SaveRawState(playerId, fileBytes);

            statusLine.Text = $"User {playerId} saved, new user profile size: " + fileBytes.Length;
        }
    }
}