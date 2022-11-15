using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using ReportPlayers;
using UnityDI;
using System.Text.RegularExpressions;
using BackendCommon.Code;
using BackendCommon.Code.Auth;
using BackendCommon.Code.Data;
using PestelLib.ChatServer;
using BackendCommon.Code.Utils;
using ClansClientLib;

namespace Backend.CM
{
    public partial class ReportsViewer : System.Web.UI.Page
    {
        private IReportsStorage reportsStorage;
        private IGameBanStorage _banStorage;
        private IBanRequestStorage _chatBanRequestStorage;
        private ClansBackendClient _clansClient;

        public ReportsViewer()
        {
            _banStorage = MainHandlerBase.ServiceProvider.GetService(typeof(IGameBanStorage)) as IGameBanStorage;
            reportsStorage = ContainerHolder.Container.Resolve<IReportsStorage>();
            _chatBanRequestStorage = MainHandlerBase.ServiceProvider.GetService((typeof(IBanRequestStorage))) as IBanRequestStorage;
            _clansClient = MainHandlerBase.ServiceProvider.GetService(typeof(ClansBackendClient)) as ClansBackendClient;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (_chatBanRequestStorage == null)
            {
                chatBanPeriod.Enabled = false;
                chatBanSelected.Enabled = false;
            }
            if (!IsPostBack)
            {
                UpdateCheatersTop();
            }
        }

        private void UpdateCheatersTop()
        {
            RegisterAsyncTask(new PageAsyncTask(UpdateCheatersTopAsync));
        }

        private async Task UpdateCheatersTopAsync()
        {
            var cheaters = await reportsStorage.GetCheatersTopAsync(100);
            var lst = cheaters.Select(x => new ListItem($"{x.PlayerGuid.ToString()}: report rate: {x.ReportsPerSession:0.##} reports: {x.ReportsCounter} {RemoveHtmlTags(x.PlayerName)}")).ToArray();
            topCheaters.Items.Clear();
            topCheaters.Items.AddRange(lst);
        }

        private string RemoveHtmlTags(string src)
        {
            return Regex.Replace(src, @"<[^>]*>", String.Empty);
        }

        private async Task UpdateReportsListAsync()
        {
            var reportData = await reportsStorage.GetReportsByReportedAsync(SelectedCheaterGuid, 100);
            reportData = reportData.OrderBy(x => x.ReportedBySystem);
            reportsTable.Rows.Clear();

            foreach (var report in reportData)
            {
                var r = new TableRow();
                
                r.Cells.AddRange(new[]
                {
                    new TableCell { Text = report.Type },
                    new TableCell { Text = report.Description },
                    new TableCell { Text = report.ReportedBySystem ? "+" : "-" },
                    new TableCell { Text = report.GamePayload.Replace("\n", "<br/>") }
                });

                reportsTable.Rows.Add(r);
            }

            ShowShortInfo(SelectedCheaterGuid);
        }
        
        protected void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            playerId.Text = CheaterGuidFromTop.ToString();
            RegisterAsyncTask(new PageAsyncTask(UpdateReportsListAsync));
        }

        private void ShowShortInfo(Guid id)
        {
            var bytes = StateLoader.LoadBytes(MainHandlerBase.ConcreteGame, id, null, 0, out id);
            var sl = MainHandlerBase.ConcreteGame.SharedLogic(bytes, MainHandlerBase.FeaturesCollection);
            playerInfo.Text = sl.GetHumanReadableInfo(string.Empty).Replace("\n", "<br/>");
        }

        protected void forgiveSelected_OnClick(object sender, EventArgs e)
        {
            CleanHistoryForSelectedUser(true);
            statusLine.Text += string.Format("{0} was forgiven </br>", SelectedCheaterGuid);
        }

        protected void viewProfile_OnClick(object sender, EventArgs e)
        {
            if (SelectedCheaterGuid != Guid.Empty)
            {
                this.Response.Redirect("~/CM/ProfileViewer.aspx?UserId=" + SelectedCheaterGuid);
            }
            else
            {
                statusLine.Text += "select somebody to view his profile<br/>";
            }
        }

        private void CleanHistoryForSelectedUser(bool wipeHistory)
        {
            reportsStorage.RemoveReportsByReported(SelectedCheaterGuid, wipeHistory);
            UpdateCheatersTop();
        }

        protected void banSelected_OnClick(object sender, EventArgs e)
        {
            if (_banStorage.Ban(SelectedCheaterGuid, "banned from ReportsViewer"))
            {
                statusLine.Text += string.Format("{0} was banned </br>", SelectedCheaterGuid);
                CleanHistoryForSelectedUser(false);
                _clansClient?.NotifyUserBanned(SelectedCheaterGuid);
            }
        }

        private Guid CheaterGuidFromTop
        {
            get
            {
                var selected = topCheaters.SelectedValue;
                if (string.IsNullOrEmpty(selected)) return Guid.Empty;

                var guidLength = Guid.Empty.ToString().Length;
                var guidStr = selected.Substring(0, guidLength);
                return Guid.Parse(guidStr);
            }
        }

        private Guid SelectedCheaterGuid => PlayerIdHelper.FromString(playerId.Text);

        protected void loadUserReports_OnClick(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(UpdateReportsListAsync));
        }

        protected void addToWhitelist_OnClick(object sender, EventArgs e)
        {
            reportsStorage.AddUserToWhitelist(Guid.Parse(playerId.Text));
        }

        protected void removeFromWhitelist_OnClick(object sender, EventArgs e)
        {
            reportsStorage.RemoveUserFromWhitelist(Guid.Parse(playerId.Text));
        }

        protected void chatBanSelected_OnClick(object sender, EventArgs e)
        {
            if(SelectedCheaterGuid == Guid.Empty)
                return;
            var days = int.Parse(chatBanPeriod.SelectedItem.Value);
            _chatBanRequestStorage.AddBanRequest(SelectedCheaterGuid, TimeSpan.FromDays(days));
            statusLine.Text += string.Format("{0} was banned in chat  for {1} days</br>", SelectedCheaterGuid, days);
            CleanHistoryForSelectedUser(false);
        }
    }
}