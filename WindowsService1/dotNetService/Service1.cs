using System;
using System.IO;
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

            WriteToFile("Service started at " + DateTime.Now, ".txt");

            try
            {
                Thread listenThread = new Thread(new ThreadStart(listen));
                listenThread.Start();
            }
            catch (Exception exception)
            {
                WriteToFile(exception.ToString(), ".txt");
            }


        }

        protected override void OnStop()
        {
            WriteToFile("Service stopped at " + DateTime.Now);
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
                WriteToFile($"Received broadcast from {groupEP} :");

                //writes complete message
                WriteToFile($" {total}");

            }
            
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
