using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ISO3166;
using MongoDB.Bson.Serialization.Attributes;
using PestelLib.ServerCommon.Geo;
using PestelLib.ServerCommon.Utils;
using ServerShared;
using ServerShared.Messaging;
using UnityDI;

namespace PestelLib.ServerCommon.Messaging
{
    [BsonIgnoreExtraElements]
    public class BroadcastMessageFilter : Dictionary<string, string[]>
    {
    }

    public class PlayerIdsFilter
    {
        public const string Name = "PlayerIdsFilter";
    }

    public class SharedLogicVersion
    {
        public const string Name = "SharedLogicVersion";
    }

    public class AbTestingGroupFilter
    {
        public const string Name = "AbTestingGroup";
        public static string[] Values = new[] { "Group A", "Group B" };
    }

    public class GeoFilter
    {
        private static Lazy<string[]> _lazyIso = new Lazy<string[]>(() => Country.List.Select(_ => _.TwoLetterCode).ToArray());
        public const string Name = "GeoFilter";
        public static string[] Values => _lazyIso.Value;
    }

#pragma warning disable 169
#pragma warning disable 649
    public class SystemLanguageFilter
    {
        private static readonly string[] UnityLangs;
        public const string Name = "SystemLanguage";
        public static string[] Values;

        static SystemLanguageFilter()
        {
            List<string> result = new List<string>();
            foreach (UnitySystemLanguage lang in Enum.GetValues(typeof(UnitySystemLanguage)))
            {
                result.Add(lang.ToString());
            }
            Values = result.ToArray();
        }
    }
    
    public class PlayerFilterMatcher
    {
        [Dependency]
        private IGeoIPProvider _geoIpProvider;
        [Dependency]
        private IPlayerIpResolver _ipResolver;
#pragma warning restore 169
#pragma warning restore 649

        public virtual Dictionary<string, string> PlayerFilters(S.Request request)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            var experimental = AbTesting.IsInExperimentGroup(request.UserId);

            result[AbTestingGroupFilter.Name] = AbTestingGroupFilter.Values[experimental ? 1 : 0];
            result[SharedLogicVersion.Name] = request.SharedLogicVersion.ToString();

            if (_geoIpProvider != null)
            {
                var userGuid = new Guid(request.UserId);
                var ip = _ipResolver.GetPlayerIp(userGuid);
                //result[GeoFilter.Name] = _geoIpProvider.CheckOne(ip).Result;
                result[GeoFilter.Name] = null;
            }

            if (request.GetServerMessagesInbox != null)
            {
                result[SystemLanguageFilter.Name] = request.GetServerMessagesInbox.SystemLanguage;
            }

            return result;
        }
    }
}
