using System;
using System.Net;
using BackendCommon.Services;
using log4net;
using MessagePack;
using PestelLib.ServerShared;
using S;
using ServerShared;

namespace BackendCommon.Code.Services.Concrete
{
    class HttpBackendService : IBackendService
    {
        private static ILog Log = LogManager.GetLogger(typeof(HttpBackendService));

        public Uri Url { get; }

        public HttpBackendService(Uri backendPath, bool pub, bool loc, bool online, DateTime maintenanceStart)
        {
            Url = backendPath;
            Public = pub;
            Internal = loc;
            Online = online;
            MaintenanceStart = maintenanceStart;
        }

        public bool Public { get; }
        public bool Internal { get; }
        public bool Maintenance => MaintenanceStart > DateTime.MinValue &&  DateTime.UtcNow > MaintenanceStart;
        public bool Online { get; private set; }
        public DateTime MaintenanceStart { get; }

        public bool IsOnline()
        {
            var url = $"{Url.Scheme}://{Url.Host}{Url.AbsolutePath}/api/test";
            try
            {
                var req = WebRequest.CreateHttp(url);
                req.Timeout = 1000;
                using (req.GetResponse()) {}
                Online = true;
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"GET {url} failed.", e);
            }

            return false;
        }

        public ServerResponse ProcessRequest(ServerRequest request)
        {
            if (!Online)
            {
                if(Internal)
                    throw new ResponseException(ResponseCode.SERVER_MAINTENANCE, $"Backend is offline. addr={Url}.");
                throw new ResponseException(ResponseCode.UNSUPPORTED_COMMANDS, $"Backend not supported. addr={Url}.");
            }
            if (!Internal)
            {
                throw new ResponseException(ResponseCode.UNSUPPORTED_COMMANDS, $"Backend not supported. addr={Url}.");
            }

            var data = MessagePackSerializer.Serialize(request);
            var url = $"{Url.Scheme}://{Url.Host}{Url.AbsolutePath}/service/mainhandler";
            Log.Debug($"Request {url}.");
            var req = WebRequest.CreateHttp(url);
            req.Method = "POST";
            var s = req.GetRequestStream();
            s.Write(data, 0, data.Length);
            using (var ans = req.GetResponse() as HttpWebResponse)
            {
                var ansData = ans.GetResponseStream().ReadAll();
                return MessagePackSerializer.Deserialize<ServerResponse>(ansData);
            }
        }

        public override string ToString()
        {
            return Url.ToString();
        }
    }
}
