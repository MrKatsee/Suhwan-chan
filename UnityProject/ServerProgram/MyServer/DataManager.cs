using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace MyServer
{
    class DataManager
    {
        public static string url = "SERVER=LOCALHOST;USER=;DATABASE=;PORT=3306;PASSWORD='SSLMODE=NONE";

        private static MySqlConnection mConnection;
        private static MySqlCommand mCommand;
        private static MySqlDataReader mDataReader;

        private static bool isInitalized = false;

        public static void Init()
        {
            mConnection = new MySqlConnection(url);
            mCommand = new MySqlCommand();
            mCommand.Connection = mConnection;
        }

        private static object lockObject = new object();
        public static void Execute(string url)
        {
            lock (lockObject)
            {
                if (!isInitalized) Init();
                mCommand.CommandText = url;
                mConnection.Open();
                mDataReader = mCommand.ExecuteReader();
                while (mDataReader.Read())
                {

                }
                mConnection.Close();
            }
        }
    }
}
