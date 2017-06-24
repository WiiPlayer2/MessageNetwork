using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageNetwork.Messages
{
    public class NodeMessage<T>
    {
        public RsaKeyParameters Receiver { get; set; }
        public RsaKeyParameters Sender { get; set; }
        public int? PayloadLength { get; set; }
        public bool IsSystemMessage { get; internal set; }
        public T Message { get; set; }
        public SystemMessage SystemMessage { get; internal set; }
    }
}
