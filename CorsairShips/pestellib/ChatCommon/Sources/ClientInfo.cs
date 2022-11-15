//using ProtoBuf;
using System;
using Lidgren.Network;

namespace PestelLib.ChatCommon
{
#pragma warning disable 612
    // нельзя обфусцировать используется как JSON
    [System.Reflection.Obfuscation(Exclude = true)]
    public class ClientInfo
    {
        [Obsolete]
        public byte[] Id;       // old chat support
        public string Name;
        public string Token;
        public byte[] AuthData; // e.g. some constant secret (PlayerId is less secure but can be used)
        public Guid PlayerId;

        public ClientInfo Copy()
        {
            byte[] id_copy = null;
            if (Id != null)
            {
                id_copy = new byte[Id.Length];
                Array.Copy(Id, id_copy, Id.Length);
            }

            return new ClientInfo()
            {
                Id = id_copy,
                Name = Name,
                Token = Token,
                PlayerId = PlayerId
            };
        }

        public static ClientInfo Create(string token, string name = null)
        {
            var result = new ClientInfo();
            result.Token = token;
            result.Name = name;
            return result;
        }

        public override int GetHashCode()
        {
            return Token.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var ci = obj as ClientInfo;
            if (obj == null)
                return false;
            return ci.Token == Token;
        }
    }
#pragma warning restore 612
}
