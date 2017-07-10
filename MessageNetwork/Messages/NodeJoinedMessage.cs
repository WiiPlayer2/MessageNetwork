using System;

namespace MessageNetwork.Messages
{
    [Serializable]
    internal class NodeJoinedMessage : SystemMessage
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