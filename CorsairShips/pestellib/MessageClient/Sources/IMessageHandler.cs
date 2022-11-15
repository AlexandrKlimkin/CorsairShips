using System;

namespace MessageServer.Sources
{
    public interface IMessageHandler
    {
        void Handle(int sender, byte[] data, int tag, IAnswerContext answerContext);
        void Error(int tag);
    }
}