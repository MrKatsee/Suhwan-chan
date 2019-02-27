using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static MyServer.NetworkMessage;

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
            isAwake = false;
            Server.ShutDown();
            LogManager.WriteLog("Server Close");
        }

        public static void Send(MessageType messageType, CastingType castingType, string data)
        {
            string message = Encapsulation(messageType, data);
            LogManager.WriteLog("Send Message : " + message);
            switch (castingType)
            {
                case CastingType.BROADCAST:
                    for (int i = 0; i < NetworkConnection.MAXINDEX; ++i)
                    {
                        NetworkConnection conn = NetworkConnection.GetConnection(i);
                        if (conn != null) conn.SendMsg(message);
                    }
                    break;
            }
        }

        public static void Send(MessageType messageType, CastingType castingType, string data, params int[] index)
        {
            string message = Encapsulation(messageType, data);
            LogManager.WriteLog("Send Message : " + message);
            switch (castingType)
            {
                case CastingType.UNICAST:
                    {
                        NetworkConnection conn = NetworkConnection.GetConnection(index[0]);
                        if (conn != null) conn.SendMsg(message);
                        break;
                    }
                case CastingType.XORCAST:
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
                                if (conn != null) conn.SendMsg(message);
                            }
                        }
                        break;
                    }
                case CastingType.MULTICAST:
                    {
                        for (int i = 0; i < index.Length; ++i)
                        {
                            NetworkConnection conn = NetworkConnection.GetConnection(index[i]);
                            if (conn != null) conn.SendMsg(message);
                        }
                        break;
                    }
            }
        }

        public static string Notice<T>(NoticeType noticeType, T value)
        {
            return string.Format("TYPE={0}&VALUE={1}", Enum.GetName(typeof(NoticeType), noticeType), Enum.GetName(typeof(T), value));
        }

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
                        message = message.Substring(message.IndexOf("]") + 1);
                        string[] splitMessage = message.Split(' ');
                        string orderType = splitMessage[0].Replace("/", "").ToUpper();
                        message = splitMessage[1];
                        switch (NetworkMessage.ParseData<MessageType>(orderType))
                        {
                            case NetworkMessage.MessageType.SIGNIN:
                                {
                                    User newUser = new User();
                                    string[] messageData = message.Split('&');
                                    foreach (string datum in messageData)
                                    {
                                        string[] stringData = datum.Split('='); // stringData[0] 은 dataType stringData[1] 은 dataValue
                                        switch (stringData[0].ToUpper())
                                        {
                                            case "ID":
                                                {
                                                    newUser.ID = stringData[1];
                                                    break;
                                                }
                                            case "PW":
                                                {
                                                    newUser.PW = stringData[1];
                                                    break;
                                                }
                                        }
                                    }
                                    if (newUser.isAvaliable)
                                    {
                                        if (ResourceManager.CheckFile(string.Format("{0}/{1}.txt", User.DIRECTORY, newUser.ID)))
                                        {
                                            User targetUser = ResourceManager.LoadFile<User>(string.Format("{0}/{1}.txt", User.DIRECTORY, newUser.ID));
                                            if (targetUser.CheckPassword(newUser.PW))
                                            {
                                                LogManager.WriteLog("New User Sign In : " + newUser.ID);
                                                // 계정 정보 전송
                                                Send(
                                                MessageType.NOTICE,
                                                CastingType.UNICAST,
                                                Notice(NoticeType.LOGINSTATE, LoginState.SIGNIN),
                                                senderIndex);
                                            }
                                            else
                                            {
                                                LogManager.WriteLog("Wrong User Tried To Sign Up : " + newUser.ID + " IP ADDRESS : " + NetworkConnection.GetConnection(senderIndex).Address);
                                                // 패스워드 에러 메시지 전송
                                                Send(
                                                MessageType.NOTICE,
                                                CastingType.UNICAST,
                                                Notice(NoticeType.LOGINSTATE, LoginState.ERROR_WRONGPW),
                                                senderIndex);
                                            }
                                        }
                                        else
                                        {
                                            // 아이디 에러 메시지 전송
                                            Send(
                                                MessageType.NOTICE,
                                                CastingType.UNICAST,
                                                Notice(NoticeType.LOGINSTATE, LoginState.ERROR_WRONGID),
                                                senderIndex);
                                        }
                                    }
                                    else
                                    {
                                        // 에러 메시지 전송
                                        Send(
                                                MessageType.NOTICE,
                                                CastingType.UNICAST,
                                                Notice(NoticeType.LOGINSTATE, LoginState.ERROR_WRONGDATA),
                                                senderIndex);
                                    }
                                    break;
                                }
                            case NetworkMessage.MessageType.SIGNUP:
                                {
                                    LogManager.WriteLog("Try to SignUp");
                                    User newUser = new User();
                                    string[] messageData = message.Split('&');
                                    foreach (string datum in messageData)
                                    {
                                        string[] stringData = datum.Split('='); // stringData[0] 은 dataType stringData[1] 은 dataValue
                                        switch (stringData[0].ToUpper())
                                        {
                                            case "ID":
                                                {
                                                    newUser.ID = stringData[1];
                                                    break;
                                                }
                                            case "PW":
                                                {
                                                    newUser.PW = stringData[1];
                                                    break;
                                                }
                                        }
                                    }
                                    LogManager.WriteLog("Parsed Data ID : " + newUser.ID + " PW : " + newUser.PW);
                                    if (newUser.isAvaliable)
                                    {
                                        if (!ResourceManager.CheckFile(string.Format("{0}/{1}.txt", User.DIRECTORY, newUser.ID)))
                                        {
                                            // 계정 작성중 메시지 전송
                                            Send(
                                                MessageType.NOTICE,
                                                CastingType.UNICAST,
                                                Notice(NoticeType.LOGINSTATE, LoginState.CREATE),
                                                senderIndex);
                                            LogManager.WriteLog("New User Sign Up : " + newUser.ID);
                                            ResourceManager.SaveFile<User>(
                                                string.Format("{0}/{1}.txt", User.DIRECTORY, newUser.ID),
                                                newUser);
                                            // 계정 작성 완료, 접속 완료 메시지 전송
                                            Send(
                                                MessageType.NOTICE,
                                                CastingType.UNICAST,
                                                Notice(NoticeType.LOGINSTATE, LoginState.SIGNIN),
                                                senderIndex);
                                        }
                                        else
                                        {
                                            // 계정 ID가 이미 존재한다는 메시지 전송
                                            Send(
                                                MessageType.NOTICE,
                                                CastingType.UNICAST,
                                                Notice(NoticeType.LOGINSTATE, LoginState.WARNING_EXIST),
                                                senderIndex);
                                        }
                                    }
                                    else
                                    {
                                        // 에러 메시지 전송
                                        Send(
                                                MessageType.NOTICE,
                                                CastingType.UNICAST,
                                                Notice(NoticeType.LOGINSTATE, LoginState.ERROR_WRONGDATA),
                                                senderIndex);
                                        LogManager.WriteLog("Try to SignUp But ERROR_1");
                                    }
                                    break;
                                }
                            case NetworkMessage.MessageType.ERROR:
                                break;
                        }
                    }
                }
                Thread.Sleep(50);
            }
        }

    }
}
