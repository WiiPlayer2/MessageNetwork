using System;

namespace MessageNetwork.Messages
{
    [Serializable]
    internal class NodeLeftMessage : SystemMessage
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