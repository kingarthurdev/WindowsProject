using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace SQLRCE
{

    internal class SQLConnection
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;
        public SQLConnection() {
            initializeServer();
        }
        private void initializeServer() {
            server = "localhost";
            database = "database";
            uid = "root";
            password = "E1495970";  // If I was actually trying to be secure, I'd use a .env file...
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);
           
        }
        private bool openConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

    }
}
