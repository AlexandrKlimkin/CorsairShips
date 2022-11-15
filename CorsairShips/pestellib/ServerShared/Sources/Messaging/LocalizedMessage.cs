using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using S;

namespace ServerShared.Messaging
{
    // нельзя обфусцировать используется как JSON
    [System.Reflection.Obfuscation(Exclude = true)]
    public class LocalizedMessageText
    {
        public UnitySystemLanguage Locale;
        public string Title;
        public string Text;
    }
    // нельзя обфусцировать используется как JSON
    [System.Reflection.Obfuscation(Exclude = true)]
    public class LocalizedMessageReward
    {
        public string IdType;
        public string Id;
        public int Amount;
    }

    // нельзя обфусцировать используется как JSON
    [System.Reflection.Obfuscation(Exclude = true)]
    public class LocalizedMessageCustomReward
    {
        public string Id;
        public int Amount;
    }

    // нельзя обфусцировать используется как JSON
    [System.Reflection.Obfuscation(Exclude = true)]
    public class LocalizedMessage
    {
        public long Id;
        /// <summary>
        /// If value is in future when hide message from user.
        /// </summary>
        public DateTime DeliveryDate;

        public bool WelcomeMessage;

        public LocalizedMessageText[] Message;

        public string[] ChestRewards;
        public LocalizedMessageCustomReward[] CustomRewards;

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is LocalizedMessage m))
                return false;
            return m.Id == Id;
        }
    }

    public static class LocalizedMessageHelper
    {
        public static ServerMessage ToServerMessage(LocalizedMessage message)
        {
            var serverMessage = new ServerMessage();
            serverMessage.MessageType = typeof(LocalizedMessage).Name;
            var json = JsonConvert.SerializeObject(message);
            serverMessage.Data = Encoding.UTF8.GetBytes(json);
            return serverMessage;
        }

        public static LocalizedMessage FromServerMessage(ServerMessage message)
        {
            var jsonData = Encoding.UTF8.GetString(message.Data);
            var locMessage = JsonConvert.DeserializeObject<LocalizedMessage>(jsonData);
            return locMessage;
        }
    }
}
