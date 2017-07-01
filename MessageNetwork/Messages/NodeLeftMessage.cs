using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageNetwork.Messages
{
    class NodeLeftMessage : SystemMessage
    {
        public override SystemMessageType Type
        {
            get
            {
                return SystemMessageType.NodeLeft;
            }

            protected set
            {
                base.Type = value;
            }
        }

        public PublicKey PublicKey { get; set; }
    }
}
