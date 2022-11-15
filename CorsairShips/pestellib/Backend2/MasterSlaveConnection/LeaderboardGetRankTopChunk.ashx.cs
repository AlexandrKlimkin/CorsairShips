using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using BackendCommon.Code.Jobs;
using BackendCommon.Code.Modules.ClassicLeaderboards;
using ClassicLeaderboards;
using MessagePack;
using PestelLib.ServerShared;
using S;
using ServerLib;
using StackExchange.Redis;
using UnityDI;

namespace Backend.MasterSlaveConnection
{
    /// <summary>
    /// http://localhost:50626/MasterSlaveConnection/LeaderboardGetRankTopChunk.ashx
    /// </summary>
    public class LeaderboardGetRankTopChunk : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            var leaderboards = ContainerHolder.Container.Resolve<ILeaderboards>();
            var ms = new MemoryStream();
            context.Request.InputStream.CopyTo(ms);
            byte[] data = ms.ToArray();

            var request = MessagePackSerializer.Deserialize<ServerRequest>(data);

            context.Response.ContentType = "application/octet-stream";
            //context.Response.Write("Hello World");

            var cmd = request.Request.LeaderboardGetRankTopChunk;

            LeaderboardUtils.CheckLeaderboardName(cmd.Type);

            var userId = new Guid(request.Request.UserId);
            var score = (int)leaderboards.GetScore(cmd.Type, userId);

            List<PlayerLeaderboardRecord> leaderboard = null;

            if (!cmd.UseLeagueIndex)
            {
                leaderboard = RedisRebuildLeaderboardChunks.GetLeaderboard(userId, score);
            }
            else
            {
                leaderboard = RedisRebuildLeaderboardChunks.GetLeaderboardByLeagueIndex(userId, cmd.LeagueIndex);
            }

            var response = new LeaderboardGetRankTopChunkResponse();
            var chunkTop = leaderboards.GetChunk(LeaderboardUtils.CurrentSeasonId, leaderboard.Select(_ => _.userId).ToArray());

            var responseBytes = MessagePackSerializer.Serialize(response);
            //var responseBytesLenght = responseBytes.Length;
            //context.Response.Write(responseBytes);

            HttpResponse r = context.Response;
            //response.ContentType = "application/octet-stream";
            //response.AddHeader("Content-Disposition", "attachment;filename=data.bin");
            r.OutputStream.Write(responseBytes, 0, responseBytes.Length);
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}