using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bolao10.Services
{

    using System.Text;
    using System.Security.Cryptography;


    public class EncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _initVector;

        private DESCryptoServiceProvider _provider;

        public EncryptionService()
        {
            _provider = new DESCryptoServiceProvider();
            _key = Encoding.Default.GetBytes("MyKeyCaS");
            _initVector = Encoding.Default.GetBytes("itVector");
        }

        public string Encrypt(string contents)
        {
            ICryptoTransform transform = _provider.CreateEncryptor(_key, _initVector);

            byte[] bArray = Encoding.Default.GetBytes(contents);
            byte[] bOutput = transform.TransformFinalBlock(bArray, 0, bArray.Length);

            return Convert.ToBase64String(bOutput);
        }

        public string Decrypt(string contents)
        {
            ICryptoTransform transform = _provider.CreateDecryptor(_key, _initVector);

            byte[] bArray = Convert.FromBase64String(contents);
            byte[] bOutput = transform.TransformFinalBlock(bArray, 0, bArray.Length);

            return Encoding.Default.GetString(bOutput);
        }
    }
}
