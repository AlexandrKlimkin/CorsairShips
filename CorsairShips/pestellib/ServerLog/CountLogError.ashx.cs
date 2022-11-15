using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using MongoDB.Driver;
using Newtonsoft.Json;
using PestelLib.ServerLogProtocol;
using ServerLog.MongoDocs;

namespace ServerLog
{
    public class CountLogError : HttpTaskAsyncHandler
    {
        public override Task ProcessRequestAsync(HttpContext context)
        {
            return Process(context.Request.InputStream);
        }

        static async Task Process(Stream s)
        {
            var reader = new StreamReader(s);
            var logMessages = await reader.ReadToEndAsync();
            var messagesGroup = JsonConvert.DeserializeObject<LogErrorsPack>(logMessages);
            await MainAsync(messagesGroup);
        }

        static async Task MainAsync(LogErrorsPack errors)
        {
            foreach (var error in errors.Errors)
            {
                try
                {
                    await Global.MongoLogErrorCounterCollection.FindOneAndUpdateAsync(
                        Builders<LogErrorCounter>.Filter.And(
                            Builders<LogErrorCounter>.Filter.Eq(message => message.Message, error),
                            Builders<LogErrorCounter>.Filter.Eq(m => m.GameName, errors.Game)
                        ),
                        Builders<LogErrorCounter>.Update.Inc(x => x.Counter, 1),
                        new FindOneAndUpdateOptions<LogErrorCounter, LogErrorCounter> {IsUpsert = true}
                    );
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + " " + e.StackTrace);
                }
            }
        }
    }
}