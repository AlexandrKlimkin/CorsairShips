using System;
using System.Collections.Generic;

namespace Backend.Code.Utils
{
    interface IPlayerIdToVersion
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        int GetVersion(Guid playerId);
    }

    public class PlayerIdToVersion : IPlayerIdToVersion
    {
        private Dictionary<Guid, int> _prev = new Dictionary<Guid, int>();
        private Dictionary<Guid, int> _current = new Dictionary<Guid, int>();
        private TimeSpan _swapPeriod;
        private DateTime _nextSwap;
        private object _sync = new object();

        public PlayerIdToVersion(TimeSpan swapPeriod)
        {
            _swapPeriod = swapPeriod;
            _nextSwap = DateTime.UtcNow + _swapPeriod;
        }

        public PlayerIdToVersion()
            :this(TimeSpan.FromMinutes(5))
        {
        }

        public int GetVersion(Guid playerId)
        {
            lock (_sync)
            {
                if (_current.TryGetValue(playerId, out var version))
                    return version;
                if (_prev.TryGetValue(playerId, out version))
                    return version;
                return 0;
            }
        }

        public void SetVersion(Guid playerId, int version)
        {
            lock (_sync)
            {
                if (_nextSwap <= DateTime.UtcNow)
                {
                    var t = _prev;
                    _prev = _current;
                    _current = t;
                    _current.Clear();
                    _nextSwap = DateTime.UtcNow + _swapPeriod;
                }
                _current[playerId] = version;
            }
        }
    }
}