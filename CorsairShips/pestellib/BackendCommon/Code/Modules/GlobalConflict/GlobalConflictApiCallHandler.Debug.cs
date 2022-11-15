using System;
using System.Threading.Tasks;
using MessagePack;
using PestelLib.ServerShared;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.Modules.GlobalConflict
{
    public partial class GlobalConflictApiCallHandler
    {
        private async Task<byte[]> Debug_AddTime(byte[] bytes, ServerRequest request)
        {
            if (_api == null)
                throw new Exception("GlobalConflictPrivateApi not available");
            var args = MessagePackSerializer.Deserialize<object[]>(bytes);
            await _api.DebugPrivateApi.AddTimeAsync((int) args[0]).ConfigureAwait(false);
            return null;
        }

        private async Task<byte[]> Debug_StartConflictById(byte[]bytes, ServerRequest request)
        {
            if (_api == null)
                throw new Exception("GlobalConflictPrivateApi not available");
            var args = MessagePackSerializer.Deserialize<object[]>(bytes);
            var r = await _api.DebugPrivateApi.StartConflictAsync((string) args[0]).ConfigureAwait(false);
            return MessagePackSerializer.Serialize(r);
        }

        private async Task<byte[]> Debug_StartConflictByProto(byte[] bytes, ServerRequest request)
        {
            if (_api == null)
                throw new Exception("GlobalConflictPrivateApi not available");
            var proto = MessagePackSerializer.Deserialize<GlobalConflictState>(bytes);
            var r = await _api.DebugPrivateApi.StartConflictAsync((GlobalConflictState) proto).ConfigureAwait(false);
            return MessagePackSerializer.Serialize(r);
        }

        private async Task<byte[]> Debug_ListConflictPrototypes(byte[] bytes, ServerRequest request)
        {
            if (_api == null)
                throw new Exception("GlobalConflictPrivateApi not available");

            var r = await _api.DebugPrivateApi.ListConflictPrototypesAsync().ConfigureAwait(false);
            return MessagePackSerializer.Serialize(r);
        }
    }
}