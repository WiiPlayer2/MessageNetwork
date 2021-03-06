﻿using Newtonsoft.Json.Linq;
using System;

namespace MessageNetwork.Messages
{
    [Serializable]
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