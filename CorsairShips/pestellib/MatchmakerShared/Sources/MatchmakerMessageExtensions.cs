
namespace PestelLib.MatchmakerShared
{
    public static class MatchmakerMessageExtensions
    {
        private static readonly MatchmakerMessageSerializer _serializer = new MatchmakerMessageSerializer();

        public static byte[] Serialize(this MatchmakerMessage message)
        {
            return _serializer.Serialize(message);
        }

        public static T Deserialize<T>(byte[] d, int len) where T : MatchmakerMessage
        {
            return _serializer.Deserialize(d, len) as T;
        }
    }
}
