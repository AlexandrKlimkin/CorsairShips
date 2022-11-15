using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Lidgren.Network;
using PestelLib.ChatServer;

namespace ChatServer.Transport
{
    class LidgrenConnection : ChatConnection
    {
        public IPEndPoint RemoteEndPoint { get; }
        public bool IsConnected => NetConnection.Status == NetConnectionStatus.Connected;
        public void Close()
        {
            NetConnection.Disconnect("User request");
        }

        public NetConnection NetConnection { get; }

        public LidgrenConnection(NetConnection conn)
        {
            NetConnection = conn;
            RemoteEndPoint = NetConnection.RemoteEndPoint;
        }
        public override bool Equals(object obj)
        {
            if (obj is LidgrenConnection objCon)
                return RemoteEndPoint == objCon.RemoteEndPoint;
            return false;
        }
    }
}
