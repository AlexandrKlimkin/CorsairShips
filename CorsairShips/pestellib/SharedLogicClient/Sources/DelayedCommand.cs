using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PestelLib.SharedLogicClient
{
#pragma warning disable 649
    internal class DelayedCommand
    {
        public object Cmd;
        public Action Finished;
    }
#pragma warning restore 649
}
