using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            Buffer.BlockCopy(uintArr, 0, finalArr, 0, uintArr.Length);
            Buffer.BlockCopy(charArr, 0, finalArr, uintArr.Length, charArr.Length);
            Buffer.BlockCopy(xmlArr, 0, finalArr, uintArr.Length + charArr.Length, xmlArr.Length);

            return finalArr;
        }



        public static (uint, char, string) convertFromByteArray(byte[] input)
        {
            uint convertedUint = BitConverter.ToUInt32(input, 0);

            char convertedChar = BitConverter.ToChar(input, sizeof(uint));

            string convertedString = Encoding.ASCII.GetString(input, 5, input.Length - 5);

            return (convertedUint, convertedChar, convertedString);
        }


        //Todo: Complete this function. Not done yet. 
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

        public static void convertFromTimestampedBytes(byte[] bytes)
        {

        }
        
        //send an acknowledgement with the timestamp of when the original request was first recieved
        public static void sendACK(byte[] recieved)
        {
            byte[] ack = Encoding.ASCII.GetBytes("ack");
            byte[] final = new byte[ack.Length+ 8];
            Buffer.BlockCopy(recieved, 0, final, 0, 8);
            Buffer.BlockCopy(ack, 0, final, 8, ack.Length);
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

    }
}
