using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageNetwork
{
    public class PublicKey
    {
        public static implicit operator RsaKeyParameters(PublicKey key)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PublicKey(RsaKeyParameters key)
        {
            throw new NotImplementedException();
        }
    }
}
