using System;
using System.Threading.Tasks;
using MessagePack;
using PestelLib.ServerShared;

namespace BackendCommon.Code.Modules.GlobalConflict
{
    partial class GlobalConflictApiCallHandler
    {
        private async Task<byte[]> Player_GetTeamPlayersStat(byte[] bytes, ServerRequest request)
        {
            if (_api == null)
                throw new Exception("GlobalConflictPrivateApi not available");
            var args = MessagePackSerializer.Deserialize<object[]>(bytes);
            var r = await _api.PlayersPrivateApi.GetTeamPlayersStatAsync((string) args[0]).ConfigureAwait(false);
            return MessagePackSerializer.Serialize(r);
        }

        private async Task<byte[]> Player_SetName(byte[] bytes, ServerRequest request)
        {
            if (_api == null)
                throw new Exception("GlobalConflictPrivateApi not available");
            var args = MessagePackSerializer.Deserialize<object[]>(bytes);
            var userId = new Guid(request.Request.UserId);
            await _api.PlayersPrivateApi.SetNameAsync(userId.ToString(), (string) args[0]).ConfigureAwait(false);
            return null;
        }

        private async Task<byte[]> Player_Register(byte[] bytes, ServerRequest request)
        {
            if (_api == null)
                throw new Exception("GlobalConflictPrivateApi not available");
            var args = MessagePackSerializer.Deserialize<object[]>(bytes);
            ValidateUserId(args, 1, request);
            var r = await _api.PlayersPrivateApi.RegisterAsync((string)args[0], (string)args[1], (string)args[2]).ConfigureAwait(false);
            return MessagePackSerializer.Serialize(r);
        }

        private async Task<byte[]> Player_GetPlayer(byte[] bytes, ServerRequest request)
        {
            if(_api == null)
                throw new Exception("GlobalConflictPrivateApi not available");
            var args = MessagePackSerializer.Deserialize<object[]>(bytes);
            ValidateUserId(args, 0, request);
            var r = await _api.PlayersPrivateApi.GetPlayerAsync((string)args[0], (string)args[1]).ConfigureAwait(false);
            if(r != null)
                await SetNames(r).ConfigureAwait(false);
            return MessagePackSerializer.Serialize(r);
        }
    }
}