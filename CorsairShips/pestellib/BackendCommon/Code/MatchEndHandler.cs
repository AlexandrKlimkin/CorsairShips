using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using Newtonsoft.Json;
using PestelLib.ServerCommon.Db;
using S;
using ServerLib.Modules.ServerMessages;
using ServerShared.Sources;

namespace BackendCommon.Code
{
    public class MatchEndHandler : IRequestHandler
    {
        public Task<byte[]> Process(byte[] data, RequestContext ctx)
        {
            var dataStr = Encoding.UTF8.GetString(data);
            var matchEndData = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(dataStr);
            var matchId = matchEndData["match"][0];

            var extra = new Dictionary<string, string>();
            if (matchEndData.TryGetValue("extra", out var extraR))
            {
                extra = extraR.Roughen();
            }

            var meW = new MatchEnd()
            {
                MatchId = matchId,
                Result = MatchResult.Win,
                Extra = extra
            };
            var meL = new MatchEnd()
            {
                MatchId = matchId,
                Result = MatchResult.Lose,
                Extra = extra
            };
            var meD = new MatchEnd()
            {
                MatchId = matchId,
                Result = MatchResult.Draw,
                Extra = extra
            };

            ApiFactory.GetMatchInfoApi().SetMatchEnd(matchId, matchEndData["winners"], matchEndData["losers"], matchEndData["draw"], extra);

            var meWData = MessagePackSerializer.Serialize(meW);
            var meLData = MessagePackSerializer.Serialize(meL);
            var meDData = MessagePackSerializer.Serialize(meD);

            foreach (var s in matchEndData["winners"])
            {
                var userId = Guid.Parse((string)s);
                ServerMessageUtils.SendMessage(new ServerMessage()
                {
                    MessageType = typeof(MatchEnd).Name,
                    Data = meWData
                }, userId);
            }

            foreach (var s in matchEndData["losers"])
            {
                var userId = Guid.Parse((string)s);
                ServerMessageUtils.SendMessage(new ServerMessage()
                {
                    MessageType = typeof(MatchEnd).Name,
                    Data = meLData
                }, userId);
            }

            foreach (var s in matchEndData["draw"])
            {
                var userId = Guid.Parse((string)s);
                ServerMessageUtils.SendMessage(new ServerMessage()
                {
                    MessageType = typeof(MatchEnd).Name,
                    Data = meDData
                }, userId);
            }

            return Task.FromResult(new byte[] { });
        }
    }
}