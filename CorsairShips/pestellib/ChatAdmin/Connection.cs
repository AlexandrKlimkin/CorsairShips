using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAdmin
{
    public class Connection
    {
        public string Name { get; set; }
        public string ServerAddr { get; set; }
        public int ServerPort { get; set; }
        public string Secret { get; set; }
        public string UserName { get; set; }
        public bool Encrypted { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
