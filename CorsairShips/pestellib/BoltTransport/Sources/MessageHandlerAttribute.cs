using System;

namespace BoltTransport
{
    /// <summary>
    /// Для того, чтобы написать обработку сообщения, нужно создать публичный метод, который будет
    /// принимать сообщение нужного вам класса и пометить его атрибутом MessageHandler. Если
    /// требуется что-то послать в ответ - можно добавить у этого метода возвращаемое значение,
    /// например:
    /// 
    /// <code>[MessageHandler]</code>
    /// 
    /// <code> public async Task&lt;CreateGameResponse&gt;CreateGameRequest(CreateGameRequest createGameRequest)</code>
    /// </summary>
    public class MessageHandlerAttribute : Attribute
    {
    }
}