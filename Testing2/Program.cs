using MessageNetwork;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Testing;

namespace Testing2
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(1000);
            var keyPair = Utilities.GenerateOrLoadKeyPair("id_rsa");
            Console.WriteLine($"[PUBLIC] {keyPair.Public.GetHashString()}");

            var node = new MessageNode<TestingMessage>(keyPair);
            node.NodeJoined += Node_NodeJoined;
            node.NodeLeft += Node_NodeLeft;
            node.MessageReceived += Node_MessageReceived;

            var success = node.Setup("localhost", 12345);

            while(true)
            {
                var line = Console.ReadLine();
                node.SendMessage(null, new TestingMessage() { Text = line });
            }
        }

        private static void Node_MessageReceived(MessageNode<TestingMessage> sender, Org.BouncyCastle.Crypto.Parameters.RsaKeyParameters senderKey, bool isPublic, TestingMessage message, byte[] payload)
        {
            Console.WriteLine($">> {message.Text}");
        }

        private static void Node_NodeLeft(MessageNode<TestingMessage> sender, Org.BouncyCastle.Crypto.Parameters.RsaKeyParameters publicKey)
        {
            Console.WriteLine($"[LEFT] {publicKey.GetHashString()}");
        }

        private static void Node_NodeJoined(MessageNode<TestingMessage> sender, Org.BouncyCastle.Crypto.Parameters.RsaKeyParameters publicKey)
        {
            Console.WriteLine($"[JOIN] {publicKey.GetHashString()}");
        }
    }
}
