using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace SQLRCE
{

    public class SQLConnection
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
        private bool closeConnection()
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
        public bool VULNERABLEtryLogin(string username, string password) // This function will be vulnerable to sql injection!
        {
            //select isAdmin from users where username = username and passwordThatShouldHaveBeenHashed = "password";
            // a malicious pass would be this: "; update users set passwordThatShouldHaveBeenHashed = "Thisissuchasecurepassword1234" where id = 1-- 

            string query = "select isAdmin from users where username = \""+username+ "\" and passwordThatShouldHaveBeenHashed = \""+password+"\"";
            Console.WriteLine("full query: "+query);


            //Open connection
            if (this.openConnection() == true)
            {
                //create mysql command
                MySqlCommand cmd = new MySqlCommand();
                //Assign the query using CommandText
                cmd.CommandText = query;
                //Assign the connection using Connection
                cmd.Connection = connection;

                //Execute query and get results
                MySqlDataReader dataReader = cmd.ExecuteReader();

                string data = "";
                while (dataReader.Read())
                {
                    string temp = dataReader[0].ToString();
                    data += temp;
                }

                if(data.Length ==1 && int.Parse(data) == 1)
                {
                    return true;
                }
                //close connection*/
                this.closeConnection();

            }
            return false;
            
        }

        public bool safeLogin(string username, string password)
        {

            string query = "select isAdmin from users where username = @USERNAME and passwordThatShouldHaveBeenHashed = @PASSWORD";


            //Open connection
            if (this.openConnection() == true)
            {
                //create mysql command
                MySqlCommand cmd = new MySqlCommand();
                //Assign the query using CommandText
                cmd.CommandText = query;
                //Assign the connection using Connection
                cmd.Connection = connection;

                cmd.Parameters.AddWithValue("@USERNAME", username);
                cmd.Parameters.AddWithValue("@PASSWORD", password);


                //Execute query and get results
                MySqlDataReader dataReader = cmd.ExecuteReader();

                string data = "";
                while (dataReader.Read())
                {
                    string temp = dataReader[0].ToString();
                    data += temp;
                }

                if (data.Length == 1 && int.Parse(data) == 1)
                {
                    return true;
                }
                //close connection*/
                this.closeConnection();

            }
            return false;

        }

    }
}
