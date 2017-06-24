using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageNetwork.Messages
{
    public abstract class SystemMessage
    {
        public abstract SystemMessageType Type { get; }
    }
}
