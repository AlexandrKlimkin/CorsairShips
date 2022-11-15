using System.Threading.Tasks;

namespace BackendCommon.Code.GlobalConflict.ApiProfiler
{
    class DonationStageProfiler : IDonationStagePrivate
    {
        private readonly IDonationStagePrivate _original;

        public DonationStageProfiler(IDonationStagePrivate original)
        {
            _original = original;
        }

        public async Task DonateAsync(string userId, int amount)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IDonationStagePrivate_DonateAsync"))
            {
                await _original.DonateAsync(userId, amount).ConfigureAwait(false);
            }
        }

        public async Task<Donation[]> UnprocessedDonationsAsync(int batchSize)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IDonationStagePrivate_UnprocessedDonationsAsync"))
            {
                return await _original.UnprocessedDonationsAsync(batchSize).ConfigureAwait(false);
            }
        }

        public async Task ProcessedDonationsAsync(params Donation[] donations)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IDonationStagePrivate_ProcessedDonationsAsync"))
            {
                await _original.ProcessedDonationsAsync(donations).ConfigureAwait(false);
            }
        }
    }
}