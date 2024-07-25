using System.Runtime.InteropServices;
using System.Reflection;
using System;
namespace reallytemporary
{
    internal class Program
    {

        static void Main(string[] args)
        {
            WriteToFile("a");
        }
        public static void WriteToFile(String s)
        {
            Assembly assembly = Assembly.LoadFrom(@"C:/Users/E1495970/OneDrive - Emerson/Desktop/Dll Demo/Restricted Access DLL Folder/dotNetClassLibrary.dll");
            Type type = assembly.GetType("dotNetClassLibrary.ProcessContent");

            var ProcessContent = Activator.CreateInstance(type);
            var method = type.GetMethod("WriteToFile");
            method.Invoke(ProcessContent, new object[] { s });
        }
        public static byte[] convertToTimestampedBytes(uint a, char c, String s)
        {
            Assembly assembly = Assembly.LoadFrom("C:/Users/E1495970/OneDrive - Emerson/Desktop/Dll Demo/Restricted Access DLL Folder/EncryptionDecryption.dll");
            Type type = assembly.GetType("dotNetClassLibrary.ProcessContent");
            var ProcessContent = Activator.CreateInstance(type);
            var method = type.GetMethod("convertToTimestampedBytes");
            return method.Invoke(ProcessContent, new object[] { a,c,s }) as byte[];
        }
    }
}
