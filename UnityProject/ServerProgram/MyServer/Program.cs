using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static MyServer.MyEnum;

namespace MyServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerManager.Start();
            while (true)
            {
                string input = Console.ReadLine();

                switch (Parse<CommandType>(input.ToUpper()))
                {
                    case CommandType.ERROR:
                        {
                            break;
                        }
                    case CommandType.EXIT:
                        {
                            goto EXIT;
                        }
                    case CommandType.LIST:
                        {
                            Console.WriteLine(NetworkConnection.GetConnectionInfo());
                            break;
                        }
                }
            }
            EXIT:
            ServerManager.Close();
        }
    }
}
