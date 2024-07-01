using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace arpspoof
{
    public class Class1
    {
        //You'll need this pinvoke signature as it is not part of the .Net framework
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(int DestIP, int SrcIP,
                                         byte[] pMacAddr, ref uint PhyAddrLen);

        public static void thingy()
        {
             //These vars are needed, if the the request was a success 
             //the MAC address of the host is returned in macAddr
            byte[] macAddr = new byte[6];
            uint macAddrLen;

            //Here you can put the IP that should be checked
            IPAddress Destination = IPAddress.Parse("127.0.0.1");

        //SendARP(1,0, new byte[], 1);

        //Send Request and check if the host is there
        if (SendARP((int) Destination.Address, 0, macAddr, ref macAddrLen) == 0)
        {
            //SUCCESS! Igor it's alive!
        }
        }

}
}
