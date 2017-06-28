using MessageNetwork;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            var lkey = Utilities.LoadKey();
            Console.WriteLine(lkey.Public.GetHashCode());
            var lkey2 = Utilities.LoadKey();
            Console.WriteLine(lkey2.Public.GetHashCode());

            new Thread(() => RunServer(lkey)).Start();
            Thread.Sleep(1000);
            RunClient(lkey);

            Console.ReadKey(true);
        }

        static void RunServer(AsymmetricCipherKeyPair keyPair)
        {
            var listener = new TcpListener(IPAddress.Any, 12736);
            listener.Start();
            var client = listener.AcceptTcpClient();
            var netStream = client.GetStream();
            var cryptStream = new CryptedStream(netStream, keyPair);
            Console.WriteLine($"Server authorized: {cryptStream.Setup(ServerAuthorize)}");

            var bReader = new BinaryReader(cryptStream);
            while(true)
            {
                Console.WriteLine(bReader.ReadString());
            }
        }

        private static bool ServerAuthorize(RsaKeyParameters arg)
        {
            throw new NotImplementedException();
        }

        static void RunClient(AsymmetricCipherKeyPair keyPair)
        {
            var client = new TcpClient();
            client.Connect("localhost", 12736);
            var netStream = client.GetStream();
            var cryptStream = new CryptedStream(netStream, keyPair);
            Console.WriteLine($"Client authorized: {cryptStream.Setup()}");

            var bWriter = new BinaryWriter(cryptStream);
            //bWriter.Write(GetRandomString(509));
            //bWriter.Flush();

            while(true)
            {
                bWriter.Write(Console.ReadLine());
                bWriter.Flush();
            }
        }

        static string GetRandomString(int length)
        {
            var random = new Random();
            var count = length / 2;
            var bytes = new byte[count];
            random.NextBytes(bytes);
            return string.Concat(bytes.Select(o => o.ToString("X2")));
        }
    }
}
