using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using PestelLib.ServerCommon.Geo;
using PestelLib.ServerCommon.Db;

namespace BackendCommon.Code.GeoIP.GeoLite2
{
    public class GeoLite2Provider : IGeoIPProvider
    {
#pragma warning disable 0649
        class CountryDTO
        {
            public ObjectId _id;
            public int start;
            public int end;
            public string network;
            public string country_iso;
        }

        class CountryV6DTO
        {
            public ObjectId _id;
            public string start;
            public string end;
            public string network;
            public string country_iso;
        }
#pragma warning restore 0649

        private IMongoCollection<CountryDTO> _country;
        private IMongoCollection<CountryV6DTO> _countryV6;

        public GeoLite2Provider(MongoUrl connStr, string dbname)
        {
            var c = connStr.GetServer();
            var db = c.GetDatabase(dbname);
            _country = db.GetCollection<CountryDTO>("country");
            _countryV6 = db.GetCollection<CountryV6DTO>("country_v6");
        }

        public bool IsEmpty()
        {
            var countV4 = _country.Count(Builders<CountryDTO>.Filter.Empty);
            if (countV4 > 0)
                return false;
            var countV6 = _countryV6.Count(Builders<CountryV6DTO>.Filter.Empty);
            return countV6 == 0;
        }

        public override async Task<string[]> Check(params string[] ips)
        {
            var tasks = new List<Task<string>>();
            for (var i = 0; i < ips.Length; ++i)
            {
                tasks.Add(CheckPrivate(ips[i]));
            }

            await Task.WhenAll(tasks);

            return Enumerable.ToArray<string>(tasks.Select(_ => _.Result));
        }

        private async Task<string> CheckPrivate(string ip)
        {
            IPAddress ipAddr;
            if (!IPAddress.TryParse(ip, out ipAddr))
                return null;

            var ipBytes = ipAddr.GetAddressBytes();
            if (ipAddr.AddressFamily == AddressFamily.InterNetwork)
            {
                var ipAddrInt = BitConverter.ToInt32(ipBytes.Reverse().ToArray(), 0);

                var filter = Builders<CountryDTO>.Filter.And(
                    Builders<CountryDTO>.Filter.Lte(_ => _.start, ipAddrInt),
                    Builders<CountryDTO>.Filter.Gte(_ => _.end, ipAddrInt)
                );
                var opts = new FindOptions<CountryDTO>();
                opts.Limit = 1;
                var r = await _country.FindAsync(filter, opts).ConfigureAwait(false);
                if (!r.MoveNext() || Enumerable.Count<CountryDTO>(r.Current) == 0)
                    return null;
                return Enumerable.First<CountryDTO>(r.Current).country_iso;
            }

            if (ipAddr.AddressFamily == AddressFamily.InterNetworkV6)
            {
                BigInteger digit = 1;
                BigInteger ipAddrBigInt = 0;
                ipBytes = ipBytes.Reverse().ToArray();
                for (var i = 0; i < ipBytes.Length; ++i)
                {
                    ipAddrBigInt += ipBytes[i] * digit;
                    digit *= 256;
                }

                var ipAddrStr = ipAddrBigInt.ToString();
                var filter = Builders<CountryV6DTO>.Filter.And(
                    Builders<CountryV6DTO>.Filter.Lte(_ => _.start, ipAddrStr),
                    Builders<CountryV6DTO>.Filter.Gte(_ => _.end, ipAddrStr));
                var opts = new FindOptions<CountryV6DTO>();
                opts.Limit = 1;
                var r = await _countryV6.FindAsync(filter, opts).ConfigureAwait(false);
                if (!r.MoveNext() || r.Current == null || Enumerable.Count<CountryV6DTO>(r.Current) == 0)
                    return null;
                return Enumerable.First<CountryV6DTO>(r.Current).country_iso;
            }

            return null;
        }
    }
}