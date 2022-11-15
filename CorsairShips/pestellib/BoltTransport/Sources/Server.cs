using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BoltTransport
{
    /// <summary>
    /// Сервер. Слушает подключения, обычно через TcpListener, создаёт для каждого свой экземпляр
    /// IConnection. В конкретном подклассе IConnection содержится код обработки сообщений с клиентов
    /// и ответов на них.
    /// 
    /// Сервер можно остановить через вызов Dispose()
    /// </summary>
    ///
    /// <typeparam name="T">    Подкласс Connection. </typeparam>
    public class Server<T> : IDisposable where T : IConnection, new()
    {
        ITcpListener listener = null;
        private Func<T> createConnectionInstance;
        
        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;

        /// <summary>
        /// В этом конструкторе вместо настоящего сетевого TcpListener'a можно передать заглушку для
        /// тестирования.
        /// </summary>
        ///
        /// <param name="listener">                 Это может быть FakeTcpListener или
        ///                                         NetworkTcpListener. </param>
        /// <param name="createConnectionInstance"> У подкласса connection часто есть зависимости,
        ///                                         которые ему нужно передать в конструктор, о которых
        ///                                         Server ничего не знает. Поэтому для создания
        ///                                         экземпляров Connection он использует эту функцию. </param>
        public Server(ITcpListener listener, Func<T> createConnectionInstance)
        {
            this.createConnectionInstance = createConnectionInstance;
            this.listener = listener;
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
            listener.Start();
            Task.Run(StartListener, cancellationToken).ConfigureAwait(false);
        }

        private Server(ITcpListener listener) : this(listener, () => new T())
        {
        }

        public Server(int port) : this(new NetworkTcpListener(IPAddress.Any, port))
        {
        }

        /// <summary>
        /// Наиболее часто используемый конструктор. Указываем какой порт слушать, и как создавать
        /// экземпляры соединения.
        /// </summary>
        ///
        /// <param name="port">                     порт. </param>
        /// <param name="createConnectionInstance"> У подкласса connection часто есть зависимости,
        ///                                         которые ему нужно передать в конструктор, о которых
        ///                                         Server ничего не знает. Поэтому для создания
        ///                                         экземпляров Connection он использует эту функцию. </param>
        public Server(int port, Func<T> createConnectionInstance) : this(new NetworkTcpListener(IPAddress.Any, port), createConnectionInstance)
        {
        }

        private async Task StartListener()
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    ITcpClient client = await listener.AcceptTcpClientAsync();
                    _ = HandleDevice(client);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                listener.Stop();
            }
        }

        private async Task HandleDevice(ITcpClient client)
        {
            try
            {
                using (client)
                {
                    using (var stream = client.GetStream())
                    {
                        var connection = createConnectionInstance();
                        connection.CancellationToken = cancellationToken;
                        connection.RemoteIP = client.RemoteIP;//((IPEndPoint)client.Client.RemoteEndPoint).Address;

                        await connection.MainLoop(stream);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Server<{typeof(T).Name}> disconnected: {e.Message}");
            }
        }

        /// <summary>   Остановка сервера. </summary>
        public void Dispose()
        {
            if (listener != null)
            {
                listener.Stop();
            }

            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }
}
