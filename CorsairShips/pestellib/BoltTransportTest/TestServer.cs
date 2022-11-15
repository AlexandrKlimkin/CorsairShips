using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotonBoltNetworkUtils;
using MasterServerProtocol;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System;
using BoltTestLibrary.Mock;
using BoltTransport;

namespace BoltTransportTest
{
    [TestClass]
    public class TestServer
    {
        [TestMethod]
        public async Task ConnectOneClientToOneServerAndSendingOneMessage()
        {
            var tcpListener = new FakeTcpListener();
            var testConnection = new ConnectionWithMessagesQueue();

            var testServer = new Server<Connection>(tcpListener, () => testConnection);

            var client = tcpListener.EmulateClientConnection();
            
            var testMessage = new Message();

            client.WriteMessage(testMessage);
            
            await Task.Delay(100);
            
            var message = testConnection.ReceivedMessages.Dequeue();
            Assert.IsNotNull(message);
            Assert.AreEqual(testMessage.MessageId, message.MessageId);            
        }

        [TestMethod]
        public async Task ConnectMultipleClientsToOneServerAndSendingOneMessage()
        {
            var tcpListener = new FakeTcpListener();
            
            var connections = new List<ConnectionWithMessagesQueue>();

            var testServer = new Server<Connection>(tcpListener, () => {
                var connection = new ConnectionWithMessagesQueue();
                connections.Add(connection);
                return connection;
            });

            var clientA = tcpListener.EmulateClientConnection();
            var clientB = tcpListener.EmulateClientConnection();

            var sentMessages = new Message[] { new Message(), new Message() };

            clientA.WriteMessage(sentMessages[0]);
            clientB.WriteMessage(sentMessages[1]);

            await Task.Delay(100);

            var messageFromClientA = connections[0].ReceivedMessages.Dequeue();
            Assert.IsNotNull(messageFromClientA);
            Assert.AreEqual(sentMessages[0].MessageId, messageFromClientA.MessageId);
            Assert.AreEqual(1, connections[0].OnConnectionEstablishedCounter);

            var messageFromClientB = connections[1].ReceivedMessages.Dequeue();
            Assert.IsNotNull(messageFromClientB);
            Assert.AreEqual(sentMessages[1].MessageId, messageFromClientB.MessageId);
            Assert.AreEqual(1, connections[1].OnConnectionEstablishedCounter);
        }

        [TestMethod]
        public async Task TestConnectionEvents()
        {
            var tcpListener = new FakeTcpListener();
            var testConnection = new ConnectionWithMessagesQueue();

            var testServer = new Server<Connection>(tcpListener, () => testConnection);

            tcpListener.EmulateClientConnection(out var serverStream);
            await Task.Delay(500);
            serverStream.Close();
            await Task.Delay(500);

            Assert.AreEqual(1, testConnection.OnConnectionEstablishedCounter);
            Assert.AreEqual(1, testConnection.OnConnectionLostCounter);
        }

        [TestMethod]
        public async Task TestSendMessageByPiecesWithRandomDelays()
        {
            byte[] MakeByteArrayWith7BitEncodedLengthPrefixFromMessage(Message msg)
            {
                var memoryStream = new MemoryStream();
                memoryStream.WriteMessage(msg);
                memoryStream.Position = 0;
                return memoryStream.ToArray();
            }

            var tcpListener = new FakeTcpListener();
            var testConnection = new ConnectionWithMessagesQueue();

            var testServer = new Server<Connection>(tcpListener, () => testConnection);

            var clientStream = tcpListener.EmulateClientConnection();

            var testMessage = new Message();

            var messageBytes = MakeByteArrayWith7BitEncodedLengthPrefixFromMessage(testMessage);

            var bytesQueue = new Queue<byte>(messageBytes);

            while (bytesQueue.Count > 0)
            {
                var byteToSend = bytesQueue.Dequeue();
                await Task.Delay(10);
                clientStream.WriteByte(byteToSend);
            }

            await Task.Delay(1000);

            var message = testConnection.ReceivedMessages.Dequeue();
            Assert.IsNotNull(message);
            Assert.AreEqual(testMessage.MessageId, message.MessageId);
        }

    }
}
