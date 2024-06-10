

using System.Net.Sockets;
using System.Net;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using dotNetClassLibrary;
using System.Windows;


// ProcessContent.convertToByteArray(1, ';', "asdf");   //This works well

/*
string filename = "C:\\Users\\E1495970\\Downloads\\examplenote.xml";
FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);

byte[] ImageData = new byte[fs.Length];
fs.Read(ImageData, 0, (int) fs.Length);

//Close the File Stream
fs.Close();


Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

IPAddress broadcast = IPAddress.Parse("127.0.0.1");

IPEndPoint endpoint = new IPEndPoint(broadcast, 12000);
sock.SendTo(ImageData, endpoint);
*/

byte[] a = ProcessContent.convertToByteArray(1, ':', "abcdefg");
(uint num, char delim, string message) = ProcessContent.convertFromByteArray(a);

Console.WriteLine(num+""+delim+message);

string message2 = "Simple MessageBox";
MessageBox.Show(message2);