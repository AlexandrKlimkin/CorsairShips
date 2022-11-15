using System;
using System.Threading.Tasks;

namespace PestelLib.ServerCommon.MessageQueue
{
    /// <summary>
    /// Обработчик заданий очереди.
    /// </summary>
    public interface IWorker
    {
        /// <summary>
        /// Если в какой либо момент становится понятно что сообщение обработать невозможно, 
        /// реализация должна вернуть false, тогда сообщение будет доставлено другим воркерам.
        /// Если реализация обрабатывает сообщение она возвращает true.
        /// Если реализация выкидывает ошибку, сообщение будет доставлено другим воркерам.
        /// Если приложение создавшее воркер падает, сообщение будет доставлено другим воркерам.
        /// </summary>
        Task<bool> ProcessWork(byte[] data);

        /// <summary>
        /// Объект воркера вы не можете уничтожить, т.к. он захвачен классом очереди IWorkerQueue, 
        /// поэтому верните здесь false когда объект потерял актуальность и если он отвязан от объекта IWorkerQueue.
        /// Это предотвратит вызовы <see cref="ProcessWork"/>.
        /// </summary>
        bool Alive { get; }
    }

    /// <summary>
    /// Предполагается что реализации вешаются на одну очередь таким образом, что
    /// Все участники по очереди получают сообщения публикуемые в очереди.
    /// </summary>
    public interface IWorkerQueue
    {
        /// <summary>
        /// Имеет ли смысл использовать <see cref="SendWork"/>.
        /// </summary>
        bool CanSend { get; }
        /// <summary>
        /// Имеет ли смысл использовать ждать вызова <see cref="IWorker.ProcessWork"/>.
        /// </summary>
        bool CanReceive { get; }
        /// <summary>
        /// Должна возвращать false если объект потерял актуальность или связанный воркер потерял актуальность.
        /// </summary>
        bool Alive { get; }
        /// <summary>
        /// Отправляет сообщение в очередь.
        /// Если <see cref="CanSend"/> == false то ничего не произойдет.
        /// </summary>
        /// <param name="data"></param>
        void SendWork(byte[] data);
    }
}
