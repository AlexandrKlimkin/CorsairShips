using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MessagePack;
using PestelLib;
using PestelLib.ServerShared;
using S;
using ServerShared.PlayerProfile;
using ServerShared.Sources.PlayerProfile;

namespace FriendsServer.Db.External
{
    class ExternalProfileStorage : IProfileStorage
    {
        private byte[] _notExisting = Guid.NewGuid().ToByteArray();
        private static readonly ILog Log = LogManager.GetLogger(typeof(ExternalProfileStorage));
        ConcurrentDictionary<Guid, ProfileDTO> _map = new ConcurrentDictionary<Guid, ProfileDTO>();
        ThreadLocal<Random> _random = new ThreadLocal<Random>(() => new Random());
        private HttpClient _client = new HttpClient();
        private ServerConfig _config = ServerConfigCache.Get();

        public async Task<ProfileDTO> Get(Guid playerId)
        {
            if (_config.ProfileServiceEnabled)
            {
                try
                {
                    var r = await _getFromService(playerId);
                    if (r[0] == null)
                        throw null;
                    return r[0];
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            return _get(playerId);
        }

        public async Task<ProfileDTO[]> GetMany(params Guid[] playerId)
        {
            if (_config.ProfileServiceEnabled)
            {
                try
                {
                    var r = (await _getFromService(playerId)).Where(_ => _ != null).ToArray();
                    if (r.Length == playerId.Length)
                        return r;
                    var notFound = playerId.Where(_ => r.All(p => p.PlayerId != _));
                    return r.Union(notFound.Select(_get)).ToArray();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            return playerId.Select(_get).ToArray();
        }

        public Task<bool> IsBot(Guid playerId)
        {
            return Task.FromResult(false);
        }

        private ProfileDTO _get(Guid id)
        {
            ProfileDTO profileDto;
            if (_map.TryGetValue(id, out profileDto))
            {
                return profileDto;
            }
            return _map[id] = new ProfileDTO()
            {
                PlayerId = id,
                Nick = NicknameGenerator.GenerateName(),
                UpdateTime = DateTime.UtcNow,
                Level = _random.Value.Next(50),
            };
        }

        private async Task<ProfileDTO[]> _getFromService(params Guid[] playerId)
        {
            var request = new Request();
            var dataGuids = playerId.SelectMany(_ => _.ToByteArray()).ToArray();
            request.UserId = _notExisting;
            request.PlayerProfile = new TypedApiCall()
            {
                Type = (int) PlayerProfileApi.Get,
                Data = MessagePackSerializer.Serialize(dataGuids)
            };
            var data = MessagePackSerializer.Serialize(request);
            var content = new ByteArrayContent(data);
            var url = new Uri(_config.ProfileServiceUrl);
            var resp = await _client.PostAsync(url, content);
            var respData = await resp.Content.ReadAsByteArrayAsync();
            var profilesArray = MessagePackSerializer.Deserialize<ProfileDTO[]>(respData);
            return profilesArray;
        }
    }
}
