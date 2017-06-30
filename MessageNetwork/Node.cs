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
        private HashSet<Node<T>> children;
        private NodeSession<T> session;

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

        public RsaKeyParameters PublicKey { get; private set; }

        public Node<T> Parent { get; private set; }

        public IEnumerable<Node<T>> Children { get { return children; } }

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

        public IEnumerable<Node<T>> GetAllChildren(bool topDown = true)
        {
            var ret = new List<Node<T>>();
            if(topDown)
            {
                ret.Add(this);
            }
            foreach(var c in Children)
            {
                ret.AddRange(c.GetAllChildren(topDown));
            }
            if(!topDown)
            {
                ret.Add(this);
            }
            return ret;
        }

        public void AddChild(Node<T> node)
        {
            children.Add(node);
            node.Parent = this;
        }

        public void RemoveChild(Node<T> node)
        {
            children.Remove(node);
            node.Parent = null;
        }

        public Node<T> Find(RsaKeyParameters publicKey)
        {
            return GetAllChildren().FirstOrDefault(o => o.PublicKey.Equals(publicKey));
        }
    }
}
