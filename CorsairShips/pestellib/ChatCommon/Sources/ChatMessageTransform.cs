using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using PestelLib.ChatCommon;
using Unity.IL2CPP.CompilerServices;

namespace ChatCommon
{
#if !ENABLE_TCPCHAT
    public class ChatMessageTransform
    {
        public ChatMessageTransform()
        {
            _rijndael = Rijndael.Create();
            var blockSize = _rijndael.BlockSize;
            _key = Encoding.ASCII.GetBytes("aSqWQ87J" + Consts.Temp);
            _iv = Encoding.ASCII.GetBytes("BMXNKJ9dQFKltT4g");
        }

        public byte[] CommitTransform(byte[] data)
        {
            var enc = _rijndael.CreateEncryptor(_key, _iv);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, enc, CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();
                    return ms.ToArray();
                }
            }
        }

        public byte[] RollbackTransform(byte[] data)
        {
            var result = new byte[data.Length];
            var enc = _rijndael.CreateDecryptor(_key, _iv);
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (CryptoStream cs = new CryptoStream(ms, enc, CryptoStreamMode.Read))
                {
                    var read = cs.Read(result, 0, result.Length);
                    Array.Resize(ref result, read);
                    return result;
                }
            }
        }

        private byte[] _key;
        private byte[] _iv;
        private Rijndael _rijndael;
    }
#else
    public class ChatMessageTransform
    {
        public ChatMessageTransform()
        {
            _xtea = new Xtea(_key);
        }

        public byte[] CommitTransform(byte[] data)
        {
            if (data == null || data.Length < 1)
                return data;
            var outSz = Xtea.GetEncryptedSize(data);
            var outBuffer = new byte[outSz]; 
            _xtea.Encrypt(data, outBuffer);
            return outBuffer;
        }

        public byte[] RollbackTransform(byte[] data)
        {
            if (data == null || data.Length < 1)
                return data;
            var result = new byte[data.Length];
            var bytesDecoded = _xtea.Decrypt(data, result);
            if(bytesDecoded < result.Length)
                Array.Resize(ref result, bytesDecoded);
            return result;
        }

        private Xtea _xtea;
        private static readonly uint[] _key = new uint[] { 0xDC529BD3, 0xDEA8BEE3, 0xC0FFEE42, 0x29211663 };
    }

    public class Xtea
    {
        // key - 4 инта
        public Xtea(uint[] key, uint rounds = 32)
        {
            _rounds = rounds;
            _key = key;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetEncryptedSize(byte[] data)
        {
            // 7 + 4 (место под размер)
            return (data.Length + 11) & 0x7FFFFFF8;
        }

        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        // за размером outData следит вызывающая логика (юзай GetEncryptedSize для определения размера выходных данных по размеру входных)
        public unsafe void Encrypt(byte[] inData, byte[] outData)
        {
            System.Diagnostics.Debug.Assert(outData.Length % 8 == 0);
            System.Diagnostics.Debug.Assert(outData.Length >= GetEncryptedSize(inData));
            var inSz = inData.Length;
            fixed (byte* ptr = outData)
            {
                *(int*)ptr = inSz;
            }

            Buffer.BlockCopy(inData, 0, outData, 4, inData.Length);
            fixed (byte* ptr = outData)
            {
                uint* dataPtr = (uint*)ptr;
                var count = outData.Length >> 2;
                for (int i = 0; i < count; i += 2)
                {
                    Encrypt(_rounds, &dataPtr[i], _key);
                }
            }

            if (!BitConverter.IsLittleEndian)
            {
                fixed (byte* ptr = outData)
                {
                    uint* dataPtr = (uint*)ptr;
                    var count = outData.Length >> 2;
                    for (int i = 0; i < count; ++i)
                    {
                        dataPtr[i] = SpawInt(dataPtr[i]);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint SpawInt(uint num)
        {
            return num >> 24 | num << 24 | (num << 8 & 0x00FF0000) | (num >> 8 & 0x0000FF00);
        }

        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public unsafe int Decrypt(byte[] inData, byte[] outData)
        {
            System.Diagnostics.Debug.Assert(inData.Length % 8 == 0);
            System.Diagnostics.Debug.Assert(outData.Length % 8 == 0);
            System.Diagnostics.Debug.Assert(outData.Length >= inData.Length);
            Buffer.BlockCopy(inData, 0, outData, 0, inData.Length);
            int outSize;
            if (!BitConverter.IsLittleEndian)
            {
                fixed (byte* ptr = outData)
                {
                    uint* dataPtr = (uint*)ptr;
                    var count = outData.Length >> 2;
                    for (int i = 0; i < count; ++i)
                    {
                        dataPtr[i] = SpawInt(dataPtr[i]);
                    }
                }
            }

            fixed (byte* ptr = outData)
            {
                uint* dataPtr = (uint*)ptr;
                var count = outData.Length >> 2;
                for (int i = 0; i < count; i += 2)
                {
                    Decrypt(_rounds, &dataPtr[i], _key);
                }

                outSize = (int)*dataPtr;
            }

            Buffer.BlockCopy(outData, 4, outData, 0, outSize);
            return outSize;
        }

        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        private static unsafe void Encrypt(uint rounds, uint* v, uint[] key)
        {
            uint v0 = v[0], v1 = v[1], sum = 0, delta = 0x9E3779B9;
            for (uint i = 0; i < rounds; i++)
            {
                v0 += (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
                sum += delta;
                v1 += (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
            }
            v[0] = v0;
            v[1] = v1;
        }

        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.NullChecks, false)]
        private static unsafe void Decrypt(uint rounds, uint* v, uint[] key)
        {
            uint v0 = v[0], v1 = v[1], delta = 0x9E3779B9, sum = delta * rounds;
            for (uint i = 0; i < rounds; i++)
            {
                v1 -= (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
                sum -= delta;
                v0 -= (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
            }
            v[0] = v0;
            v[1] = v1;
        }

        private uint _rounds;
        private uint[] _key;

    }

#endif
}
