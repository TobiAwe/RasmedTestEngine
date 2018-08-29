using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RasmedTestEngine.Common.Crypto
{
    public class BackendCryptoEngine
    {
        private static readonly byte[] Key =
        {
            129, 207, 16, 14, 44, 26, 165, 54, 114, 108, 72, 172, 37, 112, 222, 209, 241, 42,
            175, 144, 173, 53, 196, 29, 44, 29, 17, 218, 103, 236, 35, 219
        };

        private static readonly byte[] Vector = {144, 64, 201, 101, 55, 30, 16, 119, 231, 021, 221, 112, 178, 32, 114, 166};
        private readonly ICryptoTransform _encryptor;
        private readonly ICryptoTransform _decryptor;
        private readonly UTF8Encoding _encoder;

        public BackendCryptoEngine()
        {
            var rm = new RijndaelManaged();
            _encryptor = rm.CreateEncryptor(Key, Vector);
            _decryptor = rm.CreateDecryptor(Key, Vector);
            _encoder = new UTF8Encoding();
        }

        public string Encrypt(string unencrypted)
        {
            return Convert.ToBase64String(Encrypt(_encoder.GetBytes(unencrypted)));
        }

        public string Decrypt(string encrypted)
        {
            return _encoder.GetString(Decrypt(Convert.FromBase64String(encrypted)));
        }


        public byte[] Encrypt(byte[] buffer)
        {
            return Transform(buffer, _encryptor);
        }

        public byte[] Decrypt(byte[] buffer)
        {
            return Transform(buffer, _decryptor);
        }

        protected byte[] Transform(byte[] buffer, ICryptoTransform transform)
        {
            var stream = new MemoryStream();
            using (var cs = new CryptoStream(stream, transform, CryptoStreamMode.Write))
            {
                cs.Write(buffer, 0, buffer.Length);
            }
            return stream.ToArray();
        }
    }
}