using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using BackendCommon.Code.GlobalConflict;
using Newtonsoft.Json;
using ServerShared.GlobalConflict;
using UnityDI;

namespace Backend.Admin
{
    public partial class GlobalConflictStorage : System.Web.UI.Page
    {
        private const int Border = 1;
#pragma warning disable 649
        [Dependency]
        private GlobalConflictPrivateApi _api;
#pragma warning restore 649

        protected void Page_Load(object sender, EventArgs e)
        {
            if(_api == null)
                ContainerHolder.Container.BuildUp(this);
            if (_api == null)
            {
                Log("ERROR: GlobalConflictPrivateApi not not registered in DI");
                return;
            }
            LoadPrototypes().GetAwaiter().GetResult();
            btnUpload.Click += BtnUploadOnClick;
        }

        private void BtnUploadOnClick(object sender, EventArgs eventArgs)
        {
            if (btnUploadBrowse.PostedFile.ContentLength < 1)
            {
                Log("ERROR: no file selected");
                return;
            }

            var f = btnUploadBrowse.PostedFile;

            var validator = new GlobalConflictValidatorDummy();
            var errors = new ValidatorMessageCollection();
            using (var s = new StreamReader(f.InputStream))
            {
                var gcJson = s.ReadToEnd();
                var gc = JsonConvert.DeserializeObject<GlobalConflictState>(gcJson);
                if (!validator.IsValid(gc, errors))
                {
                    foreach (var msg in errors)
                    {
                        Log($"{msg.Level}: {msg.Message}");
                    }
                    return;
                }

                var r = _api.ConflictPrototypesPrivateApi.AddPrototype(gc).GetAwaiter().GetResult();
                Log($"Add proto '{gc.Id}' result: {r}");
            }

            LoadPrototypes().GetAwaiter().GetResult();
        }

        private void Log(string message)
        {
            lblStatus.Text += message + "<br/>";
        }

        private string GetProtoStats(GlobalConflictState globalConflict)
        {
            var stages = string.Join(",", globalConflict.Stages.Select(_ => _.Id));
            return $"stages: [{stages}], map: {globalConflict.Map?.TextureId}, nodes: {globalConflict.Map?.Nodes?.Length}";
        }

        private async Task LoadPrototypes()
        {
            StoredPrototypes.Controls.Clear();
            const int pageSize = 100;
            var count = await _api.ConflictPrototypesPrivateApi.ConflictPrototypesCount().ConfigureAwait(false);
            lblDbInfo.Text = $"{count} prototypes stored";
            var pagesCount = count / pageSize + 1;
            var idHead = new TableHeaderCell();
            var statsHead = new TableHeaderCell();
            var downloadHead = new TableHeaderCell();
            var headRow = new TableHeaderRow();
            idHead.Text = "ID";
            statsHead.Text = "STATS";
            downloadHead.Text = "ACTIONS";
            statsHead.BorderWidth = Border;
            idHead.BorderWidth = Border;
            downloadHead.BorderWidth = Border;
            headRow.Cells.Add(idHead);
            headRow.Cells.Add(statsHead);
            headRow.Cells.Add(downloadHead);
            StoredPrototypes.Rows.Add(headRow);
            if (count == 0)
            {
                var row = new TableRow();
                var cell = new TableCell();
                cell.ColumnSpan = 3;
                cell.BorderWidth = Border;
                row.Cells.Add(cell);
                cell.Text = "Database is empty";
                StoredPrototypes.Rows.Add(row);
            }

            for (var i = 0; i < pagesCount; ++i)
            {
                var protos = await _api.ConflictPrototypesPrivateApi.ListConflictPrototypes(i, pageSize).ConfigureAwait(false);
                for (var j = 0; j < protos.Length; j++)
                {
                    var gc = protos[j];
                    var row = new TableRow();
                    var idCell = new TableCell();
                    var statsCell = new TableCell();
                    var downloadCell = new TableCell();

                    idCell.BorderWidth = Border;
                    statsCell.BorderWidth = Border;
                    downloadCell.BorderWidth = Border;

                    row.Cells.Add(idCell);
                    row.Cells.Add(statsCell);
                    row.Cells.Add(downloadCell);
                    var btnDownload = new Button();

                    downloadCell.Controls.Add(btnDownload);
                    btnDownload.Attributes.Add("conflict_id", gc.Id);
                    btnDownload.Text = "Download";
                    btnDownload.Click += BtnDownloadOnClick;
                    idCell.Text = gc.Id;
                    statsCell.Text = GetProtoStats(gc);

                    StoredPrototypes.Rows.Add(row);
                }
            }
        }

        private void BtnDownloadOnClick(object sender, EventArgs eventArgs)
        {
            var btn = sender as Button;
            var id = btn.Attributes["conflict_id"];
            var gc = _api.ConflictPrototypesPrivateApi.GetConflictPrototype(id).Result;
            var gcJson = JsonConvert.SerializeObject(gc);
            var data = Encoding.UTF8.GetBytes(gcJson);

            Response.Clear();
            Response.ClearHeaders();
            Response.ClearContent();
            Response.AddHeader("Content-Disposition", $"attachment; filename={gc.Id}.json");
            Response.AddHeader("Content-Length", data.Length.ToString());
            Response.ContentType = "application/json";
            Response.Flush();
            Response.BinaryWrite(data);
            Response.End();
        }
    }
}