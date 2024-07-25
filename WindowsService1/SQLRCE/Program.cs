using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Threading;
namespace SQLRCE
{
    internal class VulnerableProgram
    {
        //plan:
        /*
         * 
         * random arbitrary machine sends username and password -- checked against vuln database
         * injection occurs, maybe change all passwords 
         * now evil user can sign in and execute whatever command prompt command they want.
         * 
         * */
        static int listeningPort = 2024; //arbitrary
        static int destPort = 2025;

        static void Main(string[] args)
        {
            Console.WriteLine("Data Manager Started.");
            Thread listen = new Thread(listenForCommands);
            listen.Start();
            //Task.Run(listenForCommands);
            //SQLConnection dataConnection = new SQLConnection();
            //Console.WriteLine(dataConnection.VULNERABLEtryLogin("superAdmin", "Thisissuchasecurepassword1234!"));
            Process.GetCurrentProcess().WaitForExit(); // prevents program from immediately exiting on main completion.

        }
        async static void listenForCommands()
        {
            UdpClient listener = new UdpClient(listeningPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listeningPort);
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); 
            byte[] success = Encoding.ASCII.GetBytes("Command Successfully Executed!");
            byte[] fail = Encoding.ASCII.GetBytes("Command Failed!");

            while (true)
            {
                byte[] bytes = listener.Receive(ref groupEP);
                Console.WriteLine("Data Recieved");
                IPEndPoint endpt = new IPEndPoint(groupEP.Address, destPort);

                try
                {
                    Console.WriteLine("Bytes Recieved" + Encoding.ASCII.GetString(bytes));
                    string[] parts = Encoding.UTF8.GetString(bytes).Split(':');
                    
                    int result = executeCommand(parts[0], parts[1], parts[2]);
                    Console.WriteLine(result);
                    if (result == 1)
                    {
                        //sock.SendTo(success, endpt);
                        Console.WriteLine("Success");
                    }
                    else
                    {
                        //sock.SendTo(fail, endpt);
                        Console.WriteLine("Failure");
                    }
                }
                catch
                {
                    //sock.SendTo(fail, endpt);
                    Console.WriteLine("Failure");
                }

            }

        }
        static int executeCommand(string username, string password, string command)
        {
            SQLConnection dataConnection = new SQLConnection();
            if (dataConnection.VULNERABLEtryLogin(username, password))
            {
                ProcessStartInfo start = new ProcessStartInfo("cmd.exe", "/c " + command);
                Process.Start(start);
                return 1;
            }
            return 0;

        }
    }
}