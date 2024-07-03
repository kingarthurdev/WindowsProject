using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace dotnetdeletemetemp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
            {
                System.Security.Cryptography.RSAParameters a = RSA.ExportParameters(true);
                Console.WriteLine(RSA.ExportParameters(true));
            }
        }
    }
}
