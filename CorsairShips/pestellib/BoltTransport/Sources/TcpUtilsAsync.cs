using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MasterServerProtocol;
using Newtonsoft.Json;
using Overby.Extensions.AsyncBinaryReaderWriter;

namespace BoltMasterServer
{
    public static class TcpUtilsAsync
    {
        public static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        };

        private static async Task<string> ReadUTF8StringAsync(this Stream stream, CancellationToken cancellationToken = default)
        {
            using (AsyncBinaryReader br = new AsyncBinaryReader(stream, Encoding.UTF8, true))
            {
                var result = await br.ReadStringAsync(cancellationToken);
#if UNITY_2017_1_OR_NEWER
                result = result.Replace("BoltProtocol", "Assembly-CSharp-firstpass");
#endif
                return result;
            }
        }

        /// <summary>
        /// Считывание сообщения, записанного ранее в <paramref name="stream"/>  через WriteMessageAsync.
        /// </summary>
        ///
        /// <param name="stream">               stream, обычно NetworkStream, если не считать тесты. </param>
        /// <param name="cancellationToken">    (Optional) A token that allows processing to be
        ///                                     cancelled. </param>
        ///
        /// <returns>   An asynchronous result that yields the read message. </returns>
        public static async Task<Message> ReadMessageAsync(this Stream stream, CancellationToken cancellationToken = default)
        {
            var messageStr = await stream.ReadUTF8StringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<Message>(messageStr, jsonSerializerSettings);
        }

        private static async Task WriteUTF8StringAsync(this Stream stream, string str, CancellationToken cancellationToken = default)
        {
            using (AsyncBinaryWriter bw = new AsyncBinaryWriter(stream, Encoding.UTF8, true))
            {
#if UNITY_2017_1_OR_NEWER
                str = str.Replace("Assembly-CSharp-firstpass", "BoltProtocol");
#endif
                await bw.WriteAsync(str, cancellationToken);
            }
        }

        /// <summary>
        /// Запись подкласса Message в виде JSON в переданный <paramref name="stream"/>  в виде
        /// последовательности 7-bit encoded string length и самой строки в UTF-8. В джейсонах
        /// сохраняется информация о типе. В юнити в другую сборку попадают классы протокола, поэтому
        /// приходится их подменять при приёме-отправке.
        /// </summary>
        ///
        /// <param name="stream">               stream, обычно NetworkStream, если не считать тесты. </param>
        /// <param name="message">              Один из подклассов Messsage. </param>
        /// <param name="cancellationToken">    (Optional) A token that allows processing to be
        ///                                     cancelled. </param>
        ///
        /// <returns>   An asynchronous result. </returns>
        public static async Task WriteMessageAsync(this Stream stream, Message message, CancellationToken cancellationToken = default)
        {
            var serializedMessage = JsonConvert.SerializeObject(message, Formatting.Indented, jsonSerializerSettings);
            await stream.WriteUTF8StringAsync(serializedMessage, cancellationToken);
        }
    }
}
