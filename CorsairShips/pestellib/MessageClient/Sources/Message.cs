using MessagePack;

namespace MessageServer.Sources
{
    [MessagePackObject()]
    public struct Message
    {
        [Key(0)]
        public int Type;
        [Key(1)]
        public byte[] Data;
        [Key(2)]
        public int Tag;
        [Key(3)]
        public bool Answer;
    }

    public interface IAnswerContext
    {
        bool AnswerSent { get; }
        bool Answer(int type, byte[] data);
    }

    public struct MessageFrom
    {
        public Message Message;
        public int Sender;
        public IAnswerContext AnswerContext;
    }
}
