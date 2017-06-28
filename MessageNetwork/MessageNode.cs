using MessageNetwork.Messages;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessageNetwork
{
    public class MessageNode<T>
    {
        private AsymmetricCipherKeyPair keyPair;
        private Node<T> rootNode;

        private TcpListener tcpListener;
        private TcpClient tcpClient;

        public MessageNode(AsymmetricCipherKeyPair keyPair)
        {
            if(keyPair == null)
            {
                throw new ArgumentNullException(nameof(keyPair));
            }

            this.keyPair = keyPair;
        }

        public MessageNode(AsymmetricCipherKeyPair keyPair, IPAddress localaddr, int port)
            : this(keyPair)
        {
            tcpListener = new TcpListener(localaddr, port);
        }

        public void SendMessage(RsaKeyParameters receiver, T message, byte[] payload)
        {
            //TODO: Lock tree
            if (receiver != null)
            {
                var node = rootNode.Find(receiver);
                if(node != null)
                {
                    node.Session.SendMessage(receiver, message, payload);
                }
            }
            else
            {
                foreach(var node in rootNode.Children)
                {
                    node.Session.SendMessage(null, message, payload);
                }
            }
        }

        public void AddTrustedPublicKey(RsaKeyParameters publicKey)
        {
            throw new NotImplementedException();
        }

        public void RemoveTrustedPublicKey(RsaKeyParameters publicKey)
        {
            throw new NotImplementedException();
        }

        public void ClearTrustedPublicKey(RsaKeyParameters publicKey)
        {
            throw new NotImplementedException();
        }

        public void StartServer()
        {
            throw new NotImplementedException();
        }

        public void ConnectToServer(string host, int port)
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(host, port);
            var cryptStream = new CryptedStream(tcpClient.GetStream(), keyPair);
            if(cryptStream.Setup())
            {
                var session = new NodeSession<T>(cryptStream);
                session.InternalExceptionOccured += Session_InternalExceptionOccured;
                session.RawMessageReceived += Session_RawMessageReceived;

                var node = new Node<T>(session);
                rootNode.AddChild(node);
            }
        }

        private void Session_RawMessageReceived(NodeSession<T> sender, NodeMessage<T> message, byte[] payload)
        {
            if(message.Receiver != null)
            {
                if(message.Receiver.Equals(keyPair.Public))
                {
                    if (message.IsSystemMessage)
                    {
                        HandleSystemMessage(message.SystemMessage);
                    }
                    //TODO: Handle message event
                }
                else
                {
                    rootNode.Find(message.Receiver).Session.SendMessage(message, payload);
                }
            }
            else
            {
                if(message.IsSystemMessage)
                {
                    HandleSystemMessage(message.SystemMessage);
                }

                foreach(var node in rootNode.Children.Where(o => !o.PublicKey.Equals(sender.ReceivedPublicKey)))
                {
                    node.Session.SendMessage(message, payload);
                }
            }
            throw new NotImplementedException();
        }

        private void HandleSystemMessage(SystemMessage msg)
        {

        }

        private void Session_InternalExceptionOccured(object sender, MessageNetworkException e)
        {
            var session = sender as NodeSession<T>;

            throw new NotImplementedException();
        }
    }
}
