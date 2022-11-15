using System.Linq;
using System.Threading.Tasks;

namespace PestelLib.ServerCommon.Geo
{
    public abstract class IGeoIPProvider
    {
        // ISO 3166-1 alpha-2
        public abstract Task<string[]> Check(params string[] ips);
        public async Task<string> CheckOne(string ip)
        {
            var r = await Check(ip);
            return r.First();
        }
    }
}
