using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoltTransport
{
    public interface ITcpListener
    {
        //
        // Summary:
        //     Starts listening for incoming connection requests.
        //
        // Exceptions:
        //   T:System.Net.Sockets.SocketException:
        //     Use the System.Net.Sockets.SocketException.ErrorCode property to obtain the specific
        //     error code. When you have obtained this code, you can refer to the Windows Sockets
        //     version 2 API error code documentation in MSDN for a detailed description of
        //     the error.
        void Start();

        //
        // Summary:
        //     Closes the listener.
        //
        // Exceptions:
        //   T:System.Net.Sockets.SocketException:
        //     Use the System.Net.Sockets.SocketException.ErrorCode property to obtain the specific
        //     error code. When you have obtained this code, you can refer to the Windows Sockets
        //     version 2 API error code documentation in MSDN for a detailed description of
        //     the error.
        void Stop();

        //
        // Summary:
        //     Accepts a pending connection request as an asynchronous operation.
        //
        // Returns:
        //     Returns System.Threading.Tasks.Task`1 The task object representing the asynchronous
        //     operation. The System.Threading.Tasks.Task`1.Result property on the task object
        //     returns a System.Net.Sockets.TcpClient used to send and receive data.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The listener has not been started with a call to System.Net.Sockets.TcpListener.Start.
        //
        //   T:System.Net.Sockets.SocketException:
        //     Use the System.Net.Sockets.SocketException.ErrorCode property to obtain the specific
        //     error code. When you have obtained this code, you can refer to the Windows Sockets
        //     version 2 API error code documentation in MSDN for a detailed description of
        //     the error.
        Task<ITcpClient> AcceptTcpClientAsync();
    }
}
