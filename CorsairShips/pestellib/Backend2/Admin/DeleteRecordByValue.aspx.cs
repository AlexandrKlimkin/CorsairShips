using System;
using ServerLib;
using StackExchange.Redis;

namespace Server.Admin
{
    public partial class DeleteRecordByValue : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            KillButton.Click += KillButtonOnClick;
        }

        private void KillButtonOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                double score;
                if (double.TryParse(this.Score.Text, out score))
                {
                    var usersToDelete = RedisUtils.Cache.SortedSetRangeByScore(this.TableName.Text, score - 1,
                        score + 1, Exclude.None, Order.Ascending, 0, 50);

                    if (usersToDelete == null) return;

                    OperationResult.Text = "";

                    foreach (var redisValue in usersToDelete)
                    {
                        if (redisValue.HasValue)
                        {
                            RedisUtils.Cache.HashSet("BanList", redisValue, "score table ban");
                            OperationResult.Text += "Banned: " + redisValue + "\n";
                        }
                    }

                    RedisUtils.Cache.SortedSetRemoveRangeByScore(this.TableName.Text, score - 1,
                        score + 1);

                    OperationResult.Text += "Done!";
                }
            }
            catch (Exception e)
            {
                OperationResult.Text = "Failed: " + e.Message;
            }
        }
    }
}