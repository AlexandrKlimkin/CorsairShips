using System;
using System.Collections.Generic;
using System.Text;

namespace ClansServerLib
{
    class ClanServerUserBinder
    {
        public ClanServerUserBinder()
        {
            _playerToSender = new Dictionary<Guid, int>();
            _senderToPlayer = new Dictionary<int, Guid>();
        }

        public void Bind(int sender, Guid playerId)
        {
            lock (_sync)
            {
                _playerToSender[playerId] = sender;
                _senderToPlayer[sender] = playerId;
            }
        }

        public void Unbind(int sender)
        {
            lock (_sync)
            {
                _senderToPlayer.Remove(sender, out var player);
                _playerToSender.Remove(player);
            }
        }

        public Guid GetPlayer(int sender)
        {
            lock (_sync)
            {
                if (_senderToPlayer.TryGetValue(sender, out var playerId))
                    return playerId;
            }
            return Guid.Empty;
        }

        public int GetSender(Guid playerId)
        {
            lock (_sync)
            {
                if (_playerToSender.TryGetValue(playerId, out var sender))
                    return sender;
            }
            return 0;
        }

        public IEnumerable<int> GetSenders(IEnumerable<Guid> playerIds)
        {
            List<int> result = new List<int>();
            lock (_sync)
            {
                foreach (var id in playerIds)
                {
                    if(_playerToSender.TryGetValue(id, out var sender))
                        result.Add(sender);
                }
            }

            return result;
        }


        private object _sync = new object();
        private Dictionary<int, Guid> _senderToPlayer;
        private Dictionary<Guid, int> _playerToSender;
    }
}
