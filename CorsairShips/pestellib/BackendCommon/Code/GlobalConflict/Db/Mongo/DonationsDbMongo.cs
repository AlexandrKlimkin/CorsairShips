using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using ServerLib;
using PestelLib.ServerCommon.Db;

namespace BackendCommon.Code.GlobalConflict.Db.Mongo
{
#pragma warning disable 0649
    class DonationMongo : Donation
    {
        public bool Processed;
    }
#pragma warning restore 0649

    class DonationsDbMongo : IDonationsDb
    {
        private IMongoCollection<DonationMongo> _donations;

        public DonationsDbMongo()
        {
            var cs = new MongoUrl(AppSettings.Default.GlobalConflictSettings.ConnectionString);
            var client = cs.GetServer();
            var db = client.GetDatabase(cs.DatabaseName);
            _donations = db.GetCollection<DonationMongo>("donations");
            _donations.Indexes.CreateOneAsync(Builders<DonationMongo>.IndexKeys.Ascending(_ => _.Time));
            _donations.Indexes.CreateOneAsync(Builders<DonationMongo>.IndexKeys.Ascending(_ => _.Processed));
        }

        public Task InsertAsync(string userId, int amount)
        {
            var item = new DonationMongo
            {
                UserId = userId,
                Amount =  amount
            };
            return _donations.InsertOneAsync(item);
        }

        public async Task<Donation[]> GetUnprocessedAsync(int batchSize)
        {
            var opts = new FindOptions<DonationMongo, DonationMongo> { Limit = batchSize };
            var cursor = await _donations.FindAsync(Builders<DonationMongo>.Filter.Eq(_ => _.Processed, false), opts).ConfigureAwait(false);
            var result = new List<Donation>();
            while (await cursor.MoveNextAsync().ConfigureAwait(false))
            {
                foreach (var donation in cursor.Current)
                {
                    result.Add(donation);
                }
            }
            return result.ToArray();
        }

        public Task MarkProcessedAsync(params Donation[] donations)
        {
            var ids = donations.Select(_ => _.Id).ToArray();
            return _donations.UpdateManyAsync(Builders<DonationMongo>.Filter.In(_ => _.Id, ids),
                Builders<DonationMongo>.Update.Set(_ => _.Processed, true));
        }

        public async Task Wipe()
        {
            await _donations.DeleteManyAsync(Builders<DonationMongo>.Filter.Empty).ConfigureAwait(false);
        }
    }
}