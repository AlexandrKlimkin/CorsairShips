using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using S;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PestelLib.ServerCommon.Db.Mongo
{
    public class ServerMessagesInboxMongo : ServerMessagesInbox
    {
        [BsonId]
        public Guid PlayerId;
    }

    public class MongoServerMessageStorage : IServerMessageStorage
    {
        public const int MaxInboxMessages = 20;

        public MongoServerMessageStorage(MongoUrl mongoUrl)
        {
            var server = mongoUrl.GetServer();
            var db = server.GetDatabase(mongoUrl.DatabaseName);
            _serverMessageCollection = db.GetCollection<ServerMessagesInboxMongo>("ServerMessages");
        }

        public bool SendMessage(ServerMessage message, Guid toUser)
        {
            var filter = Builders<ServerMessagesInboxMongo>.Filter.Eq(_ => _.PlayerId, toUser);
            var update = Builders<ServerMessagesInboxMongo>.Update.Push(_ => _.Messages, message);
            var opts = new FindOneAndUpdateOptions<ServerMessagesInboxMongo>();
            opts.IsUpsert = true;
            return _serverMessageCollection.FindOneAndUpdate(filter, update, opts) != null;
        }

        public byte[] GetMessages(Guid toUser)
        {
            var inbox = LoadInbox(toUser);
            if (inbox == null)
                return null;
            return MessagePack.MessagePackSerializer.Serialize<ServerMessagesInbox>(inbox);
        }

        public void ClearInbox(Guid toUser)
        {
            var filter = Builders<ServerMessagesInboxMongo>.Filter.Eq(_ => _.PlayerId, toUser);
            var update = Builders<ServerMessagesInboxMongo>.Update.Set(_ => _.Messages, new List<ServerMessage>());
            _serverMessageCollection.FindOneAndUpdate(filter, update);
        }

        public void Save(Guid user, ServerMessagesInbox inbox)
        {
            var item = new ServerMessagesInboxMongo()
            {
                PlayerId = user,
                Messages = inbox.Messages
            };

            try
            {
                _serverMessageCollection.InsertOne(item);
            }
            catch(MongoWriteException e)
            {
                // skip duplicates
                if (e?.WriteError.Code == 11000)
                    return;
                throw;
            }
        }

        public bool IsEmpty()
        {
            return _serverMessageCollection.Count(Builders<ServerMessagesInboxMongo>.Filter.Empty) == 0;
        }

        private ServerMessagesInbox LoadInbox(Guid userId)
        {
            var filter = Builders<ServerMessagesInboxMongo>.Filter.Eq(_ => _.PlayerId, userId);
            var result = _serverMessageCollection.Find(filter).SingleOrDefault();
            if (result?.Messages?.Count > MaxInboxMessages)
            {
                var toRemove = result.Messages.Count - MaxInboxMessages;
                result.Messages.RemoveRange(0, toRemove);
                var update = Builders<ServerMessagesInboxMongo>.Update.Combine(
                        Enumerable.Range(0, toRemove).Select(_ => Builders<ServerMessagesInboxMongo>.Update.PopFirst(f => f.Messages))
                    );
                _serverMessageCollection.FindOneAndUpdate(filter, update);
            }

            return result;
        }

        private IMongoCollection<ServerMessagesInboxMongo> _serverMessageCollection;
    }
}
