using ServerLib;
using log4net;

namespace PestelLib.ServerCommon.Db
{

    public static class ApiFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ApiFactory));
        private static IApiFactory _factory;

        static ApiFactory()
        {
            try
            {
                _factory = new MongoApiFactory(AppSettings.Default.PersistentStorageSettings.StorageConnectionString);
            }
            catch {}
        }

        public static IMatchInfo GetMatchInfoApi()
        {
            return _factory.GetMatchInfoApi();
        }
    }
}
