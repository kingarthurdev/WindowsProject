using System.Net.Sockets;
using System.Net;
using System.Text;
namespace dependency
{
    public static class ProcessContent
    {
        /*public static void sendFile(string FilePath)
        {
            FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read);

            byte[] ImageData = new byte[fs.Length];
            fs.Read(ImageData, 0, (int)fs.Length);

            //Close the File Stream
            fs.Close();

            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPAddress broadcast = IPAddress.Parse("127.0.0.1");

            IPEndPoint endpoint = new IPEndPoint(broadcast, 12000);
            sock.SendTo(ImageData, endpoint);

        }*/

            public static (uint, char, string) convertFromByteArray(byte[] input)
            {
                uint convertedUint = BitConverter.ToUInt32(input, 0);
                char convertedChar = Encoding.ASCII.GetChars(input, 4, 5)[0];
                string convertedString = Encoding.ASCII.GetString(input, 5, input.Length - 5);

                return (convertedUint, convertedChar, convertedString);
            }

            public static byte[] convertToByteArray(uint num, char delim, string xml)
            {
                byte[] xmlArr = new byte[xml.Length];
                xmlArr = Encoding.ASCII.GetBytes(xml);
                byte[] charArr = new byte[1] { Convert.ToByte(delim) };

                //uint bigEndianNum = (uint)IPAddress.HostToNetworkOrder((int)num);   // this is just here to make sure that everyone is talking in the same way -- avoid issues later
                byte[] uintArr = BitConverter.GetBytes(num);

                byte[] finalArr = new byte[uintArr.Length + charArr.Length + xmlArr.Length];

                Buffer.BlockCopy(uintArr, 0, finalArr, 0, uintArr.Length);
                Buffer.BlockCopy(charArr, 0, finalArr, uintArr.Length, charArr.Length);
                Buffer.BlockCopy(xmlArr, 0, finalArr, uintArr.Length + charArr.Length, xmlArr.Length);

                return finalArr;
            }
 
    }
}

