using MessageNetwork.Messages;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessageNetwork
{
    public class MessageNode<T>
    {
        private AsymmetricCipherKeyPair keyPair;
        private Node<T> rootNode;

        private TcpListener tcpListener;
        private Thread acceptThread;
        private TcpClient tcpClient;

        public MessageNode(AsymmetricCipherKeyPair keyPair)
        {
            if (keyPair == null)
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
                if (node != null)
                {
                    node.Session.SendMessage(receiver, message, payload);
                }
            }
            else
            {
                foreach (var node in rootNode.Children)
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

        public void Setup()
        {
            if (tcpListener != null && acceptThread != null)
            {
                tcpListener.Start();
                acceptThread = new Thread(AcceptLoop);
                acceptThread.Start();
            }
        }

        private void AcceptLoop()
        {
            while(true)
            {
                var client = tcpListener.AcceptTcpClient();
                var cryptStream = new CryptedStream(client.GetStream(), keyPair);
                if(cryptStream.Setup(key =>
                {
                    throw new NotImplementedException();
                }))
                {
                    var session = new NodeSession<T>(cryptStream);
                    session.InternalExceptionOccured += Session_InternalExceptionOccured;
                    session.RawMessageReceived += Session_RawMessageReceived;

                    var node = new Node<T>(session);
                    rootNode.AddChild(node);
                }
            }
        }

        public void Setup(string host, int port)
        {
            Setup();

            tcpClient = new TcpClient();
            tcpClient.Connect(host, port);
            var cryptStream = new CryptedStream(tcpClient.GetStream(), keyPair);
            if (cryptStream.Setup())
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
            if (message.Receiver != null)
            {
                if (message.Receiver.Equals(keyPair.Public))
                {
                    HandleMessage(message);
                }
                else
                {
                    rootNode.Find(message.Receiver).Session.SendMessage(message, payload);
                }
            }
            else
            {
                foreach (var node in rootNode.Children.Where(o => !o.PublicKey.Equals(sender.ReceivedPublicKey)))
                {
                    node.Session.SendMessage(message, payload);
                }
                HandleMessage(message);
            }
            throw new NotImplementedException();
        }

        private void HandleMessage(NodeMessage<T> msg)
        {
            throw new NotImplementedException();
        }

        private void Session_InternalExceptionOccured(object sender, MessageNetworkException e)
        {
            var session = sender as NodeSession<T>;

            throw new NotImplementedException();
        }
    }
}
