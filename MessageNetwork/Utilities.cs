using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.IO;
using Org.BouncyCastle.OpenSsl;

namespace MessageNetwork
{
    public static class Utilities
    {

        public static AsymmetricCipherKeyPair GenerateKeyPair()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MessageNetwork");
            Directory.CreateDirectory(folder);
            var file = Path.Combine(folder, "id_rsa.key");

            return GenerateKeyPair(file);
        }

        public static AsymmetricCipherKeyPair GenerateKeyPair(string file)
        {
            var genParam = new KeyGenerationParameters(new SecureRandom(), 4096);
            var gen = new RsaKeyPairGenerator();
            gen.Init(genParam);
            var keyPair = gen.GenerateKeyPair();

            var fileWriter = new StreamWriter(file);
            var pemWriter = new PemWriter(fileWriter);
            pemWriter.WriteObject(keyPair.Private);
            pemWriter.Writer.Flush();
            fileWriter.Close();

            return keyPair;
        }

        public static AsymmetricCipherKeyPair LoadKey()
        {
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MessageNetwork", "id_rsa.key");
            return LoadKey(file);
        }

        public static AsymmetricCipherKeyPair LoadKey(string file)
        {
            var fileReader = new StreamReader(file);
            var pemReader = new PemReader(fileReader);
            var key = pemReader.ReadObject() as AsymmetricCipherKeyPair;
            fileReader.Close();

            return key;
        }
    }
}
