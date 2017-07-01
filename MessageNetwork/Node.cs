using MessageNetwork.Messages;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageNetwork
{
    class Node<T>
        where T : CastableMessage<T>
    {
        #region Private Fields

        private HashSet<Node<T>> children;
        private NodeSession<T> session;

        #endregion Private Fields

        #region Public Constructors

        public Node(NodeSession<T> nodeSession)
            : this(nodeSession.ReceivedPublicKey)
        {
            session = nodeSession;
        }

        public Node(RsaKeyParameters publicKey)
        {
            children = new HashSet<Node<T>>();
            PublicKey = publicKey;
        }

        #endregion Public Constructors

        #region Public Properties

        public IEnumerable<Node<T>> Children { get { return children; } }
        public Node<T> Parent { get; private set; }
        public RsaKeyParameters PublicKey { get; private set; }
        public NodeSession<T> Session
        {
            get
            {
                if(session != null)
                {
                    return session;
                }
                else
                {
                    if(Parent != null)
                    {
                        return Parent.Session;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void AddChild(Node<T> node)
        {
            children.Add(node);
            node.Parent = this;
        }

        public Node<T> Find(RsaKeyParameters publicKey)
        {
            return GetAllChildren().FirstOrDefault(o => o.PublicKey.Equals(publicKey));
        }

        public IEnumerable<Node<T>> GetAllChildren(bool topDown = true, bool excludeSelf = false)
        {
            var ret = new List<Node<T>>();
            if(topDown && !excludeSelf)
            {
                ret.Add(this);
            }
            foreach(var c in Children)
            {
                ret.AddRange(c.GetAllChildren(topDown));
            }
            if(!topDown && !excludeSelf)
            {
                ret.Add(this);
            }
            return ret;
        }

        public void RemoveChild(Node<T> node)
        {
            children.Remove(node);
            node.Parent = null;
        }

        public void Remove()
        {
            Parent?.RemoveChild(this);
        }

        #endregion Public Methods
    }
}
