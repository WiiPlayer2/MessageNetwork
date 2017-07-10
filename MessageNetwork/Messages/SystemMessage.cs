using System;

namespace MessageNetwork.Messages
{
    [Serializable]
    public abstract class SystemMessage : CastableMessage<SystemMessage>
    {
        public virtual SystemMessageType Type { get; protected set; }
    }
}