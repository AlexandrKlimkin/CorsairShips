using System;
using ReportPlayers;
using ReportPlayersProtocol;
using UnityDI;

namespace Backend.Admin.Test
{
    public partial class ReportsTestGenerator : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void generateButton_OnClick(object sender, EventArgs e)
        {
            var storage = ContainerHolder.Container.Resolve<IReportsStorage>();

            var maxRep = Int32.Parse(this.maxReports.Text);
            var minRep = Int32.Parse(this.minReports.Text);
            var maxSes = Int32.Parse(this.maxSession.Text);
            var minSes = Int32.Parse(this.minSession.Text);
            var reported = Int32.Parse(this.reportedPlayers.Text);
            var percent = Int32.Parse(this.systemReportsPercent.Text);

            var rnd = new Random((int)DateTime.UtcNow.Ticks);

            var reportSerial = 0;

            for (var reportedPlayerIndex = 0; reportedPlayerIndex < reported; reportedPlayerIndex++)
            {
                var reportCount = rnd.Next(minRep, maxRep);
                var reportedGuid = Guid.NewGuid();

                var sessions = rnd.Next(minSes, maxSes);
                for (var i = 0; i < sessions; i++)
                {
                    storage.IncrementSessionCounter(reportedGuid, "testPlrName '" + reportedPlayerIndex + "'");
                }

                for (var reportIndex = 0; reportIndex < reportCount; reportIndex++)
                {
                    var bySystem = (float)rnd.NextDouble() < (percent / 100f);

                    storage.RegisterReport(new PlayerReportData
                    {
                        Type = "TestType",
                        Description = "Test description #" + (++reportSerial),
                        ReportedBySystem = bySystem,
                        Timestamp = DateTime.UtcNow,
                        Reported = reportedGuid,
                        Sender = Guid.NewGuid(),
                        GamePayload = "{someField:true\nanotherField:false\ntestInt:555}"
                    });
                }
            }
        }
    }
}