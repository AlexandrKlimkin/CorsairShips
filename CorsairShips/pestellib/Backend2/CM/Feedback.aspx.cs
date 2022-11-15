using System;
using System.Text;
using PestelLib.ServerShared;
using S;
using PestelLib.ServerCommon.Db;
using BackendCommon.Code;

namespace Backend.CM
{
    public partial class Feedback : System.Web.UI.Page
    {
        private long fromTs;
        private long toTs;
#if DEBUG
        public const bool ShowDebug = true;
#else
        public const bool ShowDebug = false;
#endif
        private IFeedbackStorage _feedbackStorage;

        public Feedback()
        {
            _feedbackStorage = MainHandlerBase.ServiceProvider.GetService(typeof(IFeedbackStorage)) as IFeedbackStorage;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var fromParam = Request.Params["from"];
            var toParam = Request.Params["to"];

            if (long.TryParse(fromParam, out fromTs) && long.TryParse(toParam, out toTs))
            {
                DownloadFeedback(fromTs, toTs);
            }

            btnSendFeedback.Click += BtnSendFeedbackOnClick;

            UpdateTotal();

            if(string.IsNullOrEmpty(txtDateFrom.Text))
                txtDateFrom.Text = TimeUtils.ConvertFromUnixTimestamp(0).ToString("u");
            if(string.IsNullOrEmpty(txtDateTo.Text))
                txtDateTo.Text = DateTime.UtcNow.ToString("u");

            btnDownload.Click += BtnDownloadOnClick;
        }

        private void BtnDownloadOnClick(object sender, EventArgs eventArgs)
        {
            if (!DateTime.TryParse(txtDateFrom.Text, out var d))
            {
                AddStatus("ERROR: 'Date from' has bad format");
                return;
            }
            fromTs = TimeUtils.ConvertToUnixTimestamp(d);

            if (!DateTime.TryParse(txtDateTo.Text, out d))
            {
                AddStatus("ERROR: 'Date to' has bad format");
                return;
            }
            toTs = TimeUtils.ConvertToUnixTimestamp(d);

            DownloadFeedback(fromTs, toTs);
        }

        private void AddStatus(string message)
        {
            lblStatus.Text += message + "<br/>";
        }

        private void UpdateTotal()
        {
            var total = _feedbackStorage.Count();
            var dt = DateTime.UtcNow;
            var forDay = _feedbackStorage.Count(dt, dt.Subtract(TimeSpan.FromDays(1)));
            var forWeek = _feedbackStorage.Count(dt, dt.Subtract(TimeSpan.FromDays(7)));
            var forMonth = _feedbackStorage.Count(dt, dt.Subtract(TimeSpan.FromDays(30)));
            lblFeedbackDbStatus.Text = $"Feedback database contains {total} record(s). Set specific date range for feedback to download: ";
            AddStatus($"{forDay} record(s) last day");
            AddStatus($"{forWeek} record(s) last week");
            AddStatus($"{forMonth} record(s) last month");
        }

        private void DownloadFeedback(long fromTs, long toTs)
        {
            Response.ContentType = "text/csv";
            Response.AppendHeader("Content-Disposition", "attachment; filename=feedback.csv");
            var output = "date,email,caption,description\n";
            foreach (var item in _feedbackStorage.GetRange(TimeUtils.ConvertFromUnixTimestamp(fromTs), TimeUtils.ConvertFromUnixTimestamp(toTs)))
            {
                var time = item.RegDate;
                var data = item.Feedback;
                data.email = data.email.Replace("\"", "\"\"");
                if (data.email.Contains("\"") || data.email.Contains(","))
                    data.email = $"\"{data.email}\"";
                data.caption = data.caption.Replace("\"", "\"\"");
                if (data.caption.Contains("\"") || data.caption.Contains(","))
                    data.caption = $"\"{data.caption}\"";
                data.description = data.description.Replace("\"", "\"\"");
                if (data.description.Contains("\"") || data.description.Contains(","))
                    data.description = $"\"{data.description}\"";
                output += $"{time},{data.email},{data.caption},{data.description}\n";
            }
            var rawData = Encoding.UTF8.GetBytes(output);
            Response.OutputStream.Write(rawData, 0, rawData.Length);
            Response.End();
        }

        private void BtnSendFeedbackOnClick(object sender, EventArgs eventArgs)
        {
            var feedback = new SendFeedback() { email = txtNewFeedbackEmail.Text, caption = txtNewFeedbackCaption.Text, description = txtNewFeedbackDescription.Text };
            _feedbackStorage.Save(feedback, DateTime.UtcNow);
            AddStatus("Feedback sent");
            UpdateTotal();
        }
    }
}