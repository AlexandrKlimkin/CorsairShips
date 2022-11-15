using System.Linq;
using System.Net;
using BackendCommon.Code.Jobs;
using log4net;
using MessagePack;
using PestelLib.ServerShared;
using S;
using ServerLib.Modules;

namespace BackendCommon.Code.Modules.ClassicLeaderboards
{
    public class LeaderboardGetRankTopChunkModule : IModule
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LeaderboardGetRankTopChunkModule));

        public ServerResponse ProcessCommand(ServerRequest request)
        {
            using (var client = new WebClient())
            {
                var insts = QuartzConfig.GetAllInstances();
                var activeInstance = insts.FirstOrDefault(x => x.IsOn);

                if (activeInstance == null)
                {
                    return new ServerResponse
                    {
                        ResponseCode = ResponseCode.UNDEFINED_RESPONSE
                    };
                }

                var virtualPath = (!string.IsNullOrEmpty(activeInstance.VirtualPath) && activeInstance.VirtualPath.Length > 1)
                    ? activeInstance.VirtualPath
                    : ":50626"; //workaround for IIS express

                var uri = string.Format("http://localhost{0}/MasterSlaveConnection/LeaderboardGetRankTopChunk.ashx", virtualPath);

                //log.Debug("Resulting uri: " + uri);

                var resp = client.UploadData(uri, MessagePackSerializer.Serialize(request));

                var respSerialized = MessagePackSerializer.Deserialize<LeaderboardGetRankTopChunkResponse>(resp);

                return new ServerResponse
                {
                    ResponseCode = ResponseCode.OK,
                    Data = MessagePackSerializer.Serialize(respSerialized)
                };
            }
        }
    }
}