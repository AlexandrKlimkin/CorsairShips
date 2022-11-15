using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using log4net;
using PestelLib.ChatCommon;
using PestelLib.ChatServer;
using PestelLib.ServerCommon.Threading;

namespace ChatServer
{
    public interface IMessageFiler
    {
        // для старого формата
        bool CanRemove(string rawMessage);
        bool CanRemove(ChatProtocol message);
    }


    class RawJsonPatternMatcher : DisposableLoop, IMessageFiler
    {
        public RawJsonPatternMatcher(BanStorageFactory banStorageFactory)
        {
            _banStorageFactory = banStorageFactory;
        }

        public bool CanRemove(string rawMessage)
        {
            foreach (var pattern in _patterns)
            {
                try
                {
                    var muted = pattern.IsMatch(rawMessage);
                    if (muted)
                    {
                        Log.Debug($"Pattern matched {pattern}. message={rawMessage}");
                        return true;
                    }
                }
                catch
                {
                }
            }

            return false;
        }


        public bool CanRemove(ChatProtocol message)
        {
            throw new System.NotImplementedException();
        }

        protected override void Update(CancellationToken cancellationToken)
        {
            if (DateTime.UtcNow - _updateDate > TimeSpan.FromSeconds(60) || _patterns == null)
            {
                _patterns = _banStorageFactory.Get().BanPatterns();
                _updateDate = DateTime.UtcNow;
            }
        }

        private DateTime _updateDate = DateTime.MinValue;
        private Regex[] _patterns;
        private BanStorageFactory _banStorageFactory;

        private static readonly ILog Log = LogManager.GetLogger(typeof(RawJsonPatternMatcher));
    }

}
