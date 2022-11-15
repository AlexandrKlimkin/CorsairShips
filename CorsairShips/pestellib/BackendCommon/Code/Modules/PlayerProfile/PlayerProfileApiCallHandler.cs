using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Code.Statistics;
using MessagePack;
using PestelLib.ServerShared;
using ServerLib;
using ServerLib.PlayerProfile;
using ServerShared.PlayerProfile;
using ServerShared.Sources.PlayerProfile;
using UnityDI;

namespace Backend.Code.Modules.PlayerProfile
{
    public class PlayerProfileApiCallHandler : TypedApiCallHandler
    {
#pragma warning disable 0649
        [Dependency]
        private IProfileStorage _storage;
#pragma warning restore 0649

        public PlayerProfileApiCallHandler()
        {
            ContainerHolder.Container.BuildUp(this);

            if (AppSettings.Default.ProfileMainHandler)
            {
                RegisterHandler((int)PlayerProfileApi.Get, GetProfiled);
                RegisterHandler((int)PlayerProfileApi.Put, PutProfiled);
            }
            else
            {
                RegisterHandler((int)PlayerProfileApi.Get, Get);
                RegisterHandler((int)PlayerProfileApi.Put, Put);
            }
        }

        private async Task<byte[]> PutProfiled(byte[] bytes, ServerRequest request)
        {
            var profiles = MessagePackSerializer.Deserialize<ProfileDTO[]>(bytes);
            using (var ctx = MainHandlerBaseStats.Instance.NewCustomRequest($"PlayerProfilePut{profiles.Length}"))
            {
                try
                {
                    return await PutInt(profiles, request);
                }
                catch
                {
                    ctx.Error = true;
                    throw;
                }
            }
        }

        private Task<byte[]> Put(byte[] bytes, ServerRequest request)
        {
            var profiles = MessagePackSerializer.Deserialize<ProfileDTO[]>(bytes);
            return PutInt(profiles, request);
        }

        private async Task<byte[]> PutInt(ProfileDTO[] profiles, ServerRequest request)
        {
            var ids = profiles.Select(_ => _.PlayerId).ToArray();
            var uniq = await _storage.IsUnique(ids);
            var userId = new Guid(request.Request.UserId);
            if (!uniq)
            {
                _log.Error($"Trying to put profiles which are already exist. playerId={userId}, profiles={string.Join(",", ids)}.");
                throw new InvalidOperationException("Profile(s) already exist.");
            }

            var expiry = DateTime.MaxValue;
            if (AppSettings.Default.PlayerProfileSettings.PutExpire > TimeSpan.Zero)
            {
                expiry = DateTime.UtcNow.Add(AppSettings.Default.PlayerProfileSettings.PutExpire);
            }
            for (var i = 0; i < profiles.Length; i++)
            {
                var p = profiles[i];
                p.CreatedBy = userId;
                p.Expiry = expiry;

                await _storage.Create(p);
            }

            return null;
        }

        private Task<byte[]> Get(byte[] bytes, ServerRequest request)
        {
            var args = MessagePackSerializer.Deserialize<byte[]>(bytes);
            return GetInt(args, request);
        }

        private async Task<byte[]> GetProfiled(byte[] bytes, ServerRequest request)
        {
            var args = MessagePackSerializer.Deserialize<byte[]>(bytes);
            var count = args.Length / 16;
            using (var ctx = MainHandlerBaseStats.Instance.NewCustomRequest($"PlayerProfileGet{count}"))
            {
                try
                {
                    return await GetInt(args, request);
                }
                catch
                {
                    ctx.Error = true;
                    throw;
                }
            }
        }

        private async Task<byte[]> GetInt(byte[] bytes, ServerRequest request)
        {
            if (bytes.Length % 16 != 0)
                throw new InvalidOperationException("Not aligned Data");
            var count = bytes.Length / 16;
            var query = new Guid[count];
            for (var i = 0; i < count; ++i)
            {
                var guidData = bytes.Skip(i * 16).Take(16).ToArray();
                query[i] = new Guid(guidData);
            }

            var noCache = false;
            if (query.Length == 1)
            {
                var playerId = new Guid(request.Request.UserId);
                noCache = playerId == query[0];
                var r = await _storage.Get(query[0], noCache);
                return MessagePackSerializer.Serialize(new[] { r });
            }
            else
            {
                var r = await _storage.Get(query).ConfigureAwait(false);
                return MessagePackSerializer.Serialize(r);
            }
        }

    }
}