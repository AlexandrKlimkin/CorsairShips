using BoltMasterServer;
using MasterServerProtocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;

namespace BoltTransport
{
    /// <summary>
    /// В подклассах от Connection описывается высокоуровневое поведение. Причем что для сервера, что
    /// для клиента - они оба используют этот класс. Но конечно для клиента и сервера обычно
    /// используются разные подклассы.
    /// 
    /// Кроме того, у одного клиента всегда один экземпляр класса Connection, у сервера кол-во
    /// экземпляров равно количеству подключенных в данный момент клиентов.
    /// 
    /// Мне кажется название Connection неудачным - вероятно есть более традиционные варианты для
    /// класса с такой функциональностью?
    /// </summary>
    /// 
    public class Connection : IConnection
    {
        private static ILog _log = LogManager.GetLogger(typeof(Connection));

        /// <summary>
        /// Адрес удалённого компьютера. Для клиента он не особенно и нужен: клиент и так знает куда он
        /// подключается, но в некоторых ситуациях он может быть нужен для сервера.
        /// </summary>
        ///
        /// <returns>   The remote IP. </returns>
        public IPAddress RemoteIP { get; set; }

        /// <summary>
        /// Чтобы разорвать существующее соединение, Client и Server используют этот токен.
        /// </summary>
        ///
        /// <returns>   The cancellation token. </returns>
        public CancellationToken CancellationToken { get; set; }
        
        /// <summary>
        /// Мне не хотелось делать lock на сокет на отправку сообщения, вместо этого все сообщения
        /// кладутся в Outbox, оттуда периодически происходит отправка.
        /// 
        /// Если делать lock - то получается будет совсем ничего не сделать, если соединение медленное,
        /// или нужно передать много данных и это будет занимать длительное время.
        /// 
        /// Это же позволяет перепосылать сообщение, если во время отправки произошёл эксепшн: удаление
        /// сообщение из очереди происходит только после успешной отправки.
        /// </summary>
        private readonly ConcurrentQueue<Message> Outbox = new ConcurrentQueue<Message>();

        /// <summary>
        /// SendMessageAsync может послать сообщение и дожидаться ответа. Т.к. сколько ждать ответа
        /// неизвестно, плюс могут приходить какие-то другие с удалённой стороны сообщения, нам нужно
        /// ждать именно сообщения с определённым guid'ом, соответствующим идентификатору отправленного
        /// запроса.
        /// 
        /// В этом Dictionary храняться все запросы, которые требуют ответов, но ещё не пришли от
        /// удалённой стороны.
        /// 
        /// Используется это только в SendMessageAsync - может есть способ сделать это как-то ещё проще?
        /// </summary>
        private readonly ConcurrentDictionary<Guid, Action<Message>> MessageReceivers = new ConcurrentDictionary<Guid, Action<Message>>();

        /// <summary>
        /// Connection при инстанцировании просматривает все свои методы, помеченные атрибутом
        /// "MessageHandler" и добавляет их в этот словарь. Ключ в словаре - это тип сообщения, которое
        /// пришло, и которое нужно обработать.
        /// 
        /// <b>Пример:</b>
        /// 
        /// <code> [MessageHandler]public async Task&lt;MasterConfigurationResponse&gt;MasterConfigurationRequest( MasterConfigurationRequest request)</code>
        /// </summary>
        private Dictionary<Type, MethodInfo> _handlers;
        
        public Connection()
        {
            _handlers = MessageHandlerUtils.GetHandlersFromType(GetType());
        }

        /// <summary>
        /// Отправить сообщение, не ждет отправки, управление вызывающей стороны возвращается сразу же.
        /// Получить ответ через этот метод невозможно, но иногда ответ не нужен - например при
        /// переодической отправке статуса.
        /// </summary>
        ///
        /// <param name="message">  Сообщение, экземпляр подкласса Message. </param>
        public void SendMessage(Message message)
        {
            Outbox.Enqueue(message);
        }

        /// <summary>   Отправить сообщение, ждать пока придёт ответ класса Т. </summary>
        ///
        /// <typeparam name="T">    Класс сообщения-ответа, должен быть подклассом Message. Если могут
        ///                         вернуться ответы разных типов - используейте в качестве T сам тип
        ///                         Message. Это может быть например, если операция может завершится ошибкой,
        ///                         и тогда вернётся класс Error наследованный от Message, вместо нормального
        ///                         ответа. </typeparam>
        ///
        /// <param name="message">  Сообщение, экземпляр подкласса Message. </param>
        ///
        /// <returns>   ответ типа T. </returns>
        public async Task<T> SendMessageAsync<T>(Message message) where T : Message
        {
            return (T)await SendMessageAsync(message);
        }

