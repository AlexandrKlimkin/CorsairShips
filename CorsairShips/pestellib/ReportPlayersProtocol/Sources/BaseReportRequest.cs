using MessagePack;

namespace ReportPlayersProtocol
{
    [Union(0, typeof(PlayerReportData))]
    [Union(1, typeof(RegisterNewSession))]
    [MessagePackObject]
    public abstract class BaseReportRequest
    {
        
    }
}