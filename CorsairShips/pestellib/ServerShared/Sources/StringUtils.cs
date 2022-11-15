using System.Text;

namespace PestelLib.ServerShared
{
    public static class StringUtils
    {
        //http://stackoverflow.com/questions/472906/converting-a-string-to-byte-array-without-using-an-encoding-byte-by-byte
        public static byte[] GetBytes(this string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static byte[] HexToBytes(string hex)
        {
            string fixedHex = hex.Replace("-", string.Empty);

            // array to put the result in
            byte[] bytes = new byte[fixedHex.Length / 2];
            // variable to determine shift of high/low nibble
            int shift = 4;
            // offset of the current byte in the array
            int offset = 0;
            // loop the characters in the string
            foreach (char c in fixedHex)
            {
                // get character code in range 0-9, 17-22
                // the % 32 handles lower case characters
                int b = (c - '0') % 32;
                // correction for a-f
                if (b > 9) b -= 7;
                // store nibble (4 bits) in byte array
                bytes[offset] |= (byte)(b << shift);
                // toggle the shift variable between 0 and 4
                shift ^= 4;
                // move to next byte
                if (shift != 0) offset++;
            }
            return bytes;
        }

        public static string ArrayToHex(byte[] data)
        {
            StringBuilder hex = new StringBuilder(data.Length * 2);
            foreach (byte b in data)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

    }
}
