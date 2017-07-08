using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageNetwork.Messages
{
    public class CastableMessage<T>
        where T : CastableMessage<T>
    {
        public JObject JObject { get; set; }

        public S Cast<S>()
            where S : T
        {
            if (JObject == null)
            {
                return (S)this;
            }
            else
            {
                var obj = JObject.ToObject<S>();
                obj.JObject = JObject;
                return obj;
            }
        }
    }
}
