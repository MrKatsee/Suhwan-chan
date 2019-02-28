using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System;
using System.Text;
using UnityEngine;
using static NetworkMessage;
using UnityEngine.UI;

public class ClientManager : MonoBehaviour
{
    private static class MyClient
    {
        public static int PORT = 11900;
        public static string ADDRESS;

        public static bool IsConnected { get; private set; }
        private static TcpClient tcpClient;

        private static NetworkStream networkStream;
        private static StreamWriter streamWriter;
        private static StreamReader streamReader;

        private static Thread thread_Connection;
        private static Thread thread_Receiver;

        public static void Init(string _address)
        {
            if (thread_Connection == null || !thread_Connection.IsAlive)
            {
                ADDRESS = _address;
                IsConnected = false;
                tcpClient = new TcpClient();
                thread_Connection = new Thread(Connection);
                thread_Connection.Start();
            }
        }

        private static void Connection()
        {
            int count = 0;
            while (!IsConnected)
            {
                try
                {
                    tcpClient.Connect(ADDRESS, PORT);
                    IsConnected = tcpClient.Connected;
                }
                catch (SocketException e)
                {
                    MyDebug.Log("SocketException : " + e.Message);
                    MyDebug.Log("접속 실패 : 인터넷이 끊겼거나, 서버가 접속 가능한 상태가 아닙니다.");
                    count++;
                    if (count > 10)
                    {
                        IsConnected = false;
                        return;
                    }
                }
                catch (Exception e)
                {
                    NetworkMessage.Enqueue("Exception : " + e.Message);
                }
            }
            MyDebug.Log("서버에 연결되었습니다.");
            InitTCP();
        }

        public static void SendMsg(string str)
        {
            if (IsConnected)
            {
                try
                {
                    streamWriter.WriteLine(str);
                    streamWriter.Flush();
                }
                catch (Exception e)
                {
                    MyDebug.Log(e.Message);
                }
            }
            else
            {
                MyDebug.Log("서버와 접속중이 아닙니다.");
            }
        }

        static void ReceiveMsg()
        {
            string receivedString;
            try
            {
                while (IsConnected)
                {
                    receivedString = streamReader.ReadLine();
                    if (false == string.IsNullOrEmpty(receivedString))
                    {
                        NetworkMessage.SyncEnqueue(receivedString);
                    }
                }
            }
            catch (Exception e)
            {
                MyDebug.Log("서버와의 연결이 끊겼습니다.");
                MyDebug.Log(e.Message);
            }
            ShutDown();
        }

        static void InitTCP()
        {
            tcpClient.NoDelay = true;
            networkStream = tcpClient.GetStream();
            streamWriter = new StreamWriter(networkStream, Encoding.UTF8);
            streamReader = new StreamReader(networkStream, Encoding.UTF8);
            thread_Receiver = new Thread(ReceiveMsg);
            thread_Receiver.Start();
        }

        private static System.Object shutDownLock = new System.Object();
        public static void ShutDown()
        {
            lock (shutDownLock)
            {
                if (IsConnected)
                {
                    IsConnected = false;

                    streamReader.Close();
                    streamWriter.Close();

                    try
                    {
                        tcpClient.Close();
                    }
                    catch (Exception e)
                    {
                        MyDebug.Log("Shut Down: " + e.Message);
                    }
                }
            }
        }
    }

    public static string IPAddress = "127.0.0.1";
    public static string ClientName { get; set; }

    public static ClientManager instance = null;

