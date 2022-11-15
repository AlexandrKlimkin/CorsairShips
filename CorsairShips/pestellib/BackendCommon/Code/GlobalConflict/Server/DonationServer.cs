using System;
using System.Threading.Tasks;
using BackendCommon.Code.GlobalConflict.Db;
using BackendCommon.Code.GlobalConflict.Server.Stages;
using PestelLib.ServerCommon.Extensions;
using ServerShared.GlobalConflict;
using UnityDI;

namespace BackendCommon.Code.GlobalConflict.Server
{
    class DonationServer : IDonationStage, IDonationStagePrivate
    {
        private GlobalConflictPrivateApi _api;
#pragma warning disable 0649
        [Dependency]
        private IDonationsDb _donationsDb;
#pragma warning restore 0649

        public DonationServer(GlobalConflictPrivateApi api)
        {
            _api = api;
            ContainerHolder.Container.BuildUp(this);
        }

        public void Donate(string userId, int amount, Action callback)
        {
            _api.DonationStagePrivateApi.DonateAsync(userId, amount).ResultToCallback(callback);
        }

        public async Task DonateAsync(string userId, int amount)
        {
            var state = await _api.ConflictsSchedulePrivateApi.GetCurrentConflictAsync().ConfigureAwait(false);
            var conflict = new Conflict(state);
            if(!await conflict.IsCurrentStage<DonationStage>().ConfigureAwait(false))
                throw new Exception("Not is donation stage");
            await _donationsDb.InsertAsync(userId, amount).ConfigureAwait(false);
        }

        public Task<Donation[]> UnprocessedDonationsAsync(int batchSize)
        {
            return _donationsDb.GetUnprocessedAsync(batchSize);
        }

        public Task ProcessedDonationsAsync(Donation[] donations)
        {
            return _donationsDb.MarkProcessedAsync(donations);
        }
    }
}