using System;
using System.Collections.Generic;

namespace PestelLib.ServerLogProtocol
{
    // нельзя обфусцировать используется как JSON
    [System.Reflection.Obfuscation(Exclude = true)]
    public enum Platform
    {
        Android,
        iOS,
        Editor,
        Standalone,
        Other
    }

    // нельзя обфусцировать используется как JSON
    [System.Reflection.Obfuscation(Exclude = true)]
    public enum LogLevel
    {
        Log,
        Debug,
        Error,
        Exception
    }

    // нельзя обфусцировать используется как JSON
    [System.Reflection.Obfuscation(Exclude = true)]
    public struct LogMessage
    {
        //[BsonId]
        //public ObjectId Id;
        public DateTime Time;
        public string Message;
        public string Tag;
        public LogLevel Type;
        public Guid PlayerId;
        public string PlayerIdString;
        public string Game;
        public string BuildVersion;
        public Platform Platform;
    }

    // нельзя обфусцировать используется как JSON
    [System.Reflection.Obfuscation(Exclude = true)]
    public class LogMessagesGroup
    {
        public List<LogMessage> Messages = new List<LogMessage>();
    }

}
