using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace sqlfilter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Listening for traffic on port 2025, filtering out sql, then forwarding to vuln service on port 2024");
            //Console.WriteLine(sanitize("superAdmin:Thisissuchasecurepassword123:curl parrot.live"));
            startProxy();
        }
        private static void startProxy()
        {
            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint dst = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2024);
            UdpClient listener = new UdpClient(2025);
            IPEndPoint anywhere = new IPEndPoint(IPAddress.Any, 2025);

            while (true)
            {
                byte[] recieved = listener.Receive(ref anywhere);
                Console.WriteLine(Encoding.ASCII.GetString(recieved));
                Console.WriteLine(sanitize(Encoding.ASCII.GetString(recieved)));
                recieved = Encoding.ASCII.GetBytes(sanitize(Encoding.ASCII.GetString(recieved)));
                sender.SendTo(recieved, dst);
                Console.WriteLine("Data Sent");
            }
        }
        private static string sanitize(string unsanitizedString)
        {
            string[] SqlInjectionKeywords = new string[]
            {
                "SELECT", "FROM", "WHERE", "JOIN","INSERT", "UPDATE", "DELETE",
                "CREATE", "DROP", "ALTER", "TABLE", "UNION", "ALL", "ANY", "NOT",
                "NULL", "AND", "OR", "BETWEEN", "LIKE", "CASE", "WHEN", "THEN", "ELSE", "END", "SET", "VALUES", "RETURN", 
                "TRUNCATE", "RENAME", "GRANT", "REVOKE", "WITH", "OPTION", "LIMIT", "OFFSET", "FETCH", "TOP", "ROWNUM",
                "UPDATE", "REPLACE", "LIKE", "CHAR", "ASCII", "UNION", "BULK INSERT", "BACKUP", "RESTORE", "GRANT", "REVOKE", "--", "___","/*"
            };

            foreach (string s in SqlInjectionKeywords)
            {
                unsanitizedString = ReplaceString(unsanitizedString, s + " ", "", StringComparison.OrdinalIgnoreCase);
            }
            foreach (string s in SqlInjectionKeywords)
            {
                unsanitizedString = ReplaceString(unsanitizedString, " " + s, "", StringComparison.OrdinalIgnoreCase);
            }
            foreach (string s in SqlInjectionKeywords)
            {
                unsanitizedString = ReplaceString(unsanitizedString, s + "/*", "", StringComparison.OrdinalIgnoreCase);
            }
            foreach (string s in SqlInjectionKeywords)
            {
                unsanitizedString = ReplaceString(unsanitizedString, "*/" + s, "", StringComparison.OrdinalIgnoreCase);
            }
            unsanitizedString = ReplaceString(unsanitizedString, "*/" , "", StringComparison.OrdinalIgnoreCase);
            unsanitizedString = ReplaceString(unsanitizedString, "/*", "", StringComparison.OrdinalIgnoreCase);
            unsanitizedString = ReplaceString(unsanitizedString, "--", "", StringComparison.OrdinalIgnoreCase);


            return unsanitizedString;

        }

        // code pulled from stackoverflow
        public static string ReplaceString(string str, string oldValue, string newValue, StringComparison comparison)
        {
            StringBuilder sb = new StringBuilder();

            int previousIndex = 0;
            int index = str.IndexOf(oldValue, comparison);
            while (index != -1)
            {
                sb.Append(str.Substring(previousIndex, index - previousIndex));
                sb.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                index = str.IndexOf(oldValue, index, comparison);
            }
            sb.Append(str.Substring(previousIndex));

            return sb.ToString();
        }

    }
}
