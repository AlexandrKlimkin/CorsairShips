using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using MongoDB.Driver;
using PestelLib.ChatCommon;
using CaptainLib.Collections;
using MongoDB.Bson.Serialization.Attributes;
using PestelLib.ServerCommon.Db;

namespace PestelLib.ChatServer.Mongo
{
#pragma warning disable 649
    [BsonIgnoreExtraElements]
    class BanInfoMongo
    {
        [BsonId]
        public Guid Id;
        public string Token;
        public BanReason Reason;
        public long ExpiryTs;
    }

    [BsonIgnoreExtraElements]
    class BanRequestMongo
    {
        [BsonId]
        public Guid PlayerId;
        public DateTime CreateTime;
        public TimeSpan Time;
        public int[] ProcessedBy;
    }

    class IpBanInfoMongo
    {
        [BsonId]
        public Guid Id;
        public string Ip;
    }

    class BanPatterns
    {
        [BsonId] public string Pattern;
    }
#pragma warning restore 649

    public class BanStorageMongo : IBanStorage, IBanRequestStorage
    {
        public BanStorageMongo(MongoUrl url)
        {
            var c = url.GetServer();
            var db = c.GetDatabase(url.DatabaseName);
            _banCollection = db.GetCollection<BanInfoMongo>("ban_info");
            _ipBanCollection = db.GetCollection<IpBanInfoMongo>("ip_ban_info");
            _banPatterns = db.GetCollection<BanPatterns>("ban_patterns");
            _banRequests = db.GetCollection<BanRequestMongo>("ban_requests");

            _ipBanCollection.Indexes.CreateOne(Builders<IpBanInfoMongo>.IndexKeys.Ascending(_ => _.Ip));
            _banCollection.Indexes.CreateOne(Builders<BanInfoMongo>.IndexKeys.Combine(
                Builders<BanInfoMongo>.IndexKeys.Ascending(_ => _.Token),
                Builders<BanInfoMongo>.IndexKeys.Ascending(_ => _.Reason)
            ));
            _banCollection.Indexes.CreateOne(Builders<BanInfoMongo>.IndexKeys.Combine(
                Builders<BanInfoMongo>.IndexKeys.Ascending(_ => _.Token),
                Builders<BanInfoMongo>.IndexKeys.Descending(_ => _.ExpiryTs)
            ));

            _banRequests.Indexes.CreateOne(Builders<BanRequestMongo>.IndexKeys.Ascending(_ => _.ProcessedBy));
        }

        public bool IsBanned(IPEndPoint endPoint)
        {
            var count = _ipBanCollection.Count(Builders<IpBanInfoMongo>.Filter.Eq(_ => _.Ip, endPoint.Address.ToString()));
            return count > 0;
        }

        private BanInfoMongo NoBan(string token)
        {
            return new BanInfoMongo()
            {
                ExpiryTs = Int64.MinValue,
                Token = token,
            };
        }

        private BanInfoMongo GetBanInfo(string token)
        {
            return _bannedCache.GetOrCreate(token, () =>
            {
                var opts = new FindOptions<BanInfoMongo>();
                opts.Limit = 1;
                opts.Sort = Builders<BanInfoMongo>.Sort.Descending(_ => _.ExpiryTs);
                var r =
                    _banCollection.FindSync(Builders<BanInfoMongo>.Filter.And(
                        Builders<BanInfoMongo>.Filter.Eq(_ => _.Token, token),
                        Builders<BanInfoMongo>.Filter.Gt(_ => _.ExpiryTs, DateTime.UtcNow.Ticks)
                    ), opts);
                if (!r.MoveNext())
                    return NoBan(token);
                if (r.Current == null || r.Current.Count() == 0)
                    return NoBan(token);
                return r.Current.First();
            }, TimeSpan.FromSeconds(5));
        }

        public bool IsBanned(ChatUser user)
        {
            var bi = GetBanInfo(user.Token);
            return bi.ExpiryTs > DateTime.UtcNow.Ticks;
        }

        public BanInfo[] GetBans(ChatUser user)
        {
            return _bansCache.GetOrCreate(user.Token, () => GetBansInt(user), TimeSpan.FromSeconds(5));
        }

        public BanInfo[] GetBans(string userToken)
        {
            return _bansCache.GetOrCreate(userToken, () => GetBansInt(userToken), TimeSpan.FromSeconds(5));
        }

        private BanInfo[] GetBansInt(ChatUser user)
        {
            return GetBansInt(user.Token);
        }

