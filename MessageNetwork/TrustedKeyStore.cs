using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace MessageNetwork
{
    public class TrustedKeyStore : IEnumerable<RsaKeyParameters>
    {
        private HashSet<RsaKeyParameters> keys;

        public TrustedKeyStore()
            : this(Utilities.GetPath("trusted_keys"))
        { }

        public TrustedKeyStore(string folder)
        {
            FolderPath = folder;
            Directory.CreateDirectory(FolderPath);

            keys = new HashSet<RsaKeyParameters>();

            foreach (var f in Directory.EnumerateFiles(FolderPath))
            {
                var fileReader = new StreamReader(f);
                var pemReader = new PemReader(fileReader);
                keys.Add(pemReader.ReadObject() as RsaKeyParameters);
                fileReader.Close();
            }
        }

        public string FolderPath { get; private set; }

        public void Add(RsaKeyParameters key)
        {
            keys.Add(key);

            var fileWriter = new StreamWriter(Path.Combine(FolderPath, $"{key.GetHashString()}.pub"));
            var pemWriter = new PemWriter(fileWriter);
            pemWriter.WriteObject(key);
            pemWriter.Writer.Flush();
            fileWriter.Close();
        }

        public void Remove(RsaKeyParameters key)
        {
            keys.Remove(key);
            File.Delete(Path.Combine(FolderPath, $"{key.GetHashString()}.pub"));
        }

        public void Clear()
        {
            keys.Clear();
            foreach (var f in Directory.EnumerateFiles(FolderPath))
            {
                File.Delete(f);
            }
        }

        public IEnumerator<RsaKeyParameters> GetEnumerator()
        {
            return keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}