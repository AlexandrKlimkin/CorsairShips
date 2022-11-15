using System;
using System.IO;
using System.Net;

namespace BoltTransport
{
    public interface ITcpClient : IDisposable
    {
        //
        // Summary:
        //     Returns the System.Net.Sockets.NetworkStream used to send and receive data.
        //
        // Returns:
        //     The underlying System.Net.Sockets.NetworkStream.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The System.Net.Sockets.TcpClient is not connected to a remote host.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.TcpClient has been closed.
        Stream GetStream();

        IPAddress RemoteIP { get; }
    }
}
