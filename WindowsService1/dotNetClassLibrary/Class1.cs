using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
        /// This is an Arthur function :O
        /// </summary>
        /// <param name="input">The input ios used for a thing</param>
        /// <returns>Returns potatos</returns>
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
            byte[] final = new byte[timeless.Length +8]; // Ensure 8 bytes, not 9
            Buffer.BlockCopy(time, 0, final, 0, time.Length);
            Buffer.BlockCopy(timeless, 0, final, 8, timeless.Length);

            return final; 
        }

        //return uint, char, string, date
        public static (uint, char, string, DateTime) convertFromTimestampedBytes(byte[] bytes)
        {
            DateTime sentTime = DateTime.FromBinary(BitConverter.ToInt64(bytes, 0));
            byte[] timeless = new byte[bytes.Length - 8];
            Buffer.BlockCopy(bytes, 8, timeless, 0, timeless.Length);
            (uint tempUint, char tempChar, string tempMessage) = convertFromByteArray(timeless);
            return (tempUint,  tempChar,  tempMessage, sentTime);
        }

        //send an acknowledgement with the timestamp of when the original request was first recieved
        public static void sendACK(byte[] recieved, string ip)
        {
            //get the message #
            var parsed = convertFromTimestampedBytes(recieved);
            byte[] uintbytes = BitConverter.GetBytes(parsed.Item1);

            byte[] ack = Encoding.ASCII.GetBytes("ack");
            byte[] final = new byte[ack.Length+ 8+4];

            //sends in this format: timestamp, "ack", uint message #
            Buffer.BlockCopy(recieved, 0, final, 0, 8);
            Buffer.BlockCopy(ack, 0, final, 8, ack.Length);
            Buffer.BlockCopy(uintbytes, 0, final, 8+ ack.Length, uintbytes.Length);

            //todo: remove duplicate code 
            //recipient address and 'port', sends ack to port 1543
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ip), 1543);

            //the parameters are: specifies that communicates with ipv4, socket will use datagrams -- independent messages with udp  ,socket will use udp 
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.SendTo(final, endpoint);

        }

        public static void WriteToFile(string message, string extension)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\logs\\servicelog" + extension;
            if (!File.Exists(filepath))
            {
                using (StreamWriter sw = File.CreateText(filepath)) { sw.WriteLine(message); }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath)) { sw.WriteLine(message); }
            }

        }
        public static void WriteToFile(string message)
        {
            WriteToFile(message, ".txt");

        }

        // todo: remove not necessary? 
        public static double getLatency(byte[] ack)
        {
            DateTime sentTime = DateTime.FromBinary(BitConverter.ToInt64(ack, 0));
            return (DateTime.Now - sentTime).TotalMilliseconds;

        }

        //todo: have another one, communicate this way, but with upstream filtering 
    }
}
