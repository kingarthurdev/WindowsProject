using System.Net.Sockets;
using System.Net;
using dotNetClassLibrary;
using System.Text;
using EncryptionDecryptionLibrary;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Runtime.InteropServices;
using SQLRCE;


namespace dotNetService
{
    public class Service
    {

        static bool pubkeyreceived = false;
        static Dictionary<string, IPInfo> keyip = new Dictionary<string, IPInfo>();

        public static void Main(string[] args)
        {
            try
            {
                Thread listenThread = new Thread(new ThreadStart(listen));
                listenThread.Start();
            }
            catch (Exception exception)
            {
                ProcessContent.WriteToFile(exception.ToString());
            }
        }

        public static void listen()
        {
            ProcessContent.WriteToFile("Listening on port 12000");
            UdpClient listener = new UdpClient(12000);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 12000);

            //these 2 handle setup, listens for public key --> sends aes key, listens for ack for aes key --> stops sending aes key and listens for normal messages
            waitForPubKey(listener, groupEP);
            waitForAESACK(listener, groupEP);

            tryLogin(listener, groupEP);


            //listen for normal messages (encrypted with aes key ofc) 
            while (true)
            {
                try
                {
                    byte[] bytes = listener.Receive(ref groupEP); // listening for encrypted messages

                    IPInfo info;
                    keyip.TryGetValue(groupEP.Address.ToString(), out info);
                    byte[] AESKey = info.aeskey;
                    byte[] IV = new byte[16];

                    //Encrypted byte[] message
                    byte[] byteMessage = new byte[bytes.Length - IV.Length];

                    //todo: there's gotta be a better way to do this...
                    Buffer.BlockCopy(bytes, 0, IV, 0, IV.Length);
                    Buffer.BlockCopy(bytes, IV.Length, byteMessage, 0, byteMessage.Length);


                    //Note: doing a new var for decryptedBytes is important because byteMessage and DecryptedBytes are of different sizes -- I think...???
                    byte[] decryptedBytes = EncryptionDecryption.AESDecrypt(byteMessage, AESKey, IV);

                    (uint num, char delim, string message, DateTime sendTime) = ProcessContent.convertFromTimestampedBytes(decryptedBytes);
 
                    ProcessContent.sendACK(decryptedBytes, groupEP.Address.ToString(), AESKey, IV);
                }
                catch(Exception error )
                {
                    Console.WriteLine("Encountered Error. Error: "+error);
                    throw new ArgumentException("broken :("); //hehe
                }


            }

        }

        public static void waitForPubKey(UdpClient listener, IPEndPoint groupEP)
        {

            //the parameters are: specifies that communicates with ipv4, socket will use datagrams -- independent messages with udp, socket will use udp 
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            while (!keyip.ContainsKey(groupEP.Address.ToString()))
            {
                byte[] bytes = listener.Receive(ref groupEP);
                Console.WriteLine(Encoding.ASCII.GetString(bytes));
                try
                {

                    //verify that the data is in the correct format and that it is valid RSA
                    var rsa = new RSACryptoServiceProvider();
                    rsa.FromXmlString(Encoding.ASCII.GetString(bytes));

                    //Verify that we got the public key only
                    if (rsa.PublicOnly)
                    {
                        Console.WriteLine("Public RSA key Received");
                        pubkeyreceived = true;

                        byte[] AESKey = EncryptionDecryption.generateRandomAESKey();

                        //assign rnd generated aes key to the ip addr and store the pub key too. 
                        keyip.Add(groupEP.Address.ToString(), new IPInfo(AESKey, Encoding.ASCII.GetString(bytes)));

                        //AES Key encrypted by RSA Public Key received 
                        byte[] AESKeyEncrypted = EncryptionDecryption.RSAEncryptMessage(AESKey, Encoding.ASCII.GetString(bytes));
                            
                        byte[] ack = Encoding.ASCII.GetBytes("Public Key Received");
                        byte[] final = new byte[ack.Length + AESKeyEncrypted.Length];

                        Buffer.BlockCopy(ack, 0, final,0, ack.Length);
                        Buffer.BlockCopy(AESKeyEncrypted, 0, final, ack.Length,AESKeyEncrypted.Length);


                        sock.SendTo(final, new IPEndPoint(IPAddress.Parse(groupEP.Address.ToString()), 1543));
                        sock.Close();
                    } 
                    rsa.Dispose();
                }
                catch
                {
                    Console.WriteLine("Invalid public key received");
                    sock.SendTo(Encoding.ASCII.GetBytes("Invalid public key received"), new IPEndPoint(IPAddress.Parse(groupEP.Address.ToString()), 1543));

                }

            }
        }

