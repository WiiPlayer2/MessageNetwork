using System;

namespace MessageNetwork
{
    [Serializable]
    public class MessageNetworkException : Exception
    {
        public MessageNetworkException()
        {
        }

        public MessageNetworkException(string message) : base(message)
        {
        }

        public MessageNetworkException(string message, Exception inner) : base(message, inner)
        {
        }

        protected MessageNetworkException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}