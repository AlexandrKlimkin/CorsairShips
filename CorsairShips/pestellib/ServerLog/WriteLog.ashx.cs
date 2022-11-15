using System.IO;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using PestelLib.ServerLogProtocol;

namespace ServerLog
{
    public class WriteLog : HttpTaskAsyncHandler
    {
        public override Task ProcessRequestAsync(HttpContext context)
        {
            return Process(context.Request.InputStream);
        }

        static async Task Process(Stream s)
        {
            var reader = new StreamReader(s);
            var logMessages = await reader.ReadToEndAsync();
            var messages = JsonConvert.DeserializeObject<LogMessagesGroup>(logMessages);
            await MainAsync(messages);
        }

        static async Task MainAsync(LogMessagesGroup messages)
        {
            await Global.MongoLogCollection.InsertManyAsync(messages.Messages);
        }
    }
}