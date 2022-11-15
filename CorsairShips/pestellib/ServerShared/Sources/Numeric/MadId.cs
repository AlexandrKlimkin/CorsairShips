using System;
using System.Text;
using MessagePack;
using ServerShared.Sources.Numeric;

namespace S
{
    public enum MadIdMode
    {
        Default,
        Mixed
    }

    [MessagePackObject()]
    [MessagePackFormatter(typeof(MadIdFormatter))]
    public struct MadId
    {
        private uint _data;
        private string _stringValue;

        public const string _alph = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly uint _radix = (uint)_alph.Length;

        public const int MaxDigits = 6;
        public static readonly MadId MaxValue = new MadId(2176782335);
        public static readonly MadId MinValue = new MadId(0);
        public static readonly MadId Zero = new MadId(0);

        private static readonly uint[] _offsetsMixed = { 16u, 13u, 12u, 20u, 25u, 30u };
        private static readonly uint[] _offsetsNormal = { 0u, 0u, 0u, 0u, 0u, 0u };
        private static uint[] _offsets = _offsetsNormal;

        public static MadIdMode Mode {
            get
            {
                if (ReferenceEquals(_offsets, _offsetsNormal))
                    return MadIdMode.Default;
                return MadIdMode.Mixed;
            }
            set
            {
                if (value == MadIdMode.Default)
                    _offsets = _offsetsNormal;
                else
                    _offsets = _offsetsMixed;
            }
        }

        public MadId(uint num)
        {
            _data = num;
            _stringValue = "";
        }

        public static bool TryParse(string val, out MadId id)
        {
            try
            {
                id = new MadId(val);
                return true;
            }
            catch
            {
                id = new MadId();
            }

            return false;
        }

        public MadId(string num)
        {
            num = num.ToUpperInvariant();
            // format 000-000
            if (num.Length == 7 && num[3] == '-')
            {
                num =
                    num.Substring(0, 3)
                    + num.Substring(4, 3);
            }

            // format 000000
            if (num.Length != 6)
                throw new Exception("Unsupported format");
            var koef = 1U;
            var result = 0U;

            for (var i = num.Length - 1; i >= 0; --i)
            {
                var digit = (uint)num[i];
                var offset = _offsets[i];
                if (digit < 0x3A)
                    digit -= 0x30;
                else if (digit > 0x40 && digit < 0x5B)
                    digit -= 0x37;
                else
                    throw new Exception($"Unsupported symbol at {i} in : '{num}'");
                if (digit < offset)
                    digit += _radix;
                digit -= offset;
                result += digit * koef;
                koef *= 36;
            }
            _data = result;
            _stringValue = num.Substring(0, 3) + "-" + num.Substring(3, 3);
        }

        public override int GetHashCode()
        {
            return _data.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is MadId id)
                return id._data == _data;
            return false;
        }

        public static implicit operator uint(MadId id)
        {
            return id._data;
        }

        public static implicit operator string(MadId id)
        {
            return id.ToString();
        }

        public static bool operator ==(MadId l, MadId r)
        {
            return l._data == r._data;
        }

        public static bool operator !=(MadId l, MadId r)
        {
            return l._data != r._data;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(_stringValue))
            {
                var num = _data;
                StringBuilder sb = new StringBuilder(MaxDigits);
                sb.Append("000000");
                var pos = 5;
                uint offset;
                while (num >= _radix)
                {
                    var result = num / _radix;
                    var digit = num - result * _radix;
                    offset = (_offsets[pos] + digit) % _radix;
                    sb[pos] = _alph[(int)offset];
                    --pos;
                    num = result;
                }
                offset = (_offsets[pos] + num) % _radix;
                sb[pos] = _alph[(int)offset];
                while (--pos > -1)
                {
                    offset = (_offsets[pos]) % _radix;
                    sb[pos] = _alph[(int)offset];
                }

                sb.Insert(3, '-');
                _stringValue = sb.ToString();
            }

            return _stringValue;
        }
    }
}
