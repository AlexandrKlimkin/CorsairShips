using System;
using System.Linq;
using System.Runtime.CompilerServices;
using ClansClientLib;
using Microsoft.Extensions.Caching.Memory;

namespace ClansServerLib.Mongo
{
    static class ClanRecordExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Size(this ClanPlayer p)
        {
            return (p.Role?.Length ?? 0) + 24;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Size(this ClanBooster p)
        {
            return 36 + (p.DefId?.Length ?? 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Size(this ClanRequirement p)
        {
            return 4 + (p.Name?.Length ?? 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Size(this ClanDesc p)
        {
            var reqSize = p.Requirements?.Select(_ => _.Size()).Sum() ?? 0;
            return 12 + reqSize + 
                   (p.Name?.Length ?? 0) + 
                   (p.Tag?.Length ?? 0) + 
                   (p.Description?.Length ?? 0) + 
                   (p.Emblem?.Length ?? 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Size(this ClanRecord rec)
        {
            var membersSize = rec.Members?.Select(_ => _.Size()).Sum() ?? 0;
            var boostersSize = rec.Boosters?.Select(_ => _.Size()).Sum() ?? 0;
            return 
                48 + membersSize + boostersSize + rec.Desc.Size();

        }
    }

    class ClanRecordCache
    {
        public ClanRecordCache()
        {
            _config = ClansConfigCache.Get();
            _cache = new MemoryCache(new MemoryCacheOptions() { SizeLimit = _config.ClanRecordCacheSize });
        }

        public void Put(ClanRecord clanRecord)
        {
            _cache.Set(GetClanKey(clanRecord.Id), clanRecord, new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = _config.ClanRecordCacheTTL,
                Size = clanRecord.Size()
            });
        }

        public ClanRecord Get(Guid clanId)
        {
            return _cache.Get<ClanRecord>(GetClanKey(clanId));
        }

        public void Invalidate(Guid clanId)
        {
            _cache.Remove(GetClanKey(clanId));
        }

        public void Invalidate(ClanRecord clanRecord)
        {
            _cache.Remove(GetClanKey(clanRecord.Id));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetClanKey(Guid clanId)
        {
            return $"Clan:{clanId}";
        }

        private ClansConfig _config;
        private MemoryCache _cache;
    }
}
