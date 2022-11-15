namespace MessageServer.Sources
{
    public interface IMessageSender
    {
        bool IsValid { get; }

        /// <summary>
        /// Send message w/o intent to receive answer
        /// </summary>
        /// <param name="target"></param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        void Notify(int target, int type, byte[] data);
        void Notify(int[] targets, int type, byte[] data);

        /// <summary>
        /// Send message with intent to receive answer
        /// </summary>
        /// <param name="target"></param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <param name="answerHandler"></param>
        int Request(int target, int type, byte[] data, IMessageHandler answerHandler);
    }
}
