using System;
using System.Threading.Tasks;

namespace BackendCommon.Code.GlobalConflict
{
    public class Donation
    {
        public Guid Id;
        public DateTime Time = DateTime.UtcNow;
        public string UserId;
        public int Amount;
    }

    public interface IDonationStagePrivate
    {
        Task DonateAsync(string userId, int amount);
        Task<Donation[]> UnprocessedDonationsAsync(int batchSize);
        Task ProcessedDonationsAsync(params Donation[] donations);
    }
}