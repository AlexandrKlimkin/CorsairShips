using System.IO;
using System.Text;
using MasterServerProtocol;
using Newtonsoft.Json;
using BoltMasterServer;

namespace PhotonBoltNetworkUtils
{
    public static class TcpUtils
    {
        private static string ReadUTF8String(this Stream stream)
        {
            using (BinaryReader br = new BinaryReader(stream, Encoding.UTF8, true))
            {
                var result = br.ReadString();
#if UNITY_2017_1_OR_NEWER
                result = result.Replace("BoltProtocol", "Assembly-CSharp-firstpass");
#endif
                return result;
            } 
        }

        public static Message ReadMessage(this Stream stream)
        {
            var messageStr = stream.ReadUTF8String();
            return JsonConvert.DeserializeObject<Message>(messageStr, TcpUtilsAsync.jsonSerializerSettings);
        }
        

        private static void WriteUTF8String(this Stream stream, string str)
        {
#if UNITY_2017_1_OR_NEWER
            str = str.Replace("Assembly-CSharp-firstpass", "BoltProtocol");
#endif
            using (BinaryWriter bw = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                bw.Write(str);
            }
        }

        public static void WriteMessage(this Stream stream, Message message)
        {
            var serializedMessage = JsonConvert.SerializeObject(message, Formatting.Indented, TcpUtilsAsync.jsonSerializerSettings);
            stream.WriteUTF8String(serializedMessage);
        }
    }
}
