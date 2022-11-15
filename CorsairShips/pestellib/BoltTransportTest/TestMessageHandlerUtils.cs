using System;
using BoltTransport;
using MasterServerProtocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BoltTransportTest
{
    [TestClass]
    public class TestMessageHandlerUtils
    {
        private class BaseClass
        {
        }

        private class DerivedWithHandlers : BaseClass
        {
            [MessageHandler]
            private void ReserveSlotRequest(ReserveSlotRequest request)
            {
                
            }
            
            [MessageHandler]
            private CreateGameResponse CreateGameRequest(CreateGameRequest request)
            {
                return new CreateGameResponse
                {
                    MessageId = request.MessageId
                };
            }
        }

        [TestMethod]
        public void TestRefl()
        {
            var typesA = MessageHandlerUtils.GetHandlersFromType(typeof(DerivedWithHandlers));
            var typesB = MessageHandlerUtils.GetHandlersFromType(typeof(DerivedWithHandlers));

            Assert.IsTrue(typesB.ContainsKey(typeof(CreateGameRequest)));
            Assert.IsTrue(typesB.ContainsKey(typeof(ReserveSlotRequest)));
        }
    }
}