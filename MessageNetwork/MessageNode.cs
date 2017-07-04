using MessageNetwork.Messages;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MessageNetwork
{
    public class MessageNode<T>
        where T : CastableMessage<T>
    {
        #region Private Fields

        private Thread acceptThread;

        private AsymmetricCipherKeyPair keyPair;

        private Node<T> rootNode;

        private Node<T> upstreamNode;

        private TcpClient tcpClient;

        private TcpListener tcpListener;

        #endregion Private Fields

        #region Public Constructors

        public MessageNode(AsymmetricCipherKeyPair keyPair)
        {
            if (keyPair == null)
            {
                throw new ArgumentNullException(nameof(keyPair));
            }

            this.keyPair = keyPair;
            rootNode = new Node<T>(keyPair.Public as RsaKeyParameters);
        }

        public MessageNode(AsymmetricCipherKeyPair keyPair, IPAddress localaddr, int port)
            : this(keyPair)
        {
            tcpListener = new TcpListener(localaddr, port);
        }

        #endregion Public Constructors

        #region Public Delegates

        public delegate void NodeJoinedEventHandler(MessageNode<T> sender, RsaKeyParameters publicKey);

        public delegate void NodeLeftEventHandler(MessageNode<T> sender, RsaKeyParameters publicKey);

        public delegate void MessageReceivedEventHandler(MessageNode<T> sender, RsaKeyParameters senderKey, bool isPublic, T message, byte[] payload);

        public delegate void DisconnectedEventHandler(MessageNode<T> sender);

        #endregion Public Delegates

        #region Public Events

        public event NodeJoinedEventHandler NodeJoined;

        public event NodeLeftEventHandler NodeLeft;

        public event MessageReceivedEventHandler MessageReceived;

        public event DisconnectedEventHandler Disconnected;

        #endregion Public Events

        #region Public Properties

        public TrustedKeyStore TrustedKeys { get; set; }

        #endregion Public Properties

        #region Public Methods

        public void SendMessage(RsaKeyParameters receiver, T message, byte[] payload = null)
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

        public void Setup()
        {
            if (TrustedKeys == null)
            {
                TrustedKeys = new TrustedKeyStore();
            }
            if (tcpListener != null && acceptThread == null)
            {
                tcpListener.Start();
                acceptThread = new Thread(AcceptLoop);
                acceptThread.Start();
            }
        }

        public bool Setup(string host, int port)
        {
            Setup();

            if (tcpClient == null)
            {
                try
                {
                    tcpClient = new TcpClient();
                    tcpClient.Connect(host, port);
                    var cryptStream = new CryptedStream(tcpClient.GetStream(), keyPair);
                    if (cryptStream.Setup())
                    {
                        upstreamNode = SetupNode(cryptStream);
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }

                tcpClient.Close();
                tcpClient = null;
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void AcceptLoop()
        {
            while (true)
            {
                var client = tcpListener.AcceptTcpClient();
                var cryptStream = new CryptedStream(client.GetStream(), keyPair);
                if (cryptStream.Setup(key => TrustedKeys.Contains(key)))
                {
                    SetupNode(cryptStream);
                }
            }
        }

        private void HandleMessage(NodeMessage<T> msg, byte[] payload)
        {
            if (msg.IsSystemMessage)
            {
                var sys = msg.SystemMessage;
                switch (sys.Type)
                {
                    case SystemMessageType.NodeLeft:
                        {
                            var node = rootNode.Find(sys.Cast<NodeLeftMessage>().PublicKey);
                            node.Remove();
                            CallNodeLeft(node.PublicKey);
                            break;
                        }
                    case SystemMessageType.NodeJoined:
                        {
                            var joinMsg = sys.Cast<NodeJoinedMessage>();
                            var parent = rootNode.Find(joinMsg.ParentPublicKey);
                            var node = new Node<T>(joinMsg.PublicKey);
                            parent.AddChild(node);
                            CallNodeJoined(node.PublicKey);
                            break;
                        }
                }
            }
            else
            {
                CallMessageReceived(msg.Sender, msg.Receiver == null, msg.Message, payload);
            }
        }

        private void Session_InternalExceptionOccured(object sender, MessageNetworkException e)
        {
            var session = sender as NodeSession<T>;
            var node = rootNode.Children.SingleOrDefault(o => o.Session == session);

            if (node != null)
            {
                node.Remove();
                var leftMsg = new NodeLeftMessage()
                {
                    PublicKey = node.PublicKey,
                };
                foreach (var n in rootNode.Children)
                {
                    n.Session.SendMessage(null, leftMsg, null);
                }

                CallNodeLeft(node.PublicKey);

                if (node == upstreamNode)
                {
                    tcpClient.Close();
                    tcpClient = null;
                    upstreamNode = null;

                    CallDisconnected();
                }
            }
        }

        private void Session_RawMessageReceived(NodeSession<T> sender, NodeMessage<T> message, byte[] payload)
        {
            if (message.Receiver != null)
            {
                if (message.Receiver.Equals(keyPair.Public))
                {
                    HandleMessage(message, payload);
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

                HandleMessage(message, payload);
            }
        }

        private Node<T> SetupNode(CryptedStream cryptStream)
        {
            var session = new NodeSession<T>(cryptStream);
            session.InternalExceptionOccured += Session_InternalExceptionOccured;
            session.RawMessageReceived += Session_RawMessageReceived;

            var joinMsg = new NodeJoinedMessage()
            {
                ParentPublicKey = rootNode.PublicKey,
                PublicKey = session.ReceivedPublicKey,
            };
            foreach (var n in rootNode.Children)
            {
                n.Session.SendMessage(null, joinMsg, null);
            }

            foreach (var n in rootNode.GetAllChildren(true, true))
            {
                session.SendMessage(null, new NodeJoinedMessage()
                {
                    ParentPublicKey = n.Parent.PublicKey,
                    PublicKey = n.PublicKey,
                }, null);
            }

            var node = new Node<T>(session);
            rootNode.AddChild(node);
            session.StartReadLoop();

            CallNodeJoined(node.PublicKey);

            return node;
        }

        private void CallNodeJoined(RsaKeyParameters publicKey)
        {
            if (NodeJoined != null)
            {
                foreach (var d in NodeJoined.GetInvocationList())
                {
                    try
                    {
                        (d as NodeJoinedEventHandler)(this, publicKey);
                    }
                    catch { }
                }
            }
        }

        private void CallNodeLeft(RsaKeyParameters publicKey)
        {
            if (NodeLeft != null)
            {
                foreach (var d in NodeLeft.GetInvocationList())
                {
                    try
                    {
                        (d as NodeLeftEventHandler)(this, publicKey);
                    }
                    catch { }
                }
            }
        }

        private void CallMessageReceived(RsaKeyParameters senderKey, bool isPublic, T message, byte[] payload)
        {
            if (MessageReceived != null)
            {
                foreach (var d in MessageReceived.GetInvocationList())
                {
                    try
                    {
                        (d as MessageReceivedEventHandler)(this, senderKey, isPublic, message, payload);
                    }
                    catch { }
                }
            }
        }

        private void CallDisconnected()
        {
            if (Disconnected != null)
            {
                foreach (var d in Disconnected.GetInvocationList())
                {
                    try
                    {
                        (d as DisconnectedEventHandler)(this);
                    }
                    catch { }
                }
            }
        }

        #endregion Private Methods
    }
}