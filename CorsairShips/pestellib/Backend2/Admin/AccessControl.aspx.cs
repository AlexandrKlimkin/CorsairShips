using System;
using System.Web.UI.WebControls;
using BackendCommon.Code;
using BackendCommon.Services;

namespace Backend.Admin
{
    public partial class AccessControl : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            LoadBackends();
        }

        private void LoadBackends()
        {
            const int Border = 1;
            var table = tblBackends;
            table.BorderWidth = Border;
            table.Rows.Clear();
            var row = new TableRow();
            var header = new[] { "Url", "Online", "Public", "Internal", "Maintenance", "Actions" };
            foreach (var h in header)
            {
                var head = new TableCell();
                head.Text = h;
                head.BorderWidth = Border;
                row.Cells.Add(head);
            }
            table.Rows.Add(row);

            var hive = MainHandlerBase.ServiceProvider.GetService(typeof(IBackendHivePrivate)) as IBackendHivePrivate;

            if (hive != null)
            {
                var rowCells = new TableCell[header.Length];
                foreach (var service in hive.AllServices())
                {
                    row = new TableRow();
                    var url = rowCells[0] = new TableCell();
                    var online = rowCells[1] = new TableCell();
                    var pub = rowCells[2] = new TableCell();
                    var intern = rowCells[3] = new TableCell();
                    var maint = rowCells[4] = new TableCell();
                    var acts = rowCells[5] = new TableCell();
                    foreach (var tableCell in rowCells)
                    {
                        tableCell.BorderWidth = Border;
                        row.Cells.Add(tableCell);
                    }

                    table.Rows.Add(row);

                    url.Text = service.ToString();
                    online.Text = service.Online.ToString();
                    pub.Text = service.Public.ToString();
                    intern.Text = service.Internal.ToString();
                    maint.Text = service.Maintenance.ToString();

                    Button btn;
                    if (service.Public)
                    {
                        btn = new Button();
                        btn.Text = "Deny Public";
                        btn.Click += (sender, args) => { hive.SetPublicAccess(service, false); Response.Redirect("AccessControl.aspx"); };
                        acts.Controls.Add(btn);
                    }
                    else
                    {
                        btn = new Button();
                        btn.Text = "Allow Public";
                        btn.Click += (sender, args) => { hive.SetPublicAccess(service, true); Response.Redirect("AccessControl.aspx"); };
                        acts.Controls.Add(btn);
                    }

                    if (service.Internal)
                    {
                        btn = new Button();
                        btn.Text = "Deny Internal";
                        btn.Click += (sender, args) => { hive.SetInternalAccess(service, false); Response.Redirect("AccessControl.aspx"); };
                        acts.Controls.Add(btn);
                    }
                    else
                    {
                        btn = new Button();
                        btn.Text = "Allow Internal";
                        btn.Click += (sender, args) => { hive.SetInternalAccess(service, true); Response.Redirect("AccessControl.aspx"); };
                        acts.Controls.Add(btn);
                    }

                    if (service.Maintenance)
                    {
                        btn = new Button();
                        btn.Text = "Stop Maintenance";
                        btn.Click += (sender, args) => { hive.SetMaintenance(service, DateTime.UtcNow); Response.Redirect("AccessControl.aspx"); };
                        acts.Controls.Add(btn);
                    }
                    else
                    {
                        btn = new Button();
                        btn.Text = "Start Maintenance";
                        btn.Click += (sender, args) => { hive.RemoveMaintenance(service); Response.Redirect("AccessControl.aspx"); };
                        acts.Controls.Add(btn);
                    }
                }
            }

            if (table.Rows.Count < 2)
            {
                row = new TableRow();
                var cell = new TableCell();
                cell.Text = "No data";
                cell.ColumnSpan = 6;
                table.Rows.Add(row);
            }
        }
    }
}