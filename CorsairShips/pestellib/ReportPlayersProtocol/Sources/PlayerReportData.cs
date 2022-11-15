using System;
using MessagePack;

namespace ReportPlayersProtocol
{
    [MessagePackObject]
    public class PlayerReportData : BaseReportRequest
    {
        [Key(1)]
        public Guid Sender;
        [Key(2)]
        public Guid Reported;
        [Key(3)]
        public string Type;
        [Key(4)]
        public string Description;
        [Key(5)]
        public bool ReportedBySystem;
        [Key(6)]
        public DateTime Timestamp;
        [Key(7)]
        public string GamePayload;
    }
}