    public static void Send(MessageType messageType, string data)
    {
        string message = Encapsulation(messageType, data);
        MyClient.SendMsg(message);
    }

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        MyClient.Init(IPAddress);
        StartCoroutine(ConnectSequenceUI());
    }

    IEnumerator ConnectSequenceUI()
    {
        UIManager_Main.instance.ui_Loading.StartLoading("접속 시도 중 : " + MyClient.ADDRESS + ":" + MyClient.PORT);
        yield return new WaitUntil(() => MyClient.IsConnected);
        UIManager_Main.instance.ui_Loading.StopLoading();
    }

    private void LateUpdate()
    {
        int msgCount = NetworkMessage.GetCount();
        if(msgCount > 0)
        {
            for(int loop = 0; loop < msgCount; ++loop)
            {
                string message = NetworkMessage.SyncDequeue();
                MyDebug.Log(message);
                LogManager.WriteLog("Received Message : " + message);
                string[] splitMessage = message.Split(' ');
                string orderType = splitMessage[0].Replace("/", "").ToUpper();
                message = splitMessage[1];
                switch (NetworkMessage.ParseData<MessageType>(orderType))
                {
                    case MessageType.NOTICE:
                        {
                            string[] messageData = message.Split('&');
                            string typeData = string.Empty;
                            string valueData = string.Empty;
                            string noticeData = string.Empty;
                            foreach (string datum in messageData)
                            {
                                string[] stringData = datum.Split('='); // stringData[0] 은 dataType stringData[1] 은 dataValue
                                switch (stringData[0].ToUpper())
                                {
                                    case "TYPE":
                                        {
                                            typeData = stringData[1];
                                            break;
                                        }
                                    case "VALUE":
                                        {
                                            valueData = stringData[1];
                                            break;
                                        }
                                    case "DATA":
                                        {
                                            noticeData = stringData[1];
                                            break;
                                        }
                                }
                            }
                            switch (NetworkMessage.ParseData<NoticeType>(typeData))
                            {
                                case NoticeType.LOGINSTATE:
                                    {
                                        switch (NetworkMessage.ParseData<LoginState>(valueData))
                                        {
                                            case LoginState.CREATE:
                                                {
                                                    UIManager_Main.instance.ui_Loading.StartLoading("계정 작성 중......");
                                                    break;
                                                }
                                            case LoginState.SIGNIN:
                                                {
                                                    PlayManager.Instance.user = User.ParseData(noticeData);
                                                    UIManager_Main.instance.ui_Loading.StopLoading();
                                                    UIManager_Main.instance.ui_Login.SetActive(false);
                                                    UIManager_Main.instance.ui_Toast.MakeToast(PlayManager.Instance.user.ID + "님, 환영합니다!", 3f);
                                                    break;
                                                }
                                            case LoginState.WARNING_EXIST:
                                                {
                                                    UIManager_Main.instance.ui_Loading.StopLoading();
                                                    UIManager_Main.instance.ui_Toast.MakeToast("이미 존재하는 ID입니다.", 3f);
                                                    break;
                                                }
                                            case LoginState.ERROR_ACCESS:
                                                {
                                                    UIManager_Main.instance.ui_Loading.StopLoading();
                                                    UIManager_Main.instance.ui_Toast.MakeToast("이미 접속 중인 ID입니다!", 3f);
                                                    break;
                                                }
                                            case LoginState.ERROR_WRONGID:
                                                {
                                                    UIManager_Main.instance.ui_Loading.StopLoading();
                                                    UIManager_Main.instance.ui_Toast.MakeToast("존재하지 않는 ID입니다.", 3f);
                                                    break;
                                                }
                                            case LoginState.ERROR_WRONGPW:
                                                {
                                                    UIManager_Main.instance.ui_Loading.StopLoading();
                                                    UIManager_Main.instance.ui_Toast.MakeToast("잘못된 비밀번호입니다.", 3f);
                                                    break;
                                                }
                                            case LoginState.ERROR_WRONGDATA:
                                                {
                                                    UIManager_Main.instance.ui_Loading.StopLoading();
                                                    UIManager_Main.instance.ui_Toast.MakeToast("네트워크 연결 상태를 확인해 주세요.", 3f);
                                                    break;
                                                }
                                            case LoginState.ERROR:
                                                break;
                                        }
                                        break;
                                    }
                                case NoticeType.ERROR:
                                    break;
                            }
                            break;
                        }
                    case MessageType.ERROR:
                        break;
                }
            }
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        Send(MessageType.DISCONNECT, "");
        MyClient.ShutDown();
    }

}
