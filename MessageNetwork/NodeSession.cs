using MessageNetwork.Messages;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.IO;
using System.Threading;

namespace MessageNetwork
{
    internal class NodeSession<T>
        where T : CastableMessage<T>
    {
        private CryptedStream baseStream;
        private BinaryWriter bWriter;
        private BinaryReader bReader;

        private Thread receiveThread;

        internal NodeSession(CryptedStream stream)
        {
            baseStream = stream;
            bWriter = new BinaryWriter(stream);
            bReader = new BinaryReader(stream);

            receiveThread = new Thread(ReceiveLoop);
        }

        private void ReceiveLoop()
        {
            try
            {
                while (true)
                {
                    var json = bReader.ReadString();
                    var msg = JsonConvert.DeserializeObject<NodeMessage<T>>(json);
                    byte[] payload = null;

                    if (msg.PayloadLength.HasValue)
                    {
                        payload = bReader.ReadBytes(msg.PayloadLength.Value);
                    }

                    try
                    {
                        RawMessageReceived(this, msg, payload);
                    }
                    catch { }
                }
            }
            catch (Exception e)
            {
                InternalExceptionOccured(this, new MessageNetworkException("Error while receiving a message.", e));
            }
            finally
            {
                baseStream.Close();
            }
        }

        public void StartReadLoop()
        {
            receiveThread.Start();
        }

        public RsaKeyParameters ReceivedPublicKey
        {
            get
            {
                return baseStream.ReceivedPublicKey;
            }
        }

        public delegate void RawMessageReceivedEventHandler(NodeSession<T> sender, NodeMessage<T> message, byte[] payload);

        public event RawMessageReceivedEventHandler RawMessageReceived = (_, __, ___) => { };

        public event EventHandler<MessageNetworkException> InternalExceptionOccured = (_, __) => { };

        public void SendMessage(NodeMessage<T> msg, byte[] payload)
        {
            var json = JsonConvert.SerializeObject(msg);

            try
            {
                lock (bWriter)
                {
                    bWriter.Write(json);
                    if (payload != null)
                    {
                        bWriter.Write(payload);
                    }
                    bWriter.Flush();
                }
            }
            catch (Exception e)
            {
                InternalExceptionOccured(this, new MessageNetworkException("Error while sending a message.", e));
            }
        }

        public void SendMessage(RsaKeyParameters receiver, T message, byte[] payload)
        {
            var msg = new NodeMessage<T>()
            {
                Sender = baseStream.LocalPublicKey,
                Receiver = receiver,
                IsSystemMessage = false,
                Message = message,
                PayloadLength = payload?.Length,
            };
            SendMessage(msg, payload);
        }

        internal void SendMessage(RsaKeyParameters receiver, SystemMessage message, byte[] payload)
        {
            var msg = new NodeMessage<T>()
            {
                Sender = baseStream.LocalPublicKey,
                Receiver = receiver,
                IsSystemMessage = true,
                PayloadLength = payload?.Length,
                SystemMessage = message,
            };
            SendMessage(msg, payload);
        }
    }
}