        public static void waitForAESACK(UdpClient listener, IPEndPoint groupEP)
        {
            //Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            while (true)
            {
                byte[] bytes = listener.Receive(ref groupEP);
                Console.WriteLine(Encoding.ASCII.GetString(bytes));
                if(Encoding.ASCII.GetString(bytes).Equals("AES Key Received"))
                {
                    break;
                }
                else
                {
                    IPInfo info;
                    keyip.TryGetValue(groupEP.Address.ToString(), out info);

                    byte[] AESKey = info.aeskey;

                    //todo: remove duplicate code?
                    byte[] AESKeyEncrypted = EncryptionDecryption.RSAEncryptMessage(AESKey, info.pubkey);

                    byte[] ack = Encoding.ASCII.GetBytes("Public Key Received");
                    byte[] final = new byte[ack.Length + AESKeyEncrypted.Length];

                    Buffer.BlockCopy(ack, 0, final, 0, ack.Length);
                    Buffer.BlockCopy(AESKeyEncrypted, 0, final, ack.Length, AESKeyEncrypted.Length);

                    Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    sock.SendTo(final, new IPEndPoint(IPAddress.Parse(groupEP.Address.ToString()), 1543));
                    sock.Close();
                }
            }
        }
        public static void tryLogin(UdpClient listener, IPEndPoint groupEP)
        {
            while (true)
            {
                byte[] bytes = listener.Receive(ref groupEP); // listening for encrypted messages

                IPInfo info;
                keyip.TryGetValue(groupEP.Address.ToString(), out info);
                byte[] AESKey = info.aeskey;
                byte[] IV = new byte[16];

                //Encrypted byte[] message
                byte[] byteMessage = new byte[bytes.Length - IV.Length];

                //todo: there's gotta be a better way to do this...
                Buffer.BlockCopy(bytes, 0, IV, 0, IV.Length);
                Buffer.BlockCopy(bytes, IV.Length, byteMessage, 0, byteMessage.Length);

                byte[] decryptedBytes = EncryptionDecryption.AESDecrypt(byteMessage, AESKey, IV);

                string[] message = Encoding.ASCII.GetString(decryptedBytes).Split(':');
                SQLConnection auth = new SQLConnection();
                Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                if (message.Length == 2 && auth.safeLogin(message[0], message[1]))
                {
                    Console.WriteLine("Login Succeeded!");
                    byte[] final = Encoding.ASCII.GetBytes("Login Succeeded!");
                    byte[] encryptedMessage = EncryptionDecryption.AESEncrypt(Encoding.UTF8.GetBytes(Encoding.ASCII.GetString(final)), AESKey, IV);
                    byte[] finalWithIVPlainText = new byte[encryptedMessage.Length + 16];

                    byte[] iv = EncryptionDecryption.generateRandomAESIV();

                    Buffer.BlockCopy(iv, 0, finalWithIVPlainText, 0, iv.Length);
                    Buffer.BlockCopy(encryptedMessage, 0, finalWithIVPlainText, iv.Length, encryptedMessage.Length);

                    sock.SendTo(finalWithIVPlainText, new IPEndPoint(IPAddress.Parse(groupEP.Address.ToString()), 1543));
                    sock.Close();
                    break;
                }
                else
                {
                    Console.WriteLine("Login Failed!");
                    byte[] final = Encoding.ASCII.GetBytes("Login Failed!");
                    byte[] encryptedMessage = EncryptionDecryption.AESEncrypt(Encoding.UTF8.GetBytes(Encoding.ASCII.GetString(final)), AESKey, IV);
                    byte[] finalWithIVPlainText = new byte[encryptedMessage.Length + 16];

                    byte[] iv = EncryptionDecryption.generateRandomAESIV();

                    Buffer.BlockCopy(iv, 0, finalWithIVPlainText, 0, iv.Length);
                    Buffer.BlockCopy(encryptedMessage, 0, finalWithIVPlainText, iv.Length, encryptedMessage.Length);

                    sock.SendTo(finalWithIVPlainText, new IPEndPoint(IPAddress.Parse(groupEP.Address.ToString()), 1543));
                    sock.Close();
                }
            }
        }
    }

    public class IPInfo
    {
        public byte[] aeskey;
        public string pubkey;
        public IPInfo(byte[] aesKey, string pubkey)
        {
            this.aeskey = aesKey;
            this.pubkey = pubkey;
        }
    }
}
