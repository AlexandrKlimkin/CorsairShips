using System;
using System.Linq;
using log4net;
using MessagePack;
using PestelLib.ServerClientUtils;
using S;
using ServerShared.PlayerProfile;
using ServerShared.Sources.PlayerProfile;
using UnityDI;

namespace PlayerProfile.Sources
{
    public class PlayerProfileClient : IProfileStorageCallback
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PlayerProfileClient));
#pragma warning disable 649
        [Dependency]
        private RequestQueue _requestQueue;
#pragma warning restore 649

        public event Action<ProfileDTO[]> OnProfilesUpdated = (p) => { };

        public PlayerProfileClient()
        {
            ContainerHolder.Container.BuildUp(this);
        }

        public void Get(Guid playerId, Action<bool, ProfileDTO> callback)
        {
            Get(new []{playerId}, (b, dtos) =>
            {
                if (!b)
                    callback(b, null);
                else
                {
                    callback(b, dtos[0]);
                    OnProfilesUpdated(dtos);
                }
            });
        }

        public void Get(Guid[] playerId, Action<bool, ProfileDTO[]> callback)
        {
            var method = "PlayerProfile_Get";
            var data = playerId.SelectMany(_ => _.ToByteArray()).ToArray();
            SendMessage(method, new TypedApiCall()
            {
                Type = (int)PlayerProfileApi.Get,
                Data = MessagePackSerializer.Serialize(data)
            }, (response, collection) =>
            {
                if (response.ResponseCode != ResponseCode.OK)
                {
                    Log.ErrorFormat("{0}: Server error {1}", method, response.ResponseCode);
                    callback(false, null);
                    return;
                }

                var result = MessagePackSerializer.Deserialize<ProfileDTO[]>(collection.Data);
                callback(true, result);
                OnProfilesUpdated(result);
            }, playerId.Length > 1);
        }

        public void Put(ProfileDTO[] profiles, Action<bool> callback)
        {
            var method = "PlayerProfile_Put";
            SendMessage(method, new TypedApiCall()
                {
                    Type = (int)PlayerProfileApi.Put,
                    Data = MessagePackSerializer.Serialize(profiles)
                }
                , (response, collection) =>
                {
                    if (response.ResponseCode != ResponseCode.OK)
                    {
                        Log.ErrorFormat("{0}: Server error {1}", method, response.ResponseCode);
                        callback(false);
                        return;
                    }

                    callback(true);
                });
        }

        private void SendMessage(string method, TypedApiCall apiCall, Action<Response, DataCollection> callback, bool async = true)
        {
            _requestQueue.SendRequest(method,
                new Request()
                {
                    PlayerProfile = apiCall
                }, callback, async: async);
        }
    }
}
