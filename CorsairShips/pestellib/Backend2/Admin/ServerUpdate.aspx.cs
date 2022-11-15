using System;
using ServerLib;

namespace Server.Admin
{
    public partial class ServerUpdate : System.Web.UI.Page
    {
        private const string GlobalMaintenance = "GlobalMaintenanceBegin";
        private DateTime _maintenanceDateTime = DateTime.UtcNow;

        protected void Page_Load(object sender, EventArgs e)
        {
            ShowCurrentMaintenace();

            StartUpdateTimer.Click += StartUpdateTimerOnClick;
            RemoveUpdateTimer.Click += RemoveUpdateTimerOnClick;
            BlockServerImmediate.Click += BlockServerImmediateOnClick;
            btnSchedule.Click += BtnSchedule_Click;

            ddHour.TextChanged += DateChanged;
            ddMinute.TextChanged += DateChanged;
            calDate.SelectionChanged += DateChanged;

            InitTimePicker();
        }

        private void BtnSchedule_Click(object sender, EventArgs e)
        {
            RedisUtils.Cache.StringSet(GlobalMaintenance, _maintenanceDateTime.Ticks);
            ShowCurrentMaintenace();
        }

        private void ShowCurrentMaintenace()
        {
            var curMaint = RedisUtils.Cache.StringGet(GlobalMaintenance);
            if (curMaint.HasValue)
                lblCurMaintenance.Text = "Maintenance scheduled to " + new DateTime(long.Parse(curMaint.ToString())).ToString() + " UTC";
            else
                lblCurMaintenance.Text = "Maintenance not scheduled";
        }

        private void DateChanged(object sender, EventArgs e)
        {
            _maintenanceDateTime = calDate.SelectedDate.AddHours(int.Parse(ddHour.Text)).AddMinutes(int.Parse(ddMinute.Text));
            ShowSupposedMaintenance();
        }

        private void ShowSupposedMaintenance()
        {
            lblTime.Text = "Schedule maintenance to " + _maintenanceDateTime.ToString() + " UTC";
        }

        private void InitTimePicker()
        {
            if (calDate.SelectedDate > DateTime.MinValue)
                _maintenanceDateTime = calDate.SelectedDate.AddHours(int.Parse(ddHour.Text)).AddMinutes(int.Parse(ddMinute.Text));
            else
                calDate.SelectedDate = _maintenanceDateTime.Date;

            const int hourOffset = 1;
            if (ddHour.Items.Count == 0)
            {
                for (var h = 0; h < 24; h += hourOffset)
                    ddHour.Items.Add(h.ToString("00"));
            }
            const int minOffset = 10;
            if (ddMinute.Items.Count == 0)
            {
                for (var m = 0; m < 60; m += minOffset)
                    ddMinute.Items.Add(m.ToString("00"));
                ddMinute.SelectedIndex = 0;
            }
            var hourIndex = _maintenanceDateTime.Hour / hourOffset;
            ddHour.SelectedIndex = hourIndex;
            var minuteIndex = _maintenanceDateTime.Minute / minOffset;
            ddMinute.SelectedIndex = minuteIndex;

            if (lblTime.Text == "")
                ShowSupposedMaintenance();

        }

        private void BlockServerImmediateOnClick(object sender, EventArgs eventArgs)
        {
            RedisUtils.Cache.StringSet(GlobalMaintenance, DateTime.UtcNow.Ticks.ToString());
            ShowCurrentMaintenace();
        }

        private void RemoveUpdateTimerOnClick(object sender, EventArgs eventArgs)
        {
            RedisUtils.Cache.KeyDelete(GlobalMaintenance);
            ShowCurrentMaintenace();
        }

        private void StartUpdateTimerOnClick(object sender, EventArgs eventArgs)
        {
            var ts = DateTime.UtcNow + new TimeSpan(0, 0, 15, 0);
            RedisUtils.Cache.StringSet(GlobalMaintenance, ts.Ticks.ToString());
            ShowCurrentMaintenace();
        }
    }
}