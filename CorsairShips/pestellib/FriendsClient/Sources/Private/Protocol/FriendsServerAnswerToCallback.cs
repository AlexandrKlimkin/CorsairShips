using System;
using System.Collections.Generic;
using System.Linq;
using FriendsClient.Sources.Serialization;
using log4net;
using MessageServer.Sources;

namespace FriendsClient.Private
{
    public class FriendsServerAnswerToCallback<ResponseT> : IMessageHandler, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FriendsServerAnswerToCallback<ResponseT>));
        private readonly Dictionary<int, Action<ResponseT>> _callbacks = new Dictionary<int, Action<ResponseT>>();
        private readonly Dictionary<int, Action> _errors = new Dictionary<int, Action>();
        private readonly Dictionary<int, ResponseT> _instantAnswer = new Dictionary<int, ResponseT>();

        public void RegisterCallback(int tag, Action<ResponseT> callback, Action error = null)
        {
            ResponseT instant;
            if (_instantAnswer.TryGetValue(tag, out instant))
            {
                callback(instant);
                _instantAnswer.Remove(tag);
            }
            else
            {
                _callbacks[tag] = callback;
                if (error != null)
                    _errors[tag] = error;
            }
        }

        public void Handle(int sender, byte[] data, int tag, IAnswerContext answerContext)
        {
            var msg = FriendServerSerializer.Deserialize<ResponseT>(data);
            try
            {
                Action<ResponseT> callback;
                if (_callbacks.TryGetValue(tag, out callback))
                    callback(msg);
                else
                    _instantAnswer[tag] = msg;
            }
            catch (Exception e)
            {
                Log.Error("Response handle error.", e);
            }
            finally
            {
                _callbacks.Remove(tag);
            }
        }

        public void Error(int tag)
        {
            try
            {
                Action error;
                if (_errors.TryGetValue(tag, out error))
                    error();
            }
            catch (Exception e)
            {
                Log.Error($"Error while handle error =).", e);
            }
            finally
            {
                _callbacks.Remove(tag);
                _errors.Remove(tag);
            }
        }

        public void Dispose()
        {
            foreach (var k in _callbacks.ToArray())
            {
                Log.Debug($"Pending request abort. tag={k.Key}.");
                Error(k.Key);
            }
        }
    }
}