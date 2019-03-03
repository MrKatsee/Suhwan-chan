using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static MyServer.NetworkMessage;
using static MyServer.MyEnum;

namespace MyServer
{
    static class ServerManager
    {
        private static class Server
        {
            static int PORT = 11900;
            public static string IPADDRESS = string.Empty;
            public static bool IsOpen { get; private set; }

            private static Socket socketTCP;
            private static TcpListener tcpListener;
            private static Thread thread_Connection;
            private static IPEndPoint ipEndPoint;

            public static void Init()
            {
                if (thread_Connection == null || !thread_Connection.IsAlive)
                {
                    IsOpen = true;
                    thread_Connection = new Thread(Connection);
                    thread_Connection.IsBackground = true;
                    thread_Connection.Start();
                    IPADDRESS = Convert.ToString(Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(address => address.AddressFamily == AddressFamily.InterNetwork));
                }
            }

            private static void Connection()
            {
                ipEndPoint = new IPEndPoint(IPAddress.Any, PORT);
                tcpListener = new TcpListener(ipEndPoint);
                tcpListener.Start();
                while (IsOpen)
                {
                    try
                    {
                        socketTCP = tcpListener.AcceptSocket();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    InitTCP(socketTCP);
                    socketTCP = null;
                }
            }

            private static void InitTCP(Socket socket)
            {
                NetworkConnection.CreateConnection(socket);
            }

            public static void ShutDown()
            {
                IsOpen = false;
                NetworkConnection.Clear();
            }
        }

        private static bool isAwake = false;
        private static Thread thread_Update;

        public static void Start()
        {
            Server.Init();
            isAwake = true;
            thread_Update = new Thread(Update);
            thread_Update.IsBackground = true;
            thread_Update.Start();
            LogManager.WriteLog("Server Open : Server Address is " + Server.IPADDRESS);
        }

        public static void Close()
        {
            //Send(MessageType.NOTICE, CastingType.BROADCAST, "서버와의 연결이 종료되었습니다.");
            ServerManager.Send("/" + MessageType.ERROR.ToString() + " " + ErrorType.SHUTDOWN.ToString(), CastType.BROADCAST);
            isAwake = false;
            Server.ShutDown();
            LogManager.WriteLog("Server Close");
        }

        public static void Send(string data, CastType castType)
        {
            LogManager.WriteLog("Send Message : " + data);
            switch (castType)
            {
                case CastType.BROADCAST:
                    for (int i = 0; i < NetworkConnection.MAXINDEX; ++i)
                    {
                        NetworkConnection conn = NetworkConnection.GetConnection(i);
                        if (conn != null) conn.SendMsg(data);
                    }
                    break;
            }
        }

        public static void Send(string data, CastType castType, params int[] index)
        {
            LogManager.WriteLog("Send Message : " + data);
            switch (castType)
            {
                case CastType.UNICAST:
                    {
                        NetworkConnection conn = NetworkConnection.GetConnection(index[0]);
                        if (conn != null) conn.SendMsg(data);
                        break;
                    }
                case CastType.XORCAST:
                    {
                        for (int i = 0; i < NetworkConnection.MAXINDEX; ++i)
                        {
                            bool isExcpetion = false;
                            for (int j = 0; j < index.Length; ++j)
                            {
                                if (index[j] == i)
                                {
                                    isExcpetion = true;
                                    break;
                                }
                            }
                            if (!isExcpetion)
                            {
                                NetworkConnection conn = NetworkConnection.GetConnection(i);
                                if (conn != null) conn.SendMsg(data);
                            }
                        }
                        break;
                    }
                case CastType.MULTICAST:
                    {
                        for (int i = 0; i < index.Length; ++i)
                        {
                            NetworkConnection conn = NetworkConnection.GetConnection(index[i]);
                            if (conn != null) conn.SendMsg(data);
                        }
                        break;
                    }
            }
        }

        // 메시지는 /[senderIndex]MessageType MessageType_2 data 로 이루어진다.

        public static void Update()
        {
            while (isAwake)
            {
                int msgCount = NetworkMessage.GetCount();
                if (msgCount > 0)
                {
                    for (int loop = 0; loop < msgCount; ++loop)
                    {
                        string message = NetworkMessage.SyncDequeue();
                        LogManager.WriteLog("Received Message : " + message);
                        int senderIndex = Convert.ToInt32(message.Split(']')[0].Replace("[", ""));
                        message = message.Replace("/", "").Substring(message.IndexOf(']') + 1);
                        string messageType = message.Substring(0, message.IndexOf(' '));
                        message = message.Substring(message.IndexOf(' ') + 1);
                        switch (Parse<MessageType>(messageType))
                        {
                            case MessageType.DEFAULT: break;
                            case MessageType.NOTICE:
                                {
                                    string noticeType = message;
                                    string messageData = string.Empty;
                                    if(message.Contains(' '))
                                    {
                                        noticeType = message.Substring(0, message.IndexOf(' '));
                                        messageData = message.Substring(message.IndexOf(' ') + 1);
                                    }
                                    switch (Parse<NoticeType>(noticeType))
                                    {
                                        case NoticeType.DEFAULT: break;
                                        case NoticeType.SYNCUSER:
                                            {
                                                Send("/" + MessageType.NOTICE.ToString() + " " + NoticeType.SYNCUSER.ToString() + " " + NetworkConnection.GetConnection(senderIndex).user.ToSecuredData(), CastType.UNICAST, senderIndex);
                                                break;
                                            }
                                        case NoticeType.DISCONNECT:
                                            {
                                                NetworkConnection.GetConnection(senderIndex).ShutDown();
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case MessageType.LOGIN:
                                {
                                    // 로그인과 관련된 메시지는 모두 LoginManager에게 맡긴다.
                                    LoginManager.Execute(senderIndex, message);
                                    break;
                                }
                            case MessageType.ERROR:
                                {
                                    string errorType = message;
                                    string messageData = string.Empty;
                                    if (message.Contains(' '))
                                    {
                                        errorType = message.Substring(0, message.IndexOf(' '));
                                        messageData = message.Substring(message.IndexOf(' ') + 1);
                                    }
                                    switch (Parse<ErrorType>(errorType))
                                    {
                                        case ErrorType.DEFAULT: break;
                                        case ErrorType.SHUTDOWN:
                                            {
                                                NetworkConnection.GetConnection(senderIndex).ShutDown();
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case MessageType.MATCH:
                                {
                                    // 매치와 관련된 메시지는 모두 MatchManager에게 맡긴다.
                                    MatchManager.Execute(senderIndex, message);
                                    break;
                                }
                        }
                    }
                }
                Thread.Sleep(50);   // 메시지의 처리는 0.05초 당 한 번 돌리면 되겠지.
            }
        }

    }
}
