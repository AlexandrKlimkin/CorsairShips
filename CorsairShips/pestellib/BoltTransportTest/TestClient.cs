using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotonBoltNetworkUtils;
using MasterServerProtocol;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using System.Threading;
using BoltMasterServer;
using BoltTestLibrary.Mock;
using BoltTransport;
using Moq;

namespace BoltTransportTest
{
    [TestClass]
    public class TestClient
    {
        [TestMethod]
        public async Task ConnectOneClientToOneServerAndSendingOneMessage()
        {
            var tcpClient = new FakeTcpClient();

            using var client = new Client<ConnectionWithMessagesQueue>(
                new ConnectionWithMessagesQueue(),
                () => tcpClient,
                false);
            {

                var testMessage = new Message();
                client.SendMessage(testMessage);

                await Task.Delay(500);

                var remoteStream = tcpClient.GetRemoteStream();
                var remoteMessage = remoteStream.ReadMessage();
                
                Assert.AreEqual(remoteMessage.MessageId, testMessage.MessageId);
            }

            await Task.Delay(1000);
        }
        
        [TestMethod]
        public async Task ConnectOneClientToOneServerAndSendingOneMessageWithResponse()
        {
            void DelayedSendFakeServerResponseToStream(Stream stream, RequestServerResponse fakeResponse1)
            {
                _ = Task.Run(async () =>
                {
                    var clientRequest = await stream.ReadMessageAsync();
                    Assert.IsInstanceOfType(clientRequest, typeof(RequestServer));
                    stream.WriteMessage(FakeServerResponse((RequestServer)clientRequest));
                });
            }

            RequestServerResponse FakeServerResponse(RequestServer requestServer)
            {
                return new RequestServerResponse
                {
                    MessageId = requestServer.MessageId,
                    JoinInfo =  new JoinInfo
                    {
                        Port = 5555,
                        IpAddress = "127.0.0.1",
                        ReservedSlot = Guid.NewGuid()
                    }
                };
            }
            
            var tcpClient = new FakeTcpClient();

            using var client = new Client<ConnectionWithMessagesQueue>(
                new ConnectionWithMessagesQueue(),
                () => tcpClient,
                false);
            {

                var requestServer = new RequestServer();

                var fakeResponse = FakeServerResponse(requestServer);

                var remoteStream = ((FakeNetworkStream) tcpClient.GetStream()).RemotePCStream;
                DelayedSendFakeServerResponseToStream(remoteStream, fakeResponse);

                var requestServerResponse = await client.SendMessageAsync<RequestServerResponse>(requestServer);

                Assert.AreEqual(requestServer.MessageId, requestServerResponse.MessageId);
                Assert.AreEqual((object) requestServerResponse.JoinInfo.Port, fakeResponse.JoinInfo.Port);
                Assert.AreEqual((object) requestServerResponse.JoinInfo.IpAddress, fakeResponse.JoinInfo.IpAddress);
                Assert.AreNotEqual(Guid.Empty, requestServerResponse.JoinInfo.ReservedSlot);
            }

            await Task.Delay(1000);
        }
    }
}