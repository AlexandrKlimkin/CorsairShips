using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Code.Utils;
using BackendCommon.Code;
using BackendCommon.Code.Data;
using BackendCommon.Code.Leagues;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using log4net;
using PestelLib.ServerCommon.Db;
using PestelLib.ServerCommon.Geo;
using PestelLib.ServerCommon.Utils;
using ServerShared.PlayerProfile;
using SharedLogicBase.Server.PlayerProfile;
using UnityDI;

namespace ServerLib.PlayerProfile
{
    public class MongoProfileStorage : IProfileStorage
    {
        private static ILog _log = LogManager.GetLogger(typeof(MongoProfileStorage));
        private readonly IMongoCollection<ProfileDTO> _profiles;
        private TimeSpan _botUpdatePeriod = TimeSpan.FromHours(2);
        private TimeSpan _playerUpdatePeriod = TimeSpan.FromMinutes(5);
#pragma warning disable 0649
        [Dependency]
        private IPlayerProfileProvider _playerProfileProvider;
        [Dependency]
        private IGeoIPProvider _geoIpProvider;
        [Dependency]
        private IPlayerIpResolver _playerIpResolver;
        [Dependency]
        private LeagueServer _leagueServer;
        [Dependency]
        private IPlayerIdToVersion _playerIdToVersion;
#pragma warning restore 0649

        public event Action<ProfileDTO> OnProfileUpdated = _ => { };

        static MongoProfileStorage()
        {
            BsonClassMap.RegisterClassMap<ProfileDTO>(map =>
            {
                map.AutoMap();
                map.MapIdField(_ => _.PlayerId);
                map.SetIgnoreExtraElements(true);
            });
        }

        public MongoProfileStorage(MongoUrl connectionString, string collection)
        {
            ContainerHolder.Container.BuildUp(this);

            var c = connectionString.GetServer();
            var db = c.GetDatabase(connectionString.DatabaseName);
            _profiles = db.GetCollection<ProfileDTO>(collection);

            var indexDef = Builders<ProfileDTO>.IndexKeys.Ascending(_ => _.Expiry);
            _profiles.Indexes.CreateOneAsync(indexDef);
        }

        public async Task Create(ProfileDTO profile)
        {
            await _profiles.InsertOneAsync(profile);
        }

        public Task<ProfileDTO[]> Get(Guid[] playerIds)
        {
            return Get(playerIds, true);
        }

        public async Task<bool> IsUnique(params Guid[] playerIds)
        {
            var filter = Builders<ProfileDTO>.Filter.In(_ => _.PlayerId, playerIds);
            return await _profiles.CountAsync(filter) == 0;
        }

        public async Task<long> RemoveExpired()
        {
            var filter = Builders<ProfileDTO>.Filter.Lt(_ => _.Expiry, DateTime.UtcNow);
            var r = await _profiles.DeleteManyAsync(filter);
            return r.DeletedCount;
        }

        private async Task<ProfileDTO[]> Get(Guid[] playerIds, bool updateProfile)
        {
            var notFound = new HashSet<Guid>(playerIds);
            var filter = Builders<ProfileDTO>.Filter.In(_ => _.PlayerId, playerIds);
            var cursor = await _profiles.FindAsync(filter);
            var result = new List<ProfileDTO>();
            while (await cursor.MoveNextAsync())
            {
                if (!updateProfile)
                {
                    result.AddRange(cursor.Current);
                }
                else
                {
                    foreach (var profileDto in cursor.Current)
                    {
                        var r = await UpdateProfile(profileDto.PlayerId, profileDto, Builders<ProfileDTO>.Filter.Eq(_ => _.PlayerId, profileDto.PlayerId), false);
                        if (r != null)
                        {
                            result.Add(r);
                            notFound.Remove(r.PlayerId);
                        }
                    }
                }
            }

            if (updateProfile)
            {
                foreach (var guid in notFound)
                {
                    var r = await UpdateProfile(guid, null, Builders<ProfileDTO>.Filter.Eq(_ => _.PlayerId, guid),
                        false);
                    if (r != null)
                        result.Add(r);
                    else
                    {
                        _log.ErrorFormat($"Player {guid} not found");
                    }
                }
            }

            return result.ToArray();
        }

