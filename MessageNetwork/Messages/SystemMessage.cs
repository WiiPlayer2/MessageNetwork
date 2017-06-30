using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageNetwork.Messages
{
    public abstract class SystemMessage : CastableMessage<SystemMessage>
    {
        public virtual SystemMessageType Type { get; protected set; }
    }
}
