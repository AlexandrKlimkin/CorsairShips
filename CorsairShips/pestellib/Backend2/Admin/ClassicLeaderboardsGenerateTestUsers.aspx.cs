using System;
using UnityDI;
using ClassicLeaderboards;
using S;

namespace Backend.Admin
{
    public partial class ClassicLeaderboardsGenerateTestUsers : System.Web.UI.Page
    {
#pragma warning disable 649
        [Dependency] private ILeaderboards _leaderboards;
#pragma warning restore 649

        protected void Page_Load(object sender, EventArgs e)
        {
            ContainerHolder.Container.BuildUp(this);
            generateButton.Click += GenerateButtonOnClick;
        }

        private void GenerateButtonOnClick(object sender, EventArgs eventArgs)
        {
            var rand = new Random();

            if (int.TryParse(amount.Text, out var amountInt) &&
                int.TryParse(min.Text, out var minInt) &&
                int.TryParse(max.Text, out var maxInt))
            {
                for (var i = 0; i < amountInt; i++)
                {
                    _leaderboards.RegisterRecord(new LeaderboardRegisterRecord
                    {
                        Add = false,
                        Name = "AutoRecord#" + i + " " + DateTime.UtcNow,
                        Type = type.Text,
                        Score = rand.Next(minInt, maxInt)
                    }, Guid.NewGuid());
                }

                statusLine.Text = "done!";
            }
            else
            {
                statusLine.Text = "error: check arguments";
            }
        }
    }
}