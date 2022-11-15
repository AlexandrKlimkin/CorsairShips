using System;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MasterServerProtocol;

namespace BoltTransport
{
    /// <summary>
    /// Клиент. Обёртка над обычным TcpClient, которая позволяет использовать подклассы IConnection
    /// для обработки сообщений от сервера, и для отправки сообщений на сервер.
    /// 
    /// Так же в клиенте есть простая реализация восстановления соединения в случае ошибок.
    /// 
    /// Класс - IDisposable - вы можете вызвать Dispose, если вам больше не нужно соединение с
    /// сервером.
    /// </summary>
    ///
    /// <typeparam name="T">    Подкласс IConnection. </typeparam>
    public class Client<T> : IDisposable where T : IConnection, new ()
    {
        private static ILog _log = LogManager.GetLogger(typeof(Client<T>));

        public event Action<Exception> OnConnectionFailed = (e) => { };

        /// <summary>
        /// Задержка на попытку восстановления подключения в мс. Используется только в том случае, если в
        /// конструкторе клиента было передано autoReconnet = true.
        /// </summary>
        public int ReconnectDelayMs = 1000;
        
        /// <summary>
        /// Для удобства тестирования в этом классе используется не непосредственно TcpClient, а
        /// ITcpClient - это позволяет при тестировании вместо настоящего сетевого соединения
        /// использовать заглушки. В большинстве случаев вызов этого делегата создаст экземпляр
        /// NetworkTcpClient - простейшей обёртки над TcpClient. При тестировании вместо этого могут
        /// использоваться заглушки, например FakeTcpClient.
        /// </summary>
        public delegate ITcpClient CreateTcpClientFunc();
        
        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;

        /// <summary>
        /// Именно connection отвечает за обработку сообщений от удалённой стороны, и отсылку сообщений с
        /// нашей стороны.
        /// </summary>
        private IConnection connection;
        private CreateTcpClientFunc createTcpClientFunc;

        private readonly bool autoReconnect;

        public int MaxReconnectAttemp = 10;
        private int connectionAttemp = 0;

        public Client(T connection, CreateTcpClientFunc createTcpClientFunc, bool autoReconnect)
        {
            this.createTcpClientFunc = createTcpClientFunc;
            this.connection = connection;
            this.autoReconnect = autoReconnect;
            RunTask();
        }

        /// <summary>   Основной, наиболее часто используемый вариант конструктора. </summary>
        ///
        /// <param name="connection">       Именно connection отвечает за обработку сообщений от
        ///                                 удалённой стороны, и отсылку сообщений с нашей стороны. </param>
        /// <param name="host">             ip адрес хоста. </param>
        /// <param name="port">             порт. </param>
        /// <param name="autoReconnect">    (Optional) использовать автоматическое переподключение. </param>
        public Client(T connection, string host, int port, bool autoReconnect = true) : 
            this(connection, () => new NetworkTcpClient(host, port), autoReconnect)
        { 
        }

        public Client(string host, int port, bool autoReconnect = true) : this(new T(), host, port, autoReconnect)
        {
        }
        
        private void RunTask()
        {
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
            Task.Run(ConnectAsync, cancellationToken).ConfigureAwait(false);
        }

        private async Task ConnectAsync()
        {
            bool AutoRetryNeeded()
            {
                return autoReconnect && !cancellationToken.IsCancellationRequested && connectionAttemp <= MaxReconnectAttemp;
            }

            do
            {
                try
                {
                    connectionAttemp++;
                    using (var client = createTcpClientFunc())
                    {
                        using (var stream = client.GetStream())
                        {
                            connection.CancellationToken = cancellationToken;

                            await connection.MainLoop(stream);
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.Warn($"Client<{typeof(T).Name}> disconnected: {e.Message}");
                    if (AutoRetryNeeded())
                    {
                        await Task.Delay(ReconnectDelayMs);
                    }
                    else
                    {
                        _log.Error("Failed to connect: " + e.Message);
                        OnConnectionFailed(e);
                        return;
                    }
                } 
            } while (AutoRetryNeeded());
        }

        /// <summary>
        /// Отправка сообщения, без ожидания какого-либо ответа от удалённой стороны. Подходит например
        /// для периодической отправки отчетов.
        /// </summary>
        ///
        /// <param name="msg">  Подкласс Message. </param>
        public void SendMessage(Message msg)
        {
            connection.SendMessage(msg);
        }

        /// <summary>
        /// Отправка сообщения с ожиданием ответа от удалённой стороны. Важный момент в реализации -
        /// определение того, что от удалённой стороны получен ответ именно на текущий запрос. Для этого
        /// используется поле Message.MessageId: в ответе на запрос оно будет точно таким же, как в
        /// запросе. Т.о. можно отправить несколько запросов одного или разных типов одновременно, и
        /// каждый из них получит свой ответ.
        /// </summary>
        ///
        /// <typeparam name="TResponse">    Тип ожидаемого ответа. Если может вернуться ответ разных
        ///                                 типов, можно использовать просто базовый тип Message, и смотреть
        ///                                 тип, который пришёл в ответе. </typeparam>
        ///
        /// <param name="message">  запрос, подкласс Message. </param>
        ///
        /// <returns>   ответ типа TResponse (или его подкласса) </returns>
        public async Task<TResponse> SendMessageAsync<TResponse>(Message message) where TResponse : Message
        {
            return await connection.SendMessageAsync<TResponse>(message);
        }

        /// <summary>
        /// Connection содержит логику обработки специфических для данного клиента сообщений, обычно вся
        /// логика запросов и ответов содержится внутри него самого, но иногда бывает нужно получить к
        /// нему доступ извне.
        /// </summary>
        ///
        /// <returns>   The connection. </returns>
        public T Connection
        {
            get { return (T)connection; }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cancellationTokenSource.Cancel();
                    cancellationTokenSource.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
