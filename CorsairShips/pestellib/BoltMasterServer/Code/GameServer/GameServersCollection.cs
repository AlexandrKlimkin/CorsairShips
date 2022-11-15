using System.Collections.Concurrent;
using System;

namespace BoltMasterServer
{

    /// <summary>   Коллекция из игровых серверов, удобнее завести её в отдельном классе. </summary>
    internal class GameServersCollection : ConcurrentDictionary<Guid, IGameServer> {}
}