        public async Task<ProfileDTO> Get(Guid playerId, bool noCache)
        {
            var filter = Builders<ProfileDTO>.Filter.Eq(_ => _.PlayerId, playerId);
            var opts = new FindOptions<ProfileDTO>();
            opts.Limit = 1;
            var cursor = await _profiles.FindAsync(filter, opts);
            if (!cursor.MoveNext())
                return await UpdateProfile(playerId, null, filter, noCache);

            var result =  cursor.Current.FirstOrDefault();
            result = await UpdateProfile(playerId, result, filter, noCache);

            return result;
        }

        private async Task<ProfileDTO> UpdateProfile(Guid playerId, ProfileDTO profile, FilterDefinition<ProfileDTO> filter, bool noCache)
        {
            var botLastUpdateDelta = TimeSpan.Zero;
            if (profile != null)
            {
                if (profile.CreatedBy != Guid.Empty)
                    return profile;
                if (profile.IsBot)
                {
                    botLastUpdateDelta = DateTime.UtcNow - profile.UpdateTime;
                    if (botLastUpdateDelta < _botUpdatePeriod)
                        return profile;
                }
                else if (!noCache)
                {
                    if (DateTime.UtcNow - profile.UpdateTime < _playerUpdatePeriod)
                        return profile;
                    var modDate = StateLoader.Storage.GetProfileModDate(profile.PlayerId);
                    if (modDate <= profile.UpdateTime)
                        return profile;
                }
            }
            else
            {
                profile = new ProfileDTO()
                {
                    PlayerId = playerId
                };
            }

            profile.UpdateTime = DateTime.UtcNow;
            var stateBytes = StateLoader.Storage.GetUserState(profile.PlayerId);
            ProfileDTO updatedProfile = null;
            var playerInfo = _leagueServer != null ? await _leagueServer.GetPlayer(profile.PlayerId) : null;
            if (stateBytes != null)
            {
                var logic = MainHandlerBase.ConcreteGame.SharedLogic(stateBytes, MainHandlerBase.FeaturesCollection);

                // to revaluate country reset ProdileDTO.Country in your implementation of IPlayerProfileProvider.CreateFromState
                updatedProfile = _playerProfileProvider.CreateFromState(playerInfo, logic, profile.Country);
                if (string.IsNullOrEmpty(profile.Country))
                {
                    var ip = _playerIpResolver.GetPlayerIp(profile.PlayerId);
                    if (ip != null && _geoIpProvider != null)
                        profile.Country = await _geoIpProvider.CheckOne(ip);
                }
            }
            else
            {
                if (!profile.IsBot) // profile of not existing user
                    return null;
                if (botLastUpdateDelta > TimeSpan.Zero)
                {
                    updatedProfile = _playerProfileProvider.UpdateLeagueBot(playerInfo, profile, botLastUpdateDelta);
                }
                else
                {
                    var pi = await _leagueServer.GetPlayer(profile.PlayerId);
                    if (pi == null)
                    {
                        _log.ErrorFormat($"League player {profile.PlayerId} not found.");
                        return null;
                    }

                    var divTop = await _leagueServer.DivisionPlayersRanks(profile.PlayerId);
                    var ids = divTop.Ranks.Where(_ => _.PlayerId != profile.PlayerId).Select(_ => _.PlayerId).ToArray();
                    var profiles = await Get(ids, false);
                    updatedProfile = _playerProfileProvider.CreateLeagueBot(playerInfo, profiles);
                    updatedProfile.Expiry = _leagueServer.SeasonEndTime.AddHours(1);
                }
            }
            updatedProfile.PlayerId = profile.PlayerId;
            updatedProfile.UpdateTime = DateTime.UtcNow;
            var version = _playerIdToVersion?.GetVersion(playerId);
            if (version != null && version > 0)
                updatedProfile.Version = version.Value;

            var opts = new FindOneAndReplaceOptions<ProfileDTO> {IsUpsert = true};
            _profiles.FindOneAndReplace(filter, updatedProfile, opts);

            OnProfileUpdated(updatedProfile);

            return updatedProfile;
        }
    }
}