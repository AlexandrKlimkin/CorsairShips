using System.Security.Cryptography;

namespace PestelLib.ServerShared
{
    public class Md5
    {
        static public byte[] MD5byteArray(byte[] data)
        {
            var md5 = MD5.Create();
            return md5.ComputeHash(data);
        }

        static public string MD5string(byte[] data)
        {
            string strHash = string.Empty;
            var md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(data);
            foreach (byte b in hash)
            {
                strHash += b.ToString("x2");
            }
            return strHash;
        }
    }
}