using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MyServer.MyEnum;

namespace MyServer
{
    class Login
    {
        public MyUser userData;
        public NetworkConnection userConnection;

        public Login(MyUser _userData, int senderIndex)
        {
            userData = _userData;
            userConnection = NetworkConnection.GetConnection(senderIndex);
            userConnection.user = userData;
            userConnection.OnDisConnected += OnDisConnected;
        }

        private void OnDisConnected(NetworkConnection connection)
        {
            LoginManager.OnUserDisconnected(this);
        }

    }

    static class LoginManager
    {
        private static List<Login> loginInfo = new List<Login>();

        private static bool IsUserConnected(MyUser _userData)
        {
            lock (loginInfo)
            {
                for (int i = 0; i < loginInfo.Count; ++i)
                {
                    if (loginInfo[i].userData.CheckID(_userData.ID))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public static void OnUserConnected(Login _loginData)
        {
            lock (loginInfo)
            {
                bool isAdded = false;
                for(int i = 0; i < loginInfo.Count; ++i)
                {
                    if(loginInfo[i] == null)
                    {
                        isAdded = true;
                        loginInfo.Insert(i, _loginData);
                        break;
                    }
                }
                if (!isAdded) loginInfo.Add(_loginData);
            }
        }

        public static void OnUserDisconnected(Login _loginData)
        {
            lock (loginInfo)
            {
                loginInfo.Remove(_loginData);
            }
        }

        public static void Execute(int senderIndex, string message)
        {
            string loginType = message;
            string messageData = string.Empty;
            if(message.Contains(' '))
            {
                loginType = message.Substring(0, message.IndexOf(' '));
                messageData = message.Substring(message.IndexOf(' ') + 1);
            }
            switch (Parse<LoginType>(loginType))
            {
                case LoginType.DEFAULT: break;
                case LoginType.SIGNIN:
                    {
                        MyUser userData = MyUser.ParseData(messageData);
                        if (userData == null)
                        {
                            // 데이터 이상
                            
                            break;
                        }
                        if (MyUser.CheckUserData(userData.ID))
                        {
                            MyUser newUser = MyUser.LoadUserData(userData.ID);
                            if (newUser.CheckPassword(userData.PW))
                            {
                                if (IsUserConnected(userData))
                                {
                                    // 이미 접속 중인 ID
                                    LogManager.WriteLog("Warning! Already Accessed Account : " + userData.ID + "IP ADDRESS : " + NetworkConnection.GetConnection(senderIndex).Address);
                                    ServerManager.Send("/" + MessageType.ERROR.ToString() + " " + ErrorType.ACCESS.ToString(), CastType.UNICAST, senderIndex);
                                }
                                else
                                {
                                    // 로그인
                                    LogManager.WriteLog("New User Sign In : " + userData.ID + " IP ADDRESS : " + NetworkConnection.GetConnection(senderIndex).Address);
                                    OnUserConnected(new Login(newUser, senderIndex));
                                    ServerManager.Send("/" + MessageType.LOGIN.ToString() + " " + LoginType.SIGNIN.ToString() + " " + newUser.ToData(), CastType.UNICAST, senderIndex);
                                }
                            }
                            else
                            {
                                // 비밀 번호 오류
                                LogManager.WriteLog("Wrong User Tried To Sign In : " + userData.ID + " IP ADDRESS : " + NetworkConnection.GetConnection(senderIndex).Address);
                                ServerManager.Send("/" + MessageType.ERROR.ToString() + " " + ErrorType.WRONGPW.ToString(), CastType.UNICAST, senderIndex);
                            }
                        }
                        else
                        {
                            // 존재하지 않는 ID
                            ServerManager.Send("/" + MessageType.ERROR.ToString() + " " + ErrorType.WRONGID.ToString(), CastType.UNICAST, senderIndex);
                        }
                        break;
                    }
                case LoginType.SIGNUP:
                    {
                        MyUser userData = MyUser.ParseData(messageData);
                        if (userData == null)
                        {
                            // 데이터 이상

                            break;
                        }
                        if (MyUser.CheckUserData(userData.ID))
                        {
                            // 이미 존재하는 ID
                            ServerManager.Send("/" + MessageType.ERROR.ToString() + " " + ErrorType.EXIST.ToString(), CastType.UNICAST, senderIndex);
                        }
                        else
                        {
                            // 회원 가입
                            LogManager.WriteLog("New User Sign Up : " + userData.ID + " IP ADDRESS : " + NetworkConnection.GetConnection(senderIndex).Address);
                            MyUser newUser = new MyUser(userData.ID, userData.PW);
                            MyUser.SaveUserData(newUser);
                            OnUserConnected(new Login(newUser, senderIndex));
                            ServerManager.Send("/" + MessageType.LOGIN.ToString() + " " + LoginType.SIGNIN.ToString() + " " + newUser.ToData(), CastType.UNICAST, senderIndex);
                        }
                        break;
                    }
            }
        }
    }
}
