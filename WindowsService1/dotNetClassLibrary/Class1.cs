using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace dotNetClassLibrary
{
   
    public static class ProcessContent
    {
        
        public static byte[] convertToByteArray(uint num, char delim, string xml)
        {
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
            uint convertedUint = BitConverter.ToUInt32(input, 0);

            char convertedChar = BitConverter.ToChar(input, sizeof(uint));

            string convertedString = Encoding.ASCII.GetString(input, 5, input.Length - 5);

            return (convertedUint, convertedChar, convertedString);
        }


        public static byte[] convertToTimestampedBytes(uint num, char delim, string xml)
        {
            DateTime dt = DateTime.Now;
            byte[] time = BitConverter.GetBytes(dt.Ticks);
            byte[] timeless = convertToByteArray(num,delim,xml);
            byte[] final = new byte[timeless.Length +8]; // 8 bytes
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
            DateTime sentTime = DateTime.FromBinary(BitConverter.ToInt64(bytes, 0));
            byte[] timeless = new byte[bytes.Length - 8];
            Buffer.BlockCopy(bytes, 8, timeless, 0, timeless.Length);
            (uint tempUint, char tempChar, string tempMessage) = convertFromByteArray(timeless);
            return (tempUint,  tempChar,  tempMessage, sentTime);
        }

        //send an acknowledgement with the timestamp of when the original request was first recieved
        //TODO: actually respond with valid xml data as well as ack messages
        public static void sendACK(byte[] recieved, string ip)
        {
            //get the message #
            var parsed = convertFromTimestampedBytes(recieved);
            byte[] uintbytes = BitConverter.GetBytes(parsed.Item1);
            byte[] xmlBytes = genXMLBytes();
            byte[] ack = Encoding.ASCII.GetBytes("ack");

            byte[] final = new byte[ack.Length+ 8+4 +xmlBytes.Length]; //8 comes from timestamp, 4 comes from uint

            //sends in this format: timestamp, "ack", uint message #, xml message --> 8, 3, 4, possibly a bunch of bytes
            Buffer.BlockCopy(recieved, 0, final, 0, 8); //index 0-7
            Buffer.BlockCopy(ack, 0, final, 8, ack.Length); //index 8-10
            Buffer.BlockCopy(uintbytes, 0, final, 8+ ack.Length, uintbytes.Length); //index 11-14
            Buffer.BlockCopy(xmlBytes, 0, final, 8 + 3 + 4, xmlBytes.Length); //index 15 - 15+xmlLength

            //todo: remove duplicate code 
            //recipient address and 'port', sends ack to port 1543
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ip), 1543);

            //the parameters are: specifies that communicates with ipv4, socket will use datagrams -- independent messages with udp  ,socket will use udp 
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.SendTo(final, endpoint);

        }

        
        public static void WriteToFile(string message)
        {
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
                new XElement("UnresolvedIssues", rand.Next(1,21))
            );
            byte[] XMLBytes = Encoding.ASCII.GetBytes(xmlTree.ToString());
            //to decode: XElement.Parse(Encoding.ASCII.GetString(XMLBytes, 0, XMLBytes.Length)).ToString(); 
            return XMLBytes; 
        }
        //todo: have another one, communicate this way, but with upstream filtering 
    }
}
