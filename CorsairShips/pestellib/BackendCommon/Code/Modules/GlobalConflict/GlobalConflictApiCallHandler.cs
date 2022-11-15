using System;
using System.Threading.Tasks;
using Backend.Code.Modules;
using BackendCommon.Code.GlobalConflict;
using MessagePack;
using PestelLib.ServerShared;
using ServerShared.GlobalConflict;
using UnityDI;

namespace BackendCommon.Code.Modules.GlobalConflict
{
    partial class GlobalConflictApiCallHandler : TypedApiCallHandler
    {
#pragma warning disable 0649
        [Dependency]
        private GlobalConflictPrivateApi _api;
#pragma warning restore 0649

        public GlobalConflictApiCallHandler()
        {
            ContainerHolder.Container.BuildUp(this);

            RegisterHandler(GlobalConflictApiTypes.ConflictsSchedule.GetCurrentConflict, ConflictsSchedule_GetCurrent);

            RegisterHandler(GlobalConflictApiTypes.Players.Register, Player_Register);
            RegisterHandler(GlobalConflictApiTypes.Players.GetPlayer, Player_GetPlayer);
            RegisterHandler(GlobalConflictApiTypes.Players.GetTeamPlayersStat, Player_GetTeamPlayersStat);
            RegisterHandler(GlobalConflictApiTypes.Players.SetName, Player_SetName);

            RegisterHandler(GlobalConflictApiTypes.DonationStage.Donate, Donation_Donate);

            RegisterHandler(GlobalConflictApiTypes.Battle.RegisterBattleResult, Battle_RegisterResult);

            RegisterHandler(GlobalConflictApiTypes.Leaderboards.GetDonationTopMyPosition, Leaderboards_GetDonationToMyPosition);
            RegisterHandler(GlobalConflictApiTypes.Leaderboards.GetDonationTop, Leaderboards_GetDonationTop);
            RegisterHandler(GlobalConflictApiTypes.Leaderboards.GetWinPointsTopMyPosition, Leaderbords_GetWinPointsTopMyPosition);
            RegisterHandler(GlobalConflictApiTypes.Leaderboards.GetWinPointsTop, Leaderboards_GetWinPointsTop);

            RegisterHandler(GlobalConflictApiTypes.ConflictResults.GetResult, ConflictResults_GetResult);

            RegisterHandler(GlobalConflictApiTypes.PointsOfInterest.GetTeamPointsOfInterest, PointsOfInterest_GetTeamPointsOfInterest);
            RegisterHandler(GlobalConflictApiTypes.PointsOfInterest.DeployPointOfInterestAsync, PointsOfInterest_DeployPointOfInterest);

            RegisterHandler(GlobalConflictApiTypes.Debug.AddTime, Debug_AddTime);
            RegisterHandler(GlobalConflictApiTypes.Debug.StartConflictById, Debug_StartConflictById);
            RegisterHandler(GlobalConflictApiTypes.Debug.StartConflictByProto, Debug_StartConflictByProto);
            RegisterHandler(GlobalConflictApiTypes.Debug.ListConflictPrototypes, Debug_ListConflictPrototypes);
        }

        private void ValidateUserId(object[] args, int index, ServerRequest request)
        {
            var argUserId = (string)args[index];
            var userId = new Guid(request.Request.UserId);
            if (new Guid(argUserId) != userId)
                throw new InvalidOperationException();
        }

        private async Task<byte[]> ConflictResults_GetResult(byte[] bytes, ServerRequest request)
        {
            if (_api == null)
                throw new Exception("GlobalConflictPrivateApi not available");
            var args = MessagePackSerializer.Deserialize<object[]>(bytes);
            var r = await _api.ConflictResultsPrivateApi.GetResultAsync((string)args[0]).ConfigureAwait(false);
            return MessagePackSerializer.Serialize(r);
        }

        private async Task<byte[]> Battle_RegisterResult(byte[] bytes, ServerRequest request)
        {
            if (_api == null)
                throw new Exception("GlobalConflictPrivateApi not available");
            var args = MessagePackSerializer.Deserialize<object[]>(bytes);
            var winMod = Decimal.Parse((string) args[3]);
            var loseMod = Decimal.Parse((string) args[4]);
            ValidateUserId(args, 0, request);
            await _api.BattlePrivateApi.RegisterBattleResultAsync((string)args[0], (int)args[1], (bool)args[2], winMod, loseMod).ConfigureAwait(false);
            await _api.Update().ConfigureAwait(false);
            return null;
        }

        private async Task<byte[]> Donation_Donate(byte[] bytes, ServerRequest request)
        {
            if (_api == null)
                throw new Exception("GlobalConflictPrivateApi not available");

            var args = MessagePackSerializer.Deserialize<object[]>(bytes);
            ValidateUserId(args, 0, request);
            await _api.DonationStagePrivateApi.DonateAsync((string)args[0], (int)args[1]).ConfigureAwait(false);
            await _api.Update().ConfigureAwait(false);
            return null;
        }

        private async Task<byte[]> ConflictsSchedule_GetCurrent(byte[] bytes, ServerRequest request)
        {
            if (_api == null)
                throw new Exception("GlobalConflictPrivateApi not available");
            var r = await _api.ConflictsSchedulePrivateApi.GetCurrentConflictAsync().ConfigureAwait(false);
            return MessagePackSerializer.Serialize(r);
        }

        private async Task<byte[]> PointsOfInterest_GetTeamPointsOfInterest(byte[] bytes, ServerRequest request)
        {
            if (_api == null)
                throw new Exception("GlobalConflictPrivateApi not available");
            var args = MessagePackSerializer.Deserialize<object[]>(bytes);
            var r = await _api.PointOfInterestPrivateApi.GetTeamPointsOfInterestAsync((string)args[0], (string)args[1]).ConfigureAwait(false);
            return MessagePackSerializer.Serialize(r);
        }

        private async Task<byte[]> PointsOfInterest_DeployPointOfInterest(byte[] bytes, ServerRequest request)
        {
            if (_api == null)
                throw new Exception("GlobalConflictPrivateApi not available");
            var args = MessagePackSerializer.Deserialize<object[]>(bytes);
            ValidateUserId(args, 1, request);
            await _api.PointOfInterestPrivateApi.DeployPointOfInterestAsync((string)args[0], (string)args[1], (string)args[2], (int)args[3], (string)args[4]).ConfigureAwait(false);
            return null;
        }
    }
}