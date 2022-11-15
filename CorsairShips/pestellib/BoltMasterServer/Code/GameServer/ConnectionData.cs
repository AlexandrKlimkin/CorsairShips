using System;
using System.Runtime.CompilerServices;


[assembly: InternalsVisibleTo("BoltMasterServerTest")]
namespace BoltMasterServer
{
    internal class ConnectionData
    {
        public string IPAddress;
        public int Port;
        public Guid GameServerId;
    }
}