using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNetClassLibrary
{
   
    public static class ProcessContent
    {
        
        public static byte[] convertToByteArray(uint num, char delim, string xml)
        {
            byte[] xmlArr = new byte[xml.Length];
            xmlArr = Encoding.ASCII.GetBytes(xml);
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

    }
}
