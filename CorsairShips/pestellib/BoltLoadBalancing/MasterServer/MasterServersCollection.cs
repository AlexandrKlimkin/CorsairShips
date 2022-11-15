using System;
using System.Collections.Concurrent;

namespace BoltLoadBalancing.MasterServer
{
    public class MasterServersCollection : ConcurrentDictionary<Guid, IMasterServer>
    {
    }
}