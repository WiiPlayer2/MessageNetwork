using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageNetwork.Messages
{
    public class NodeMessage<T>
        where T : CastableMessage<T>
    {
        #region Public Properties

        public bool IsSystemMessage { get; internal set; }

        [JsonIgnore]
        public T Message { get; set; }

        [JsonProperty("Message")]
        public JObject MessageJson
        {
            get
            {
                if(Message == null)
                {
                    return null;
                }
                return JObject.FromObject(Message);
            }
            set
            {
                if(value == null)
                {
                    Message = null;
                    return;
                }
                Message = value.ToObject<T>();
                Message.JObject = value;
            }
        }

        public int? PayloadLength { get; set; }

        public PublicKey Receiver { get; set; }

        public PublicKey Sender { get; set; }

        [JsonIgnore]
        public SystemMessage SystemMessage { get; internal set; }

        [JsonProperty("SystemMessage")]
        public JObject SystemMessageJson
        {
            get
            {
                if(SystemMessage == null)
                {
                    return null;
                }
                return JObject.FromObject(SystemMessage);
            }
            set
            {
                if(value == null)
                {
                    SystemMessage = null;
                    return;
                }
                SystemMessage = value.ToObject<SystemMessage>();
                SystemMessage.JObject = value;
            }
        }

        #endregion Public Properties
    }
}
