using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using S;
using ServerShared.GlobalConflict;
using UnityDI;

namespace BackendCommon.Code.GlobalConflict.Db.Mongo
{
    public static class GlobalConflictMongoInitializer
    {
        private static readonly object Sync = new object();
        private static bool _inited;

        public static void Init()
        {
            if (_inited)
                return;
            lock (Sync)
            {
                if (_inited)
                    return;

                InitDb();
                InitDi();
                _inited = true;
            }
        }

        private static void InitDi()
        {
            var battleDb = new BattleDbMongo();
            ContainerHolder.Container.RegisterCustom<IBattleDb>(() => battleDb);

            var protDb = new ConflictPrototypesDbMongo();
            ContainerHolder.Container.RegisterCustom<IConflictPrototypesDb>(() => protDb);

            var resultsDb = new ConflictResultsDbMongo();
            ContainerHolder.Container.RegisterCustom<IConflictResultsDb>(() => resultsDb);

            var scheduDb = new ConflictsScheduleDbMongo();
            ContainerHolder.Container.RegisterCustom<IConflictsScheduleDb>(() => scheduDb);

            var donatDb = new DonationsDbMongo();
            ContainerHolder.Container.RegisterCustom<IDonationsDb>(() => donatDb);

            var lbDb =new LeaderboardsDbMongo();
            ContainerHolder.Container.RegisterCustom<ILeaderboardsDb>(() => lbDb);

            var playDb = new PlayersDbMongo();
            ContainerHolder.Container.RegisterCustom<IPlayersDb>(() => playDb);
            
            var poiDb = new PointsOfInterestsDbMongo();
            ContainerHolder.Container.RegisterCustom<IPointsOfInterestsDb>(() => poiDb);
        }

        private static void InitDb()
        {
            // put some complex bson mapping here
            BsonClassMap.RegisterClassMap<GlobalConflictState>(cm =>
            {
                cm.AutoMap();
                cm.MapIdField(_ => _.Id);
            });
            BsonClassMap.RegisterClassMap<PlayerState>(cm =>
            {
                cm.AutoMap();
                cm.MapIdField(_ => _.Id);
            });
            BsonClassMap.RegisterClassMap<Donation>(m =>
            {
                m.AutoMap();
                m.MapIdField(_ => _.Id);
            });
            BsonClassMap.RegisterClassMap<BattleResultInfo>(cm =>
            {
                cm.AutoMap();
                cm.MapIdField(_ => _.Id);
            });
            BsonClassMap.RegisterClassMap<ConflictResult>(cm =>
            {
                cm.AutoMap();
                cm.MapIdField(_ => _.ConflictId);
            });
            BsonClassMap.RegisterClassMap<DeployedPointOfInterest>(cm =>
            {
                cm.AutoMap();
                cm.MapIdField(_ => _.Id);
            });
            BsonClassMap.RegisterClassMap<PlayerNameData>(cm =>
            {
                cm.AutoMap();
                cm.MapIdField(_ => _.PlayerId);
            });
        }

        public static async Task<bool> InsertNew(Task action)
        {
            try
            {
                await action.ConfigureAwait(false);
            }
            catch (MongoWriteException e)
            {
                if (e.WriteError == null || e.WriteError.Code != 11000)
                    throw;

                return false;
            }

            return true;
        }
    }
}