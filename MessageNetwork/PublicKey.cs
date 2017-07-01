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

        public static implicit operator RsaKeyParameters(PublicKey key)
        {
            if(key == null)
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
