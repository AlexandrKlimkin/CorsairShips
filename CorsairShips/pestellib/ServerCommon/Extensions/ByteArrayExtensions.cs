using System.Linq;

namespace PestelLib.ServerCommon.Extensions
{
    public static class ByteArrayExtensions
    {
        public static string ToHex(this byte[] array)
        {
            if (array == null)
                return "<null>";
            return string.Join(":", array.Select(_ => _.ToString("x2")));
        }
    }
}
