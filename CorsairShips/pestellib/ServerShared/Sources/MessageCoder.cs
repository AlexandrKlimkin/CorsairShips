using System;
using System.Linq;
using System.Text;

namespace PestelLib.ServerShared
{
    public static class MessageCoder
    {
        public const string AuthKey = "4a5cd9bfb49867278ca1406d4d0a5f88";
        public const int SignatureLength = 16;

        public static string SignString(string str)
        {
            if (str == null)
                return null;
            var sign = SignStringImpl(str);
            return str + ":" + sign;
        }

        public static bool CheckStringSignature(string str, out string originalString)
        {
            var signSepIdx = str.LastIndexOf(':');
            originalString = string.Empty;
            if (signSepIdx < 0)
                return false;
            originalString = str.Substring(0, signSepIdx);
            var strSign = str.Substring(signSepIdx + 1);
            var sign = SignStringImpl(originalString);
            if (sign != strSign)
                return false;
            return true;
        }

        public static byte[] AddSignature(byte[] data)
        {
            byte[] authBytes = ConvertHexStringToByteArray(AuthKey);

            byte[] authAndData = new byte[authBytes.Length + data.Length];
            authBytes.CopyTo(authAndData, 0);
            data.CopyTo(authAndData, authBytes.Length);

            byte[] signature = Md5.MD5byteArray(authAndData);

            byte[] requestData = new byte[signature.Length + data.Length];
            signature.CopyTo(requestData, 0);
            data.CopyTo(requestData, signature.Length);
            
            return requestData;
        }

        public static byte[] GetData(byte[] signedData)
        {
            if (signedData.Length <= SignatureLength)
            {
                return new byte[0];
            }

            var result = new byte[signedData.Length - SignatureLength];
            Array.Copy(signedData, SignatureLength, result, 0, result.Length);
            return result;
        }

        public static bool CheckSignature(byte[] signedData)
        {
            byte[] sign = new byte[SignatureLength];
            Array.Copy(signedData, 0, sign, 0, sign.Length);

            byte[] data = new byte[signedData.Length - sign.Length];
            Array.Copy(signedData, sign.Length, data, 0, data.Length);

            byte[] trueSignedData = AddSignature(data);
            byte[] trueSign = new byte[SignatureLength];

            Array.Copy(trueSignedData, 0, trueSign, 0, trueSign.Length);
            return trueSign.SequenceEqual(sign);
        }

        //http://stackoverflow.com/questions/321370/convert-hex-string-to-byte-array
        private static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format("The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] HexAsBytes = new byte[hexString.Length / 2];
            for (int index = 0; index < HexAsBytes.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                HexAsBytes[index] = byte.Parse(byteValue, System.Globalization.NumberStyles.HexNumber);
            }

            return HexAsBytes;
        }

        private static string SignStringImpl(string str)
        {
            return Md5.MD5string(Encoding.UTF8.GetBytes(str + AuthKey));
        }
    }
}
