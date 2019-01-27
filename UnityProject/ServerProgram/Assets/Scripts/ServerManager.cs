using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System;
using static NetworkMessage;
using System.IO;
using System.Text;
using System.Linq;

public class ServerManager : MonoBehaviour
{
    private static class MyServer
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
                    MyDebug.Log(e.ToString());
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

    private void Start()
    {
        MyServer.Init();
        LogManager.WriteLog("Server Open : Server Address is " + MyServer.IPADDRESS);
    }

    public static void Send(MessageType messageType, CastingType castingType, string data)
    {
        string message = Encapsulation(messageType, data);
        LogManager.WriteLog("Send MEssage : " + message);
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
        LogManager.WriteLog("Send MEssage : " + message);
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

    private void LateUpdate()
    {
        int msgCount = NetworkMessage.GetCount();
        if(msgCount > 0)
        {
            for(int loop = 0; loop < msgCount; ++loop)
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
                                if(ResourceManager.CheckFile(string.Format("{0}/{1}.txt", User.DIRECTORY, newUser.ID)))
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
    }
    /*
    void LateUpdate()
    {
        int msgCount = ServerMessage.GetCount();
        if (msgCount > 0)
        {
            for (int roop = 0; roop < msgCount; ++roop) // 여기서 메시지를 받아서 처리작업을 수행한다.
            {
                string message = ServerMessage.SyncDequeue();
                //MyDebug.Log("클라에서 보내고 서버에서 받은 메시지 : " + message);
                int senderIndex = Convert.ToInt32(message.Split(']')[0].Replace("[", ""));
                message = message.Substring(message.IndexOf("]") + 1);
                string serverOrder = message.Split(' ')[0].Replace("/", "").ToUpper();
                switch (serverOrder)
                {
                    case "ADMISSION":
                        {
                            string name = message.Substring(message.IndexOf(" ") + 1);
                            NetworkConnection conn = NetworkConnection.GetConnection(senderIndex);
                            conn.SetName(name);
                            Send(
                                MessageType.NOTICE,
                                CastingType.XORCAST,
                                conn.name + " 님이 접속했습니다. " + "( " + NetworkConnection.Count + " / " + NetworkConnection.MAXINDEX + " )",
                                senderIndex
                                );
                            break;
                        }
                    case "WELCOME":
                        {
                            NetworkConnection conn = NetworkConnection.GetConnection(senderIndex);
                            if (senderIndex == 0)
                            {
                                Send(
                                    MessageType.NOTICE,
                                    CastingType.UNICAST,
                                    "서버를 성공적으로 개설하였습니다 : IP 주소는 " + Server.IPADDRESS + "입니다.",
                                    senderIndex
                                    );
                            }
                            else
                            {
                                Send(
                                    MessageType.NOTICE,
                                    CastingType.UNICAST,
                                    conn.name + " 님, 환영합니다.",
                                    senderIndex
                                    );
                            }
                            break;
                        }
                    case "MESSAGE":
                        {
                            string data = message.Substring(message.IndexOf(" ") + 1);
                            NetworkConnection conn = NetworkConnection.GetConnection(senderIndex);
                            Send(
                                MessageType.MESSAGE,
                                CastingType.BROADCAST,
                                conn.name + " : " + data);
                            break;
                        }
                    case "GOODBYE":
                        {
                            string name = message.Substring(message.IndexOf(" ") + 1);
                            Send(
                                MessageType.OBJECT,
                                CastingType.XORCAST,
                                "DESTROY " + NetworkObjectData.DestroyObjectByAuthorIndex(senderIndex),
                                senderIndex);
                            Send(
                                MessageType.NOTICE,
                                CastingType.XORCAST,
                                name + " 님이 접속을 종료했습니다.",
                                senderIndex
                                );
                            break;
                        }
                    case "OBJECT":
                        {
                            string data = message.Substring(message.IndexOf(" ") + 1);
                            string order = data.Split(' ')[0].ToUpper();
                            switch (order)
                            {
                                case "CREATE":
                                    {
                                        data = data.Substring(data.IndexOf(" ") + 1);
                                        string name = data.Substring(0, data.IndexOf(" "));
                                        string createData = data.Substring(data.IndexOf(" ") + 1);
                                        NetworkObjectData newData = NetworkObjectData.GetObject(NetworkObjectData.CreateObject(name, senderIndex));
                                        newData.defaultData = createData;
                                        float randomX = UnityEngine.Random.Range(-5f, 5f);
                                        float randomZ = UnityEngine.Random.Range(-5f, 5f);
                                        Send(MessageType.OBJECT, CastingType.XORCAST, "CREATE " + newData.name + " " + newData.defaultData +
                                            " transform[(" + randomX + ",5.5," + randomZ + ")]",
                                            senderIndex);
                                        Send(MessageType.OBJECT, CastingType.UNICAST, "CREATEAS " + newData.name + " " + newData.defaultData +
                                            " transform[(" + randomX + ",5.5," + randomZ + ")]", senderIndex);
                                        break;
                                    }
                                case "UPDATE":
                                    {
                                        data = data.Substring(data.IndexOf(" ") + 1);
                                        string name = data.Substring(0, data.IndexOf(" "));
                                        string updateData = data.Substring(data.IndexOf(" ") + 1);
                                        if (NetworkObjectData.IsEnable(name))
                                        {
                                            NetworkObjectData networkObject = NetworkObjectData.GetObject(name);
                                            if (networkObject.authorIndex == senderIndex)
                                            {
                                                networkObject.updateData = updateData;
                                                Send(MessageType.OBJECT, CastingType.XORCAST, "UPDATE " + name + " " + updateData, senderIndex);
                                            }
                                        }
                                        break;
                                    }
                                case "DESTROY":
                                    {
                                        data = data.Substring(data.IndexOf(" ") + 1);
                                        string name = data.Substring(0, data.IndexOf(" "));
                                        if (NetworkObjectData.IsEnable(name))
                                        {
                                            NetworkObjectData.DestroyObject(name);
                                            Send(MessageType.OBJECT, CastingType.XORCAST, "DESTROY " + name, senderIndex);
                                        }
                                        break;
                                    }
                                case "SYNCHRONIZE":
                                    {
                                        Send(MessageType.OBJECT, CastingType.UNICAST, "SYNCHRONIZE " + NetworkObjectData.SerializeAllData(), senderIndex);
                                        break;
                                    }
                            }
                            break;
                        }
                }
            }
        }
    }*/

    private void OnDestroy()
    {
        //Send(MessageType.NOTICE, CastingType.BROADCAST, "서버와의 연결이 종료되었습니다.");
        MyServer.ShutDown();
        LogManager.WriteLog("Server ShutDown");
    }
}
