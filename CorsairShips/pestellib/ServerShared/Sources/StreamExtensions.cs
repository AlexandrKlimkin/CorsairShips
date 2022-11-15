using System.IO;

namespace ServerShared
{
    public static class StreamExtensions
    {
        public static byte[] ReadAll(this Stream stream)
        {
            var buffer = new byte[1024];
            using (var ms = new MemoryStream())
            {
                while (true)
                {
                    var read = stream.Read(buffer, 0, buffer.Length);
                    if (read == 0)
                        return ms.GetBuffer();
                    ms.Write(buffer, 0, read);
                }
            }
        }
    }
}
