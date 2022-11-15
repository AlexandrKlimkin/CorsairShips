using System;
using MessagePack;

namespace ReportPlayersProtocol
{
    [MessagePackObject]
    public class RegisterNewSession : BaseReportRequest
    {
        [Key(1)]
        public Guid PlayerId;

        [Key(2)]
        public string PlayerName;
    }
}