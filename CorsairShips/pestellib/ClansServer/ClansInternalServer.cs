using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ClansClientLib;
using log4net;
using MessagePack;
using PestelLib.ServerCommon.MessageQueue;
using UnityDI;

namespace ClansServerLib
{
    interface IClanRequestHandler
    {
        Task Handle(ClansBackendMessage message);
    }
    class ClanRequestHandlerT<RequestT, ResponseT> : IClanRequestHandler
    {
        public ClanRequestHandlerT(Func<RequestT, Task<ResponseT>> handle)
        {
            _handle = handle;
        }

        public async Task Handle(ClansBackendMessage message)
        {
            var request = MessagePackSerializer.Deserialize<RequestT>(message.Data);
            var result = await _handle(request);
            var answerQueue = MessageQueueFactory.Instance.CreateWorkerQueuePublisher(ClansConfigCache.Get().MessageQueueConnectionString, message.Source);
            var resp = new ClansBackendResponse();
            resp.Tag = message.SourceTag;
            resp.Data = MessagePackSerializer.Serialize(resp);
        }

        private Func<RequestT, Task<ResponseT>> _handle;
    }

    class ClansInternalServer : IWorker
    {
        public ClansInternalServer()
        {
            Alive = true;

            _clanDb = ContainerHolder.Container.Resolve<IClansDbPrivate>();

            _handlers = new Dictionary<ClansBackendMessageType, IClanRequestHandler>()
            {
                //[ClansBackendMessageType.CreateClan] = new ClanRequestHandlerT<CreateClanRequest, CreateClanResult>(request => _clanDb.CreateClan(request.Description, request.Owner))
            };

            _config = ClansConfigCache.Get();
            _workerQueue = MessageQueueFactory.Instance.CreateWorkerQueueConsumer(_config.MessageQueueConnectionString, _config.MessageQueueAppId, this);
        }

        public async Task<bool> ProcessWork(byte[] data)
        {
            var message = MessagePackSerializer.Deserialize<ClansBackendMessage>(data);
            if (message.Type == ClansBackendMessageType.UserDelete)
            {
                var playerId = new Guid(message.Data);
                await _clanDb.UserDeletedPrivate(playerId);
            }
            else if (message.Type == ClansBackendMessageType.UserBanned)
            {
                var playerId = new Guid(message.Data);
                await _clanDb.UserBannedPrivate(playerId);
            }

            return true;
        }

        public bool Alive { get; private set; }

        private Dictionary<ClansBackendMessageType, IClanRequestHandler> _handlers;
        private IClansDbPrivate _clanDb;
        private IWorkerQueue _workerQueue;
        private ClansConfig _config;

        private static readonly ILog Log = LogManager.GetLogger(typeof(ClansInternalServer));
    }
}
