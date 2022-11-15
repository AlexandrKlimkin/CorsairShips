using System;
using BackendCommon.Code.GeoIP.GeoLite2;
using log4net;
using MongoDB.Driver;
using PestelLib.ServerCommon.Geo;
using ServerLib;

namespace BackendCommon.Code.GeoIP
{
    public static class GeoIPFactory
    {
        private static ILog _log = LogManager.GetLogger(typeof(GeoIPFactory));
        public static IGeoIPProvider Create()
        {
            try
            {
                var mongoGeo = new GeoLite2Provider(new MongoUrl(AppSettings.Default.GeoIpService.ConnectionString), AppSettings.Default.GeoIpService.DbName);
                if (!mongoGeo.IsEmpty())
                {
                    return mongoGeo;
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }

            return null;
        }
    }
}