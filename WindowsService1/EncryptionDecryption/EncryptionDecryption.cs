using System;
using System.Security.Cryptography;
using System.Text;

namespace EncryptionDecryptionLibrary
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

        public static byte[] RSADecryptMessage(byte[] data, string XMLPrivateKey) {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(XMLPrivateKey);

            return rsa.Decrypt(data, true);
        }

        public static byte[] RSAEncryptMessage(byte[] data, string XMLPublicKey)
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(XMLPublicKey);
            
            return rsa.Encrypt(data, true);
        }

        public static byte[] generateRandomAESKey()
        {
            var rnd = new RNGCryptoServiceProvider();
            var key = new byte[32]; // 256 bit aes encryption
            rnd.GetNonZeroBytes(key);
            return key; 
        }

        public static byte[] generateRandomAESIV()
        {
            var rnd = new RNGCryptoServiceProvider();
            var key = new byte[16]; // 256 bit aes encryption
            rnd.GetNonZeroBytes(key);

            //TEMPORARY, DELETE LATER FOR REAL SECURITY
            for (int i = 0; i < key.Length; i++)
            {
                key[i] = 0;
            }



            return key;
        }

        //Code provided by microsoft documentation
        public static byte[] AESEncrypt(byte[] UTF8Bytes, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                byte[] encryptedBytes;
                using (var msEncrypt = new System.IO.MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(UTF8Bytes, 0, UTF8Bytes.Length);
                    }
                    encryptedBytes = msEncrypt.ToArray();
                }
                return encryptedBytes;
            }
        }

        //Code provided by microsoft documentation, edited slightly because repeated conversions btwn utf8 bytes breaks certain things
        public static byte[] AESDecrypt(byte[] ciphertext, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                byte[] decryptedBytes;
                using (var msDecrypt = new System.IO.MemoryStream(ciphertext))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var msPlain = new System.IO.MemoryStream())
                        {
                            csDecrypt.CopyTo(msPlain);
                            decryptedBytes = msPlain.ToArray();
                        }
                    }
                }
                //return Encoding.UTF8.GetString(decryptedBytes);
                return decryptedBytes;

            }
        }

        /// <summary>
        /// Parses and decrypts data from the format where the first 16 bytes are the IV
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="AESKey"></param>
        /// <returns>Returns a byte[] that contains the plaintext data</returns>
        public static byte[] parseAndDecrypt(byte[] ciphertext, byte[] AESKey)
        {
            byte[] IV = new byte[16];
            byte[] byteMessage = new byte[ciphertext.Length - IV.Length];

            //todo: there's gotta be a better way to do this...
            Buffer.BlockCopy(ciphertext, 0, IV, 0, IV.Length);
            Buffer.BlockCopy(ciphertext, IV.Length, byteMessage, 0, byteMessage.Length);

            byteMessage = EncryptionDecryption.AESDecrypt(byteMessage, AESKey, IV);

            return byteMessage;

        }

     
    }

}
