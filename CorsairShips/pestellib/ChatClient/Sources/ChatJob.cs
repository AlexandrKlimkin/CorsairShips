using System;
using System.Collections.Generic;
using log4net;
using PestelLib.ChatCommon;

namespace PestelLib.ChatClient {

    public class ChatJob
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ChatJob));
        public volatile bool jobDone = false;

        private Func<ChatJob, bool> _job;
        private Action<bool> _onComplete;
        private ChatProtocol _chatProtocol;
        private bool _success;

        public ChatProtocol ChatProtocol { get { return _chatProtocol; }}

        private ChatJob()
        {

        }

        public ChatJob(Func<ChatJob, bool> job, ChatProtocol chatProtocol = null)
        {
            _job = job;
            _chatProtocol = chatProtocol;
        }

        public ChatJob(Func<ChatJob, bool> job, Action<bool> onComplete, ChatProtocol chatProtocol = null)
        {
            _job = job;
            _onComplete = onComplete;
            _chatProtocol = chatProtocol;
        }

        public void DoJob()
        {
            try
            {
                _success = _job(this);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            jobDone = true;
        }

        public void NotifyComplete()
        {
            if (_onComplete != null)
                _onComplete(_success);
        }
    }
}