using System;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using BackendCommon.Code.GlobalConflict;
using ServerShared.GlobalConflict;
using UnityDI;

namespace Backend.Admin
{
    public partial class GlobalConflictScheduler : System.Web.UI.Page
    {
        private const int Border = 1;
#pragma warning disable 649
        [Dependency]
        private GlobalConflictPrivateApi _api;
#pragma warning restore 649

        private GlobalConflictState _closestConflict;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (_api == null)
                ContainerHolder.Container.BuildUp(this);
            if (_api == null)
            {
                Log("ERROR: GlobalConflictPrivateApi not not registered in DI");
                return;
            }
            btnSchedule.Click += BtnScheduleOnClick;

            LoadProtos().GetAwaiter().GetResult();
            LoadSchedule().GetAwaiter().GetResult();

            if(string.IsNullOrEmpty(tbStartTime.Text))
                tbStartTime.Text = DateTime.UtcNow.ToString("u");
        }

        private void BtnScheduleOnClick(object sender, EventArgs eventArgs)
        {
            var selected = ddConflictProtoSelect.SelectedItem;
            var conflictId = selected.Text;
            if (!DateTime.TryParse(tbStartTime.Text, out var dt))
            {
                Log($"Failed to parse '{tbStartTime.Text}' as valid DateTime");
                return;
            }

            var fix = 0;
            ConflictsScheduleRc rc;
            do
            {
                var scheduledId = conflictId + dt.ToString("yyyyMMdd") + (fix++ > 0 ? $"_{fix}" : "");
                rc = _api.ConflictsSchedulePrivateApi.ScheduleConflictAsync(conflictId, scheduledId, dt).Result;
            } while (rc == ConflictsScheduleRc.AlreadyExists);

            if (rc != ConflictsScheduleRc.Success)
            {
                Log($"ERROR: Cant schedule {conflictId}. Result {rc}");
                return;
            }

            LoadSchedule().GetAwaiter().GetResult();
        }

        private void Log(string message)
        {
            lblStatus.Text += message + "<br/>";
        }

        private string GetConflictStatus(Conflict conflict)
        {
            if (conflict.Finished)
            {
                return "Finished";
            }

            if (conflict.InProgress)
            {
                var stage = conflict.GetCurrentStage().Result;
                return $"In progress (stage '{stage.info}' ends at {stage.endTime})";
            }

            return "Scheduled";
        }

        private async Task LoadProtos()
        {
            const int pageSize = 100;
            var count = await _api.ConflictPrototypesPrivateApi.ConflictPrototypesCount().ConfigureAwait(false);
            if(count < 1)
                return;
            var pages = count / pageSize + 1;
            for (var i = 0; i < pages; i++)
            {
                var protos = await _api.ConflictPrototypesPrivateApi.ListConflictPrototypes(i, pageSize).ConfigureAwait(false);
                for (var j = 0; j < protos.Length; ++j)
                {
                    var prot = protos[j];
                    ddConflictProtoSelect.Items.Add(new ListItem(prot.Id));
                }
            }
            ddConflictProtoSelect.SelectedIndex = 0;
        }

        private async Task LoadSchedule()
        {
            ScheduledConflicts.Controls.Clear();
            
            var idHead = new TableHeaderCell();
            var startHead = new TableHeaderCell();
            var endHead = new TableHeaderCell();
            var statusHead = new TableHeaderCell();
            var actionsHead = new TableHeaderCell();
            var headRow = new TableHeaderRow();
            idHead.BorderWidth = Border;
            idHead.Text = "ID";
            startHead.BorderWidth = Border;
            startHead.Text = "START";
            endHead.BorderWidth = Border;
            endHead.Text = "END";
            statusHead.BorderWidth = Border;
            statusHead.Text = "STATUS";
            actionsHead.BorderWidth = Border;
            actionsHead.Text = "ACTIONS";
            headRow.Cells.AddRange(new TableCell[] {idHead, startHead, endHead, statusHead, actionsHead});
            ScheduledConflicts.Rows.Add(headRow);

            var count = await _api.ConflictsSchedulePrivateApi.CountAsync().ConfigureAwait(false);
            const int pageSize = 100;
            var pages = count / pageSize + 1;

            if (count == 0)
            {
                var row = new TableRow();
                var cell = new TableCell();
                cell.ColumnSpan = headRow.Cells.Count;
                cell.BorderWidth = Border;
                row.Cells.Add(cell);
                cell.Text = "Database is empty";

                ScheduledConflicts.Rows.Add(row);
            }

            for (var i = 0; i < pages; i++)
            {
                var conflicts = await _api.ConflictsSchedulePrivateApi.ListConflictsAsync(i, pageSize).ConfigureAwait(false);
                for (var j = 0; j < conflicts.Length; ++j)
                {
                    var c = conflicts[j];
                    if (_closestConflict == null || _closestConflict.StartTime < c.StartTime)
                    {
                        _closestConflict = c;
                    }
                    var cO = new Conflict(c);
                    var idCell = new TableCell();
                    var startCell = new TableCell();
                    var endCell = new TableCell();
                    var statusCell = new TableCell();
                    var actionsCell = new TableCell();
                    var row = new TableHeaderRow();
                    idCell.BorderWidth = Border;
                    startCell.BorderWidth = Border;
                    statusCell.BorderWidth = Border;
                    endCell.BorderWidth = Border;
                    statusCell.BorderWidth = Border;
                    actionsCell.BorderWidth = Border;
                    row.Cells.AddRange(new[] {idCell, startCell, endCell,statusCell, actionsCell});

                    idCell.Text = c.Id;
                    startCell.Text = c.StartTime.ToString();
                    endCell.Text = c.EndTime.ToString();
                    statusCell.Text = GetConflictStatus(cO);
                    if (!cO.Started)
                    {
                        var button = new Button();
                        button.Attributes.Add("conflict_id", c.Id);
                        button.Text = "Cancel";
                        button.Click += OnCancelClick;
                        actionsCell.Controls.Add(button);
                    }
                    else
                    {
                        actionsCell.Text = "N/A";
                    }

                    ScheduledConflicts.Rows.Add(row);
                }
            }
        }

        private void OnCancelClick(object sender, EventArgs eventArgs)
        {
            var btn = sender as Button;
            var conflictId = btn.Attributes["conflict_id"];
            var r = _api.ConflictsSchedulePrivateApi.CancelScheduledConflictAsync(conflictId).Result;
            if (r != ConflictsScheduleRc.Success)
            {
                Log($"Cancel {conflictId} conflict failed. Already in progress");
            }
            LoadSchedule().GetAwaiter().GetResult();
        }
    }
}