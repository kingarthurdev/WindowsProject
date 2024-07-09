using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using dotNetClassLibrary;
using EncryptionDecryptionLibrary; 
namespace burnerProj
{
    internal class Program
    {
        static uint count = 1;
        static void Main(string[] args)
        {
            for (int i = 0; i < 50; i++)
            {



                byte[] AESKey = EncryptionDecryption.generateRandomAESKey();
                byte[] AESIV = EncryptionDecryption.generateRandomAESIV();

                byte[] plaintextTimestampedDataRequest = ProcessContent.convertToTimestampedBytes(count, ';', "Request For Data");
                Console.WriteLine(plaintextTimestampedDataRequest);


                byte[] encryptedTimestampedDataRequest = EncryptionDecryption.AESEncrypt(plaintextTimestampedDataRequest, AESKey, AESIV);


                //first 16 bytes reserved to add the IV
                byte[] final = new byte[16 + encryptedTimestampedDataRequest.Length];
                Buffer.BlockCopy(AESIV, 0, final, 0, AESIV.Length);
                Buffer.BlockCopy(encryptedTimestampedDataRequest, 0, final, 16, encryptedTimestampedDataRequest.Length);

                //the format of final should first 16 bytes are iv (all 0s for testing), the rest are encrypted data. 
                Console.WriteLine(final);



                //Now for the code from encrypted data provider, replacing "bytes" with final.
                byte[] IV = new byte[16];

                //Encrypted byte[] message
                byte[] byteMessage = new byte[final.Length - IV.Length];

                //todo: there's gotta be a better way to do this...
                Buffer.BlockCopy(final, 0, IV, 0, IV.Length);
                Buffer.BlockCopy(final, IV.Length, byteMessage, 0, byteMessage.Length);
                byte[] decryptedBytes = EncryptionDecryption.AESDecrypt(byteMessage, AESKey, IV); //this = plaintextTimestampedDataRequest



                sendACK(decryptedBytes, "127.0.0.1", AESKey, AESIV);
            }
            /*working: 
             * 
             *  byte[] AESKey = EncryptionDecryption.generateRandomAESKey();

            byte[] sendBytes = ProcessContent.convertToTimestampedBytes(count, ';', "Request For Data");

            Console.WriteLine(sendBytes);

            byte[] AESIV = EncryptionDecryption.generateRandomAESIV();
            sendBytes = EncryptionDecryption.AESEncrypt(sendBytes, AESKey, AESIV);

            //first 16 bytes reserved to add the IV
            byte[] final = new byte[sendBytes.Length + 16];
            Buffer.BlockCopy(AESIV, 0, final, 0, AESIV.Length);
            Buffer.BlockCopy(sendBytes, 0, final, 16, sendBytes.Length);

            //the format of final should first 16 bytes are iv (all 0s for testing), the rest are encrypted data. 
            Console.WriteLine(final);



            //Now for the code from encrypted data provider, replacing "bytes" with final.


            byte[] IV = new byte[16];

            //Encrypted byte[] message
            byte[] byteMessage = new byte[final.Length - IV.Length];

            //todo: there's gotta be a better way to do this...
            Buffer.BlockCopy(final, 0, IV, 0, IV.Length);
            Buffer.BlockCopy(final, IV.Length, byteMessage, 0, byteMessage.Length);

            Console.WriteLine(final);
            Console.WriteLine(byteMessage);
            uint a; char b; string c; DateTime d;
            (a, b, c, d) = ProcessContent.convertFromTimestampedBytes(EncryptionDecryption.AESDecrypt(byteMessage, AESKey, IV));

            Console.WriteLine(a);
            Console.WriteLine(b);
            Console.WriteLine(c);
            Console.WriteLine(d);


            */ 
            /*
            byte[] AESKey = EncryptionDecryption.generateRandomAESKey();
            byte[] AESIV = EncryptionDecryption.generateRandomAESIV();
            byte[] encrypted = EncryptionDecryption.AESEncrypt(Encoding.ASCII.GetBytes("test"), AESKey, AESIV);
            Console.WriteLine(EncryptionDecryption.AESDecrypt(encrypted, AESKey, AESIV));


            byte[] sendBytes = ProcessContent.convertToTimestampedBytes(count, ';', "Request For Data");

            uint a; char b; string c; DateTime d;





            for (int i = 0; i < sendBytes.Length; i++)
            {
                Console.Write(sendBytes[i]);
            }
            Console.WriteLine("\n\n");

            byte[] encryptedSendBytes = EncryptionDecryption.AESEncrypt(sendBytes, AESKey, AESIV);
            byte[] decryptedSendBytes = AESDecrypt(encryptedSendBytes, AESKey, AESIV);

            for (int i = 0; i < decryptedSendBytes.Length; i++)
            {
                Console.Write(decryptedSendBytes[i]);
            }
            Console.WriteLine("\n\n");

            (a, b, c, d) = ProcessContent.convertFromTimestampedBytes(decryptedSendBytes);
            Console.WriteLine(a);
            Console.WriteLine(b);
            Console.WriteLine(c);
            Console.WriteLine(d);
            Thread.Sleep(100);
        */

            /*
            byte[] sendBytes = ProcessContent.convertToTimestampedBytes(count, ';', "Request For Data");

            Console.WriteLine(sendBytes);

            byte[] AESIV = EncryptionDecryption.generateRandomAESIV();
            sendBytes = EncryptionDecryption.AESEncrypt(sendBytes, AESKey, AESIV);

            byte[] test = Encoding.UTF8.GetBytes(EncryptionDecryption.AESDecrypt(sendBytes, AESKey, AESIV));
            Console.WriteLine(test);


            //first 16 bytes reserved to add the IV
            byte[] final = new byte[sendBytes.Length + 16];
            Buffer.BlockCopy(AESIV, 0, final, 0, AESIV.Length);
            Buffer.BlockCopy(sendBytes, 0, final, 16, sendBytes.Length);

            //the format of final should first 16 bytes are iv (all 0s for testing), the rest are encrypted data. 
            Console.WriteLine(final);



            //Now for the code from encrypted data provider, replacing "bytes" with final.


            byte[] IV = new byte[16];

            //Encrypted byte[] message
            byte[] byteMessage = new byte[final.Length - IV.Length];

            //todo: there's gotta be a better way to do this...
            Buffer.BlockCopy(final, 0, IV, 0, IV.Length);
            Buffer.BlockCopy(final, IV.Length, byteMessage, 0, byteMessage.Length);

            Console.WriteLine(final);
            Console.WriteLine(byteMessage);
            uint a; char b; string c; DateTime d;
            (a,b,c,d) = ProcessContent.convertFromTimestampedBytes(Encoding.UTF8.GetBytes(EncryptionDecryption.AESDecrypt(byteMessage, AESKey, IV)));

            Console.WriteLine(a);
            Console.WriteLine(b);
            Console.WriteLine(c);
            //Console.WriteLine(d);


            //ProcessContent.convertFromTimestampedBytes(ProcessContent.convertToTimestampedBytes(count, ';', "Thingy boi thing"));
            */
        }
        public static void sendACK(byte[] recieved, string ip, byte[] key, byte[] iv)
        {

            //get the message #
            var parsed = ProcessContent.convertFromTimestampedBytes(recieved);
            byte[] uintbytes = BitConverter.GetBytes(parsed.Item1);
            byte[] xmlBytes = ProcessContent.genXMLBytes();
            byte[] ack = Encoding.ASCII.GetBytes("ack");
            Console.WriteLine(parsed.Item4);




            byte[] final = new byte[ack.Length + 8 + 4 + xmlBytes.Length]; //8 comes from timestamp, 4 comes from uint

            //this format so far: timestamp, "ack", uint message #, xml message --> 8, 3, 4, possibly a bunch of bytes
            byte[] timestampReformatted = BitConverter.GetBytes(parsed.Item4.Ticks);
            Buffer.BlockCopy(timestampReformatted, 0, final, 0, 8); //index 0-7            -- This is supposed to send over the original timestamp 
            Console.Write(Encoding.ASCII.GetString(final));

            Buffer.BlockCopy(ack, 0, final, 8, ack.Length); //index 8-10
            Console.Write(Encoding.ASCII.GetString(final));

            Buffer.BlockCopy(uintbytes, 0, final, 8 + ack.Length, uintbytes.Length); //index 11-14
            Console.Write(Encoding.ASCII.GetString(final));

            Buffer.BlockCopy(xmlBytes, 0, final, 8 + 3 + 4, xmlBytes.Length); //index 15 - 15+xmlLength
            Console.Write(Encoding.ASCII.GetString(final));


            //todo: remove duplicate code 
            //recipient address and 'port', sends ack to port 1543
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ip), 1543);

            Console.WriteLine("Encrypted Response Sent.");

            

            byte[] encryptedMessage = EncryptionDecryption.AESEncrypt(Encoding.UTF8.GetBytes(Encoding.ASCII.GetString(final)), key, iv);
            byte[] finalWithIVPlainText = new byte[encryptedMessage.Length + 16];

            iv = EncryptionDecryption.generateRandomAESIV();

            Buffer.BlockCopy(iv, 0, finalWithIVPlainText, 0, iv.Length);
            Buffer.BlockCopy(encryptedMessage, 0, finalWithIVPlainText, iv.Length, encryptedMessage.Length);


            //Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //sock.SendTo(finalWithIVPlainText, endpoint);

        }

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
                return decryptedBytes;
                //return decryptedBytes;

            }
        }


    }
}
