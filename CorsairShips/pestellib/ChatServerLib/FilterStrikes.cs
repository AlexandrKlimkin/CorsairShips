using System.Collections.Generic;
using System.Linq;
using PestelLib.ChatCommon;

namespace PestelLib.ChatServer
{
    public class FilterStrikes
    {
        private HashSet<BanReason> _strikes = new HashSet<BanReason>();
        public int Count { get; private set; }
        public BanReason[] Resons => _strikes.ToArray();

        public int Add(BanReason reason)
        {
            _strikes.Add(reason);
            return ++Count;
        }

        public void Clear()
        {
            Count = 0;
            _strikes.Clear();
        }
    }
}
