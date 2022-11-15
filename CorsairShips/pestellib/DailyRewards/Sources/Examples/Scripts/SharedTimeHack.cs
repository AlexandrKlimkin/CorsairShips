using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PestelLib.ServerClientUtils;

namespace PestelLib.DailyRewards
{
    class SharedTimeHack : SharedTime
    {
        TimeSpan _offset = TimeSpan.Zero;

        public event Action OnChange = () => { };

        public override DateTime Now
        {
            get
            {
                return DateTime.UtcNow + _offset;
            }
        }

        public void Add(TimeSpan period)
        {
            _offset += period;
            OnChange();
        }

        public void HackADay()
        {
            Add(TimeSpan.FromDays(1));
        }
    }
}
