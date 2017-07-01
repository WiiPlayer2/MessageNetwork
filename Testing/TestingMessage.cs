using MessageNetwork.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    class TestingMessage : CastableMessage<TestingMessage>
    {
        public string Text { get; set; }
    }
}
