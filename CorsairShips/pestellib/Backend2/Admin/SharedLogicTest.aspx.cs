using System;
using BackendCommon.Code;
using MessagePack;
using Newtonsoft.Json;
using PestelLib.ServerShared;
using S;
using ServerLib.Modules;

namespace Backend.Admin
{
    public partial class SharedLogicTest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public static string ReplaceTab(string value)
        {
            return value.Replace("\t", "    ").Replace(" ", "&nbsp;&#8203;").Replace("\n", "<br/>");
        }

        protected void UploadButton_Click(object sender, EventArgs e)
        {
            if (FileUploadControlState.HasFile && FileUploadControlRequest.HasFile)
            {
                try
                {
                    var state = FileUploadControlState.FileBytes;
                    var cmd = FileUploadControlRequest.FileBytes;

                    var processor = new ProcessCommandsModule(MainHandlerBase.FeaturesCollection);
                    var result = processor.InternalProcessCommand(MessagePackSerializer.Deserialize<ServerRequest>(cmd),
                        false, state);



                    StatusLabel.Text = JsonConvert.SerializeObject(result);
                }
                catch (ResponseException ex)
                {
                    var info = JsonConvert.DeserializeObject<HashMismatchInfo>(ex.DebugMessage);
                    StatusLabel.Text = ReplaceTab(ex.ResponseCode +"\n" + info.Server + "\n" + info.Client);
                }
                catch (Exception ex)
                {
                    StatusLabel.Text = "Upload status: The file could not be uploaded. The following error occured: " + ex.Message;
                }
            }
            else
            {
                StatusLabel.Text = "You have to specify both files.";
            }
        }

    }
}