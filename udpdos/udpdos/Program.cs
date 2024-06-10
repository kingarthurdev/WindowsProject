using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;


Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

IPAddress broadcast = IPAddress.Parse("127.0.0.1");

//byte[] sendbuf = Encoding.ASCII.GetBytes("Hello world");

byte[] sendbuf = {
    // DNS header
    0x12, 0x34,             // Message ID
    0x01, 0x00,             // Flags: Standard query
    0x00, 0x01,             // Question Count: 1
    0x00, 0x00,             // Answer Count: 0
    0x00, 0x00,             // Authority Count: 0
    0x00, 0x00,             // Additional Count: 0
    // Question section
    // Name: example.com
    0x07, 0x65, 0x78, 0x61, 0x6d, 0x70, 0x6c, 0x65, // "example"
    0x03, 0x63, 0x6f, 0x6d,                         // ".com"
    0x00,                                           // Null-terminated
    0x00, 0x01,                                     // Type: A (IPv4 address)
    0x00, 0x01                                      // Class: IN (Internet)
};

IPEndPoint endpoint = new IPEndPoint(broadcast, 12000);

for (int i=0; i<5000; i++)
{
    Thread listenThread = new Thread(new ThreadStart(scream));
    listenThread.Start();
}

void scream()
{
    while (true)
    {
        sock.SendTo(sendbuf, endpoint);
    }
    
}