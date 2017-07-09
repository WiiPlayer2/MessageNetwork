using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageNetwork.Messages
{
    [Serializable]
    class NodeJoinedMessage : SystemMessage
    {
        public override SystemMessageType Type
        {
            get
            {
                return SystemMessageType.NodeJoined;
            }

            protected set
            {
                base.Type = value;
            }
        }

        public PublicKey PublicKey { get; set; }

        public PublicKey ParentPublicKey { get; set; }
    }
}
