using System;
using System.Linq;
using System.Threading.Tasks;
using MessagePack;
using PestelLib.ServerShared;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.Modules.GlobalConflict
{
    partial class GlobalConflictApiCallHandler
    {
        private async Task<byte[]> Leaderboards_GetWinPointsTop(byte[] bytes, ServerRequest request)
        {
            if (_api == null)
                throw new Exception("GlobalConflictPrivateApi not available");
            var args = MessagePackSerializer.Deserialize<object[]>(bytes);
            var r = await _api.LeaderboardsPrivateApi.GetWinPointsTopAsync((string)args[0], (string)args[1], (int)args[2], (int)args[3]).ConfigureAwait(false);
            if(r != null)
                await SetNames(r).ConfigureAwait(false);
            return MessagePackSerializer.Serialize(r);
        }

        private async Task<byte[]> Leaderbords_GetWinPointsTopMyPosition(byte[] bytes, ServerRequest request)
        {
            if (_api == null)
                throw new Exception("GlobalConflictPrivateApi not available");
            var args = MessagePackSerializer.Deserialize<object[]>(bytes);
            var r = await _api.LeaderboardsPrivateApi.GetWinPointsTopMyPositionAsync((string)args[0], (bool)args[1], (string)args[2]).ConfigureAwait(false);
            return MessagePackSerializer.Serialize(r);
        }

        private async Task<byte[]> Leaderboards_GetDonationTop(byte[] bytes, ServerRequest request)
        {
            if (_api == null)
                throw new Exception("GlobalConflictPrivateApi not available");
            var args = MessagePackSerializer.Deserialize<object[]>(bytes);
            var r = await _api.LeaderboardsPrivateApi.GetDonationTopAsync((string)args[0], (string)args[1], (int)args[2], (int)args[3]).ConfigureAwait(false);
            if(r != null)
                await SetNames(r).ConfigureAwait(false);
            return MessagePackSerializer.Serialize(r);
        }

        private async Task<byte[]> Leaderboards_GetDonationToMyPosition(byte[] bytes, ServerRequest request)
        {
            if (_api == null)
                throw new Exception("GlobalConflictPrivateApi not available");
            var args = MessagePackSerializer.Deserialize<object[]>(bytes);
            var r = await _api.LeaderboardsPrivateApi.GetDonationTopMyPositionAsync((string)args[0], (bool)args[1], (string)args[2]).ConfigureAwait(false);
            return MessagePackSerializer.Serialize(r);
        }

        private async Task SetNames(params PlayerState[] players)
        {
            var names = await _api.PlayersPrivateApi.GetNamesAsync(players.Select(_ => _.Id).ToArray<string>()).ConfigureAwait(false);
            for (var i = 0; i < players.Length; ++i)
            {
                var p = players[i];
                var name = names[i];
                if(string.IsNullOrEmpty(name))
                    continue;
                p.Name = name;
            }
        }
    }
}