        private BanInfo[] GetBansInt(string userToken)
        {
            var filter = Builders<BanInfoMongo>.Filter.And(
                Builders<BanInfoMongo>.Filter.Eq(_ => _.Token, userToken),
                Builders<BanInfoMongo>.Filter.Gt(_ => _.ExpiryTs, DateTime.UtcNow.Ticks));
            var cursor = _banCollection.FindSync(filter);
            var result = new List<BanInfo>();
            while (cursor.MoveNext())
            {
                result.AddRange(cursor.Current.Select(_ => new BanInfo()
                {
                    Token = _.Token,
                    Reason = _.Reason,
                    Expiry = new DateTime(_.ExpiryTs)
                }));
            }

            return result.ToArray();
        }

        public DateTime GrantBan(string token, BanReason reason, TimeSpan period)
        {
            _bannedCache.Invalidate(token);
            _bansCache.Invalidate(token);
            var filter = Builders<BanInfoMongo>.Filter.And(
                Builders<BanInfoMongo>.Filter.Eq(_ => _.Token, token),
                Builders<BanInfoMongo>.Filter.Eq(_ => _.Reason, reason)
            );
            var expiry = (DateTime.UtcNow + period).Ticks;
            var update = Builders<BanInfoMongo>.Update.Set(_ => _.ExpiryTs, expiry);
            var result = _banCollection.FindOneAndUpdate(filter, update);
            if (result == null)
            {
                result = new BanInfoMongo()
                {
                    Token = token,
                    Reason = reason,
                    ExpiryTs = expiry
                };
                _banCollection.InsertOne(result);
            }
            else
            {
                result.ExpiryTs += period.Ticks;
            }

            return new DateTime(expiry);
        }

        public DateTime GrantBan(ClientInfo user, BanReason reason, TimeSpan period)
        {
            return GrantBan(user.Token, reason, period);
        }

        public DateTime GrantBan(ClientInfo user, BanReason reason, DateTime expiry)
        {
            _bannedCache.Invalidate(user.Token);
            _bansCache.Invalidate(user.Token);
            var filter = Builders<BanInfoMongo>.Filter.And(
                Builders<BanInfoMongo>.Filter.Eq(_ => _.Token, user.Token),
                Builders<BanInfoMongo>.Filter.Eq(_ => _.Reason, reason)
            );
            var update = Builders<BanInfoMongo>.Update.Set(_ => _.ExpiryTs, expiry.Ticks);
            var result = _banCollection.FindOneAndUpdate(filter, update);
            if (result == null)
            {
                result = new BanInfoMongo()
                {
                    Token = user.Token,
                    Reason = reason,
                    ExpiryTs = expiry.Ticks
                };
                _banCollection.InsertOne(result);
            }

            return expiry;
        }

        public Regex[] BanPatterns()
        {
            List<Regex> result = new List<Regex>();
            var cur = _banPatterns.FindSync(Builders<BanPatterns>.Filter.Empty);
            while (cur.MoveNext())
            {
                if (cur.Current == null)
                    break;
                foreach (var pattern in cur.Current)
                {
                    try
                    {
                        var rgx = new Regex(pattern.Pattern);
                        result.Add(rgx);
                    }
                    catch
                    {
                    }
                }
            }

            return result.ToArray();
        }

        public BanRequest GetBanRequest(int myId)
        {
            var filter = Builders<BanRequestMongo>.Filter.AnyNe(_ => _.ProcessedBy, myId);
            var update = Builders<BanRequestMongo>.Update.Push(_ => _.ProcessedBy, myId);
            var cur = _banRequests.FindOneAndUpdate(filter, update);
            if (cur == null)
                return null;
            return new BanRequest()
            {
                PlayerId = cur.PlayerId,
                Period = cur.Time,
                CreateTime = cur.CreateTime
            };
        }

        public void AddBanRequest(Guid playerId, TimeSpan period)
        {
            var filter = Builders<BanRequestMongo>.Filter.Eq(_ => _.PlayerId, playerId);
            var update = Builders<BanRequestMongo>.Update.Combine(
                Builders<BanRequestMongo>.Update.Set(_ => _.ProcessedBy, new int[] { }),
                Builders<BanRequestMongo>.Update.Set(_ => _.CreateTime, DateTime.UtcNow),
                Builders<BanRequestMongo>.Update.Set(_ => _.Time, period)
            );
            var opts = new FindOneAndUpdateOptions<BanRequestMongo>();
            opts.IsUpsert = true;
            _banRequests.FindOneAndUpdate(filter, update, opts);
        }

        private MemCache<string, BanInfoMongo> _bannedCache = new MemCache<string, BanInfoMongo>();
        private MemCache<string, BanInfo[]> _bansCache = new MemCache<string, BanInfo[]>();
        private IMongoCollection<BanInfoMongo> _banCollection;
        private IMongoCollection<IpBanInfoMongo> _ipBanCollection;
        private IMongoCollection<BanPatterns> _banPatterns;
        private IMongoCollection<BanRequestMongo> _banRequests;
    }
}
