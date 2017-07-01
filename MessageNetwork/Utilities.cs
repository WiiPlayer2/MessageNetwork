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
        public static string BasePath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MessageNetwork");
            }
        }

        public static string GetPath(params string[] paths)
        {
            return Path.Combine(new string[] { BasePath }.Concat(paths).ToArray());
        }

        public static AsymmetricCipherKeyPair GenerateKeyPair()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MessageNetwork");
            Directory.CreateDirectory(folder);
            var file = Path.Combine(folder, "id_rsa");

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

            fileWriter = new StreamWriter($"{file}.pub");
            pemWriter = new PemWriter(fileWriter);
            pemWriter.WriteObject(keyPair.Public);
            pemWriter.Writer.Flush();
            fileWriter.Close();

            return keyPair;
        }

        public static AsymmetricCipherKeyPair LoadKeyPair()
        {
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MessageNetwork", "id_rsa");
            return LoadKeyPair(file);
        }

        public static AsymmetricCipherKeyPair LoadKeyPair(string file)
        {
            var fileReader = new StreamReader(file);
            var pemReader = new PemReader(fileReader);
            var key = pemReader.ReadObject() as AsymmetricCipherKeyPair;
            fileReader.Close();

            return key;
        }

        public static AsymmetricCipherKeyPair GenerateOrLoadKeyPair()
        {
            return GenerateOrLoadKeyPair(GetPath("id_rsa"));
        }

        public static AsymmetricCipherKeyPair GenerateOrLoadKeyPair(string file)
        {
            if (File.Exists(file))
            {
                return LoadKeyPair(file);
            }
            else
            {
                return GenerateKeyPair(file);
            }
        }

        public static string GetHashString(this object obj)
        {
            var hashCode = obj.GetHashCode();
            var bytes = BitConverter.GetBytes(hashCode);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return string.Concat(bytes.Select(o => o.ToString("X2")));
        }
    }
}
