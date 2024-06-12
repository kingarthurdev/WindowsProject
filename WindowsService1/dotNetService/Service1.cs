using System;
using System.Net.Sockets;
using System.Net;
using System.ServiceProcess;
using System.Threading;
using dotNetClassLibrary;


namespace dotNetService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {

            ProcessContent.WriteToFile("Service started at " + DateTime.Now, ".txt");

            try
            {
                Thread listenThread = new Thread(new ThreadStart(listen));
                listenThread.Start();
            }
            catch (Exception exception)
            {
                ProcessContent.WriteToFile(exception.ToString(), ".txt");
            }


        }

        protected override void OnStop()
        {
            ProcessContent.WriteToFile("Service stopped at " + DateTime.Now);
        }

        public static void listen()
        {
            UdpClient listener = new UdpClient(12000);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 12000);
            while (true)
            {

                byte[] bytes = listener.Receive(ref groupEP);
                //string representation = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                (uint num, char delim, string message) = ProcessContent.convertFromByteArray(bytes);


                string total = num+""+delim+message;

                //console log where the data came from in the format ipaddr:port
                ProcessContent.WriteToFile($"Received broadcast from {groupEP} :");
                ProcessContent.WriteToFile($" {total}");

            }
            
        }

        
    }
}
