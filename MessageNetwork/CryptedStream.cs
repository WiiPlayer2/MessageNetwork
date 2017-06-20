using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageNetwork
{
    public class CryptedStream : Stream
    {
        private AsymmetricCipherKeyPair keyPair;
        private IAsymmetricBlockCipher decryptEngine;
        private IAsymmetricBlockCipher encryptEngine;

        private byte[] inputBuffer = null;
        private int inputBufferPos = 0;
        private byte[] outputBuffer;
        private int outputBufferPos = 0;

        private BinaryWriter bWriter;
        private BinaryReader bReader;

        public CryptedStream(Stream baseStream, AsymmetricCipherKeyPair keyPair)
        {
            if (!baseStream.CanRead || !baseStream.CanWrite)
            {
                throw new ArgumentException("The base stream has to be readable and writeable.");
            }
            //Check if keyPair are both private and public keys

            Base = baseStream;
            this.keyPair = keyPair;

            decryptEngine = new OaepEncoding(new RsaEngine());
            decryptEngine.Init(false, keyPair.Private);

            encryptEngine = new OaepEncoding(new RsaEngine());

            bWriter = new BinaryWriter(Base);
            bReader = new BinaryReader(Base);
        }

        public bool Setup()
        {
            return Setup(null);
        }

        public bool Setup(Func<RsaKeyParameters, bool> authorizedCallback)
        {
            var publicKey = keyPair.Public as RsaKeyParameters;
            var mod = publicKey.Modulus.ToByteArray();
            var exp = publicKey.Exponent.ToByteArray();

            bWriter.Write(mod.Length);
            bWriter.Write(mod);
            bWriter.Write(exp.Length);
            bWriter.Write(exp);

            mod = bReader.ReadBytes(bReader.ReadInt32());
            exp = bReader.ReadBytes(bReader.ReadInt32());
            publicKey = new RsaKeyParameters(false, new BigInteger(mod), new BigInteger(exp));

            var authorized = true;
            if (authorizedCallback != null)
            {
                try
                {
                    authorized = authorizedCallback(publicKey);
                }
                catch
                {
                    authorized = false;
                }
            }

            if (!authorized)
            {
                Base.Close();
            }
            else
            {
                encryptEngine.Init(true, publicKey);
                outputBuffer = new byte[encryptEngine.GetInputBlockSize()];
            }

            return authorized;
        }

        public Stream Base { get; private set; }

        #region Unsupported
        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }
        #endregion

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override void Flush()
        {
            if (outputBufferPos > 0)
            {
                var writeBytes = encryptEngine.ProcessBlock(outputBuffer, 0, outputBufferPos);
                bWriter.Write(writeBytes);
                outputBufferPos = 0;
                Base.Flush();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            for (var i = 0; i < count; i++)
            {
                if (inputBuffer == null)
                {
                    var readBytes = bReader.ReadBytes(decryptEngine.GetInputBlockSize());
                    inputBuffer = decryptEngine.ProcessBlock(readBytes, 0, readBytes.Length);
                }

                buffer[offset + i] = inputBuffer[inputBufferPos];
                inputBufferPos++;

                if(inputBufferPos == inputBuffer.Length)
                {
                    inputBuffer = null;
                    inputBufferPos = 0;
                }
            }
            return count;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            for(var i = 0; i < count; i++)
            {
                outputBuffer[outputBufferPos] = buffer[offset + i];
                outputBufferPos++;
                if(outputBufferPos == outputBuffer.Length)
                {
                    Flush();
                }
            }
        }
    }
}
