using System;
using PestelLib.ServerShared;
using PestelLib.UniversalSerializer;
using ReportPlayersProtocol;
using S;
using ServerExtension;
using log4net;

namespace ReportPlayers
{
    public class ReportPlayerModule : IExtension
    {
        private readonly IReportsStorage _reportsStorage;

        public ReportPlayerModule(IReportsStorage reportsStorage)
        {
            _reportsStorage = reportsStorage;
        }

        public ServerResponse ProcessRequest(byte[] requestData)
        {
            try
            {
                var request = Serializer.Deserialize<BaseReportRequest>(requestData);
                if (request is PlayerReportData report)
                {
                    report.Timestamp = DateTime.UtcNow;

                    _reportsStorage.RegisterReport(report);

                    return new ServerResponse { ResponseCode = ResponseCode.OK };
                }
                else if (request is RegisterNewSession r)
                {
                    _reportsStorage.IncrementSessionCounter(r.PlayerId, r.PlayerName);
                    return new ServerResponse() { ResponseCode = ResponseCode.OK };
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                return new ServerResponse { ResponseCode = ResponseCode.OK };
            }
            throw new ReportPlayerException("Unknown request type");
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(ReportPlayerModule));
    }
}
