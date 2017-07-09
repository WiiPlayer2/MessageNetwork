using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageNetwork
{
    public class PublicKey
    {
        public byte[] ModulusBytes { get; set; }

        public byte[] ExponentBytes { get; set; }

        public override string ToString()
        {
            return $"{string.Concat(ModulusBytes.Select(o => o.ToString("X2")))}:{string.Concat(ExponentBytes.Select(o => o.ToString("X2")))}"
        }

        public static PublicKey Parse(string str)
        {
            var split = str.Split(':');
            var modBytes = new byte[split[0].Length / 2];
            var expBytes = new byte[split[1].Length / 2];

            for(var i = 0; i < modBytes.Length; i++)
            {
                modBytes[i] = byte.Parse(split[0].Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }
            for (var i = 0; i < expBytes.Length; i++)
            {
                expBytes[i] = byte.Parse(split[1].Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }

            return new PublicKey()
            {
                ModulusBytes = modBytes,
                ExponentBytes = expBytes,
            };
        }

        public static bool operator ==(PublicKey key1, PublicKey key2)
        {
            return key1?.ToString() == key2?.ToString();
        }

        public static bool operator !=(PublicKey key1, PublicKey key2)
        {
            return !(key1 == key2);
        }

        public override bool Equals(object obj)
        {
            if(obj is PublicKey)
            {
                return this == (PublicKey)obj;
            }
            if(obj is RsaKeyParameters)
            {
                return this == (PublicKey)(RsaKeyParameters)obj;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static implicit operator RsaKeyParameters(PublicKey key)
        {
            if (key == null)
            {
                return null;
            }
            return new RsaKeyParameters(false, new BigInteger(key.ModulusBytes), new BigInteger(key.ExponentBytes));
        }

        public static implicit operator PublicKey(RsaKeyParameters key)
        {
            if (key == null)
            {
                return null;
            }
            return new PublicKey()
            {
                ModulusBytes = key.Modulus.ToByteArray(),
                ExponentBytes = key.Exponent.ToByteArray(),
            };
        }
    }
}
