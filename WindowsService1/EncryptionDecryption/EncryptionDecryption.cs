using System;
using System.Security.Cryptography;

namespace EncryptionDecryption
{
    public class EncryptionDecryption
    {
        public static (string publicKey, string privateKey) GenerateRSAKeys()
        {
            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
            {
                return (RSA.ToXmlString(false), RSA.ToXmlString(true));
            }
        }

        public static byte[] decryptMessage(byte[] data, string XMLPrivateKey) {
            var rsa = RSA.Create();
            rsa.FromXmlString(XMLPrivateKey);

            return rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA256);
        }

        public static byte[] encryptMessage(byte[] data, string XMLPublicKey)
        {
            var rsa = RSA.Create();
            rsa.FromXmlString(XMLPublicKey);

            return rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
        }
    }

}
