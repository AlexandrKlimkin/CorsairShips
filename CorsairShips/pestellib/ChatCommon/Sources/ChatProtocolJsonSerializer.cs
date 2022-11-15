using System;
using System.Text;
using log4net;
using Newtonsoft.Json;
using PestelLib.ChatCommon;

namespace ChatCommon
{
    public class ChatProtocolJsonSerializer : IChatProtocolSerializer
    {
        public byte[] Serialize(ChatProtocol message)
        {
            try
            {
                var json = JsonConvert.SerializeObject(message, Settings);
                return Encoding.UTF8.GetBytes(json);
            }
            catch (Exception e)
            {
                Log.Error($"Serialization error.", e);
            }

            return null;
        }

        public ChatProtocol Deserialize(byte[] data)
        {
            try
            {
                var json = Encoding.UTF8.GetString(data);
                return JsonConvert.DeserializeObject<ChatProtocol>(json, Settings);
            }
            catch (Exception e)
            {
                Log.Error($"Deserialization error. message='{Encoding.UTF8.GetString(data)}'.", e);
            }

            return null;
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(ChatProtocolJsonSerializer));

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings()
            {DefaultValueHandling = DefaultValueHandling.Ignore};
    }
}
