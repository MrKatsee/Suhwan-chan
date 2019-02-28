using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyServer
{
    public class User
    {
        public static string DIRECTORY = "User";

        public static User ParseData(string data)
        {
            return JsonConvert.DeserializeObject<User>(data);
        }

        public string ID { get; set; }
        public string PW { get; set; }
        public int Win { get; set; }
        public int Lose { get; set; }
        public float Rate
        {
            get
            {
                if (Win + Lose <= 0) return 100f;
                return (float)Win / (Win + Lose) * 100f;
            }
        }

        public bool IsAvaliable
        {
            get
            {
                if (!string.IsNullOrEmpty(ID) && !string.IsNullOrEmpty(PW)) return true;
                else return false;
            }
        }

        public User()
        {
            ID = string.Empty;
            PW = string.Empty;
        }

        public User(string _id, string _pw)
        {
            ID = _id;
            PW = _pw;
        }

        public bool CheckID(string _id)
        {
            return string.Equals(ID, _id);
        }

        public bool CheckPassword(string _pw)
        {
            return string.Equals(PW, _pw);
        }

        public string ToData()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
    }

    public enum CommandType
    {
        ERROR = 0,
        EXIT
    }

    class Program
    {
        static void Main(string[] args)
        {
            ServerManager.Start();
            while (true)
            {
                string input = Console.ReadLine();

                switch (Parse(input))
                {
                    case CommandType.ERROR:
                        {
                            break;
                        }
                    case CommandType.EXIT:
                        {
                            goto EXIT;
                        }
                }
            }
            EXIT:
            ServerManager.Close();
        }

        static CommandType Parse(string content)
        {
            string order = content.ToUpper();
            if (!System.Enum.IsDefined(typeof(CommandType), order)) return default(CommandType);
            return (CommandType)System.Enum.Parse(typeof(CommandType), order);
        }
    }
}
