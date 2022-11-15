using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BackendCommon.Code.Jobs;
using ServerLib;

namespace Server.Admin
{
    public partial class QuartzControl : System.Web.UI.Page
    {
        Dictionary<string, bool> _ids = new Dictionary<string, bool>();
        protected void Page_Load(object sender, EventArgs e)
        {
            btnReload.Click += BtnReload_Click;
            btnSave.Click += BtnSave_Click;
            LoadQuartzInstances();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (QuartzInstances.Controls.Count < 1)
                return;

            foreach (var id in _ids)
            {
                var control = (CheckBox)QuartzInstances.FindControl(id.Key);
                if (id.Value != control.Checked)
                {
                    QuartzConfig.SetQuartzState(id.Key, control.Checked);
                }
            }
            btnSave.Enabled = false;
        }

        private void BtnReload_Click(object sender, EventArgs e)
        {
            LoadQuartzInstances();
        }

        private void LoadQuartzInstances()
        {
            QuartzInstances.Controls.Clear();
            _ids.Clear();
            var insts = QuartzConfig.GetAllInstances();
            foreach (var inst in insts)
            {
                var row = new TableRow();
                var idCell = new TableCell();
                var pathCell = new TableCell();
                var isOnCell = new TableCell();
                QuartzInstances.Controls.Add(row);

                row.Cells.Add(idCell);
                row.Cells.Add(pathCell);
                row.Cells.Add(isOnCell);

                var idLbl = new Label();
                idLbl.Text = inst.Id;
                idCell.Controls.Add(idLbl);

                var pathLbl = new Label();
                pathLbl.Text = inst.AppPath;
                pathCell.Controls.Add(pathLbl);

                var isOnCheckbox = new CheckBox();
                isOnCheckbox.Checked = inst.IsOn;
                isOnCheckbox.ID = inst.Id;
                isOnCheckbox.AutoPostBack = true;
                isOnCheckbox.CheckedChanged += IsOnCheckbox_CheckedChanged;
                isOnCell.Controls.Add(isOnCheckbox);

                _ids[inst.Id] = inst.IsOn;
            }
        }

        private void CheckSaveBtn()
        {
            foreach (var id in _ids)
            {
                var control = (CheckBox)QuartzInstances.FindControl(id.Key);
                if (control.Checked != id.Value)
                {
                    btnSave.Enabled = true;
                    return;
                }
            }
            btnSave.Enabled = false;
        }

        private void IsOnCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            CheckSaveBtn();
        }
    }
}