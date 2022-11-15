using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PestelLib.ServerCommon.Geo;

namespace BackendCommon.Code.GeoIP.ip_api_com
{
    public class IpApiProvider : IGeoIPProvider
    {
#pragma warning disable 0649
        // http://ip-api.com/docs/api:batch
        class ResponseDTO
        {
            public string status;
            public string message;
            public string country;
            public string countryCode;
            public string region;
            public string regionName;
            public string city;
            public string district;
            public string zip;
            public float lat;
            public float lon;
            public string timezone;
            public string isp;
            public string org;
            [JsonProperty("as")]
            public string @as;
            public string reverse;
            public bool mobile;
            public bool proxy;
            public string query;
        }
#pragma warning restore 0649

        /*
         * from http://ip-api.com/docs/api:batch:
         *
         * This endpoint is limited to 150 requests per minute from an IP address. If you go over this limit your IP address will be blackholed. You can unban here.
         *
         * We do not allow commercial use of this endpoint. Please see our pro service for SSL access, unlimited queries and commercial support.
         *
         */

        public override async Task<string[]> Check(params string[] ips)
        {
            var r = WebRequest.CreateHttp("http://ip-api.com/batch?fields=countryCode");
            r.Method = "POST";
            r.ContentType = "application/json; charset=utf-8";
            var queries = ips.Select(_ => $"{{\"query\": \"{_}\"}}").ToArray();
            var batch = "[" + string.Join(",", queries) + "]";
            var data = Encoding.UTF8.GetBytes(batch);
            r.ContentLength = data.Length;

            var s = r.GetRequestStream();
            await s.WriteAsync(data, 0, data.Length);

            s.Close();

            var resp = await r.GetResponseAsync();

            s = resp.GetResponseStream();
            var respStr = await (new StreamReader(s)).ReadToEndAsync();
            var o = JsonConvert.DeserializeObject<ResponseDTO[]>(respStr);

            return Enumerable.Select<ResponseDTO, string>(o, _ => _.countryCode).ToArray();
        }
    }
}