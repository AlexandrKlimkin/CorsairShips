using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PestelLib.UniversalSerializer;
using S;
using ShortPlayerId;
using ShortPlayerId.Storage;
using ShortPlayerIdProtocol;

namespace ShortPlayerIdTest
{

    [TestClass]
    public class ShortPlayerIdTest
    {
        [TestMethod]
        public async Task GetShortPlayerId()
        {
            var shortPlayerIdModule = new ShortPlayerIdModule(new ShortPlayerIdStorage("mongodb://localhost:27017"));

            var newLongPlayerId = Guid.NewGuid();

            var getShortPlayerIdRequestSerialized = Serializer.Serialize<BaseShortPlayerRequest>(new GetShortPlayerIdRequest
            {
                Guid = newLongPlayerId.ToByteArray()
            });
            
            var response = await shortPlayerIdModule.ProcessRequestAsync(getShortPlayerIdRequestSerialized);
            var getShortPlayerIdResponse = Serializer.Deserialize<GetShortPlayerIdResponse>(response.Data);

            Assert.IsTrue(getShortPlayerIdResponse.ShortPlayerId > 0);
            Assert.IsNotNull(response);
            Assert.AreEqual(ResponseCode.OK, response.ResponseCode);
            
            var getPlayerIdRequest = Serializer.Serialize<BaseShortPlayerRequest>(new GetPlayerIdRequest
                {
                    ShortPlayerId = getShortPlayerIdResponse.ShortPlayerId
                }
            );
            var responseGetPlayerIdRequest = await shortPlayerIdModule.ProcessRequestAsync(getPlayerIdRequest);
            var getPlayerIdResponse = Serializer.Deserialize<GetPlayerIdResponse>(responseGetPlayerIdRequest.Data);

            Assert.IsNotNull(getPlayerIdResponse);
            Assert.IsNotNull(getPlayerIdResponse.PlayerId);
            Assert.AreEqual(new Guid(getPlayerIdResponse.PlayerId), newLongPlayerId);
        }
    }
}
