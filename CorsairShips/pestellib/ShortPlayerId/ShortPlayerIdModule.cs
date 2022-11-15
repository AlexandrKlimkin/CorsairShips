using System;
using System.Threading.Tasks;
using PestelLib.ServerShared;
using PestelLib.UniversalSerializer;
using S;
using ServerExtension;
using ShortPlayerId.Storage;
using ShortPlayerIdProtocol;

namespace ShortPlayerId
{
    public class ShortPlayerIdModule : IAsyncExtension
    {
        private readonly ShortPlayerIdStorage _storage;

        public ShortPlayerIdModule(ShortPlayerIdStorage storage)
        {
            _storage = storage;
        }
        
        public async Task<ServerResponse> ProcessRequestAsync(byte[] requestData)
        {
            var request = Serializer.Deserialize<BaseShortPlayerRequest>(requestData);
            if (request is GetPlayerIdRequest r)
            {
                var playerId = await _storage.GetFullPlayerId(r.ShortPlayerId);
                return new ServerResponse
                {
                    ResponseCode = ResponseCode.OK,
                    Data = Serializer.Serialize(new GetPlayerIdResponse
                    {
                        PlayerId = playerId.ToByteArray()
                    })
                };
            }
            else if (request is GetShortPlayerIdRequest requestShort)
            {
                var shortId = await _storage.GetShortPlayerId(new Guid(requestShort.Guid));
                return new ServerResponse
                {
                    ResponseCode = ResponseCode.OK,
                    Data = Serializer.Serialize(new GetShortPlayerIdResponse
                    {
                        ShortPlayerId = shortId
                    })
                };
            }

            throw new UnknownRequestForExtension($"extension: {nameof(ShortPlayerIdModule)}");
        }
    }
}