        /// <summary>
        /// Поскольку lock'ов я не стал делать, приходится периодически проверять, не появилось ли новых
        /// исходящих сообщений, или новых входящих сообщений.
        /// </summary>
        ///
        /// <exception cref="OperationCanceledException">   Thrown when an Operation Canceled error
        ///                                                 condition occurs. </exception>
        ///
        /// <param name="stream">   обычно это NetworkStream, полученный из TcpListener или TcpClient. </param>
        ///
        /// <returns>   An asynchronous result. </returns>
        public async Task MainLoop(Stream stream)
        {
            await OnConnectionEstablished(stream);
            try
            {
                var parallelTask = await Task.WhenAny(
                    ReadIncomingMessagesCycleAsync(stream),
                    WriteOutboxMessagexCycleAsync(stream)
                );
                await parallelTask; //second await is for unwrapping exceptions
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                await OnConnectionLost(e);
            }
        }

        /// <summary>
        /// Это всегда вызывается при установке соединения, можно переопределить в подклассе Connection и
        /// отправить какое-то сообщени удалённой стороне, или ещё что-то сделать.
        /// </summary>
        ///
        /// <param name="stream">   обычно это NetworkStream, полученный из TcpListener или TcpClient. </param>
        ///
        /// <returns>   An asynchronous result. </returns>
        protected virtual async Task OnConnectionEstablished(Stream stream) {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Это вызовется при разрыве соединения, если оно произошло не через cancellationToken, а из-за
        /// каких-либо ошибок. Предназначено для переопределения в дочерних классах.
        /// </summary>
        ///
        /// <param name="reason">   эксепшн, из-за которого соединение было разорвано. </param>
        ///
        /// <returns>   An asynchronous result. </returns>
        protected virtual async Task OnConnectionLost(Exception reason)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Обычно обработка сообщений происходит через методы, помеченные аттрибутом MessageHandler.
        /// Переопределение этого метода в дочернем классе - это альтернативный вариант. В этом случае в
        /// него будут приходить входящие сообщения любых типов.
        /// </summary>
        ///
        /// <param name="message">  Сообщение, экземпляр подкласса Message. </param>
        ///
        /// <returns>   An asynchronous result. </returns>
        protected virtual async Task ProcessMessage(Message message)
        {
            await Task.CompletedTask;
        }

        private async Task ReadIncomingMessagesCycleAsync(Stream stream)
        {
            while (!CancellationToken.IsCancellationRequested)
            {                
                var request = await stream.ReadMessageAsync(CancellationToken);
                //_log.Debug("Received message: " + JsonConvert.SerializeObject(request));

                //если кто-то был подписан на получение этого сообщения - уведомляем его
                if (MessageReceivers.TryRemove(request.MessageId, out var callback))
                {
                    callback(request);
                }

                //ищем в закэшированных раньше методах обработки сообщений подходящий метод
                //для поступившего сообщения и вызываем его
                if (_handlers.TryGetValue(request.GetType(), out var methodInfo))
                {
                    var response = await methodInfo.InvokeAsync(this, request);
                    if (response is Message m)
                    {
                        //если обработчик возвращает не void и не Task, значит мы получили запрос, 
                        //на который удалённая сторона ожидает ответа с таким же MessageId, как и у запроса
                        m.MessageId = request.MessageId;
                        SendMessage(m);
                    }
                }

                await ProcessMessage(request);
            }
        }

        /// <summary>
        /// Периодически проверяем, нет ли данных для отправки, если есть - отправляем.
        /// </summary>
        ///
        /// <param name="stream">   обычно это NetworkStream, полученный из TcpListener или TcpClient. </param>
        ///
        /// <returns>   An asynchronous result. </returns>
        private async Task WriteOutboxMessagexCycleAsync(Stream stream)
        {
            while (!CancellationToken.IsCancellationRequested)
            {
                if (Outbox.TryPeek(out var message))
                {
                    //_log.Debug("Sending message: " + JsonConvert.SerializeObject(message));
                    await stream.WriteMessageAsync(message, CancellationToken);
                    //отдельно Peek и Dequeue т.к. если вылетим из WriteMessageAsync, то сообщение останется в очереди
                    //и будет послано после реконнекта
                    
                    Outbox.TryDequeue(out _); 
                }
                await Task.Delay(30);
            }
        }

        private async Task<Message> SendMessageAsync(Message message)
        {
            /*
             * Есть небольшие сомнения в использовании вот этой конструкции с TaskCompletionSource - 
             * я не нашел другого способа, как заставить асинхронный метод ждать коллбека
             * Может можно это сделать проще и очевиднее чем в этом коде?
             */
            var taskCompletionSource = new TaskCompletionSource<Message>();
            SendMessage(message, (response) => {
                taskCompletionSource.SetResult(response);
            });
            return await taskCompletionSource.Task;
        }

        private void SendMessage(Message message, Action<Message> response)
        {
            MessageReceivers.AddOrUpdate(message.MessageId,
                response,
                (messageId, oldMessage) => { return response; }
            );

            SendMessage(message);
        }
    }
}
