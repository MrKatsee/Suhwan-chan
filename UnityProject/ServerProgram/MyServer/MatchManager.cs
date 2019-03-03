using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static MyServer.MyEnum;

namespace MyServer
{
    class Match
    {
        private const int MAX_USER = 2;

        private class MatchState
        {
            private int index;
            public MyUser userData { get; private set; }
            public NetworkConnection userConnection { get; private set; }
            Dictionary<string, object> variables;

            public MatchState(int _index, MyUser _userData, NetworkConnection _userConnection, Match _match)
            {
                index = _index;
                userData = _userData;
                userConnection = _userConnection;
                userConnection.OnDisConnected += _match.OnDisConnected;
                variables = new Dictionary<string, object>();
                SetVar("Ready", false);
                SetVar("TimeEnd", false);
                SetVar("Battle", -1);
            }

            public object GetVar(string key)
            {
                if (variables.ContainsKey(key)) return variables[key];
                return null;
            }

            public void SetVar(string key, object values)
            {
                if (variables.ContainsKey(key)) variables[key] = values;
                else variables.Add(key, values);
            }
        }

        private MatchState[] matchStates;

        public Match(MyUser newUser_1, MyUser newUser_2)
        {
            matchStates = new MatchState[MAX_USER];
            matchStates[0] = new MatchState(0, newUser_1, NetworkConnection.GetConnectionByUser(newUser_1), this);
            matchStates[1] = new MatchState(1, newUser_2, NetworkConnection.GetConnectionByUser(newUser_2), this);
            ServerManager.Send(
                "/" + MessageType.MATCH.ToString() + " " + MatchType.START + " " + matchStates[0].userData.ToSecuredData(),
                CastType.UNICAST,
                GetMatchStateByIndex(1).userConnection.index);
            ServerManager.Send(
                "/" + MessageType.MATCH.ToString() + " " + MatchType.START + " " + matchStates[1].userData.ToSecuredData(),
                CastType.UNICAST,
                GetMatchStateByIndex(0).userConnection.index);
        }

        public bool CheckIndex(int index)
        {
            for(int i = 0; i < matchStates.Length; ++i)
            {
                if (matchStates[i] == null) continue;
                if (matchStates[i].userConnection.index == index) return true;
            }
            return false;
        }

        private MatchState GetMatchStateByIndex(int index)
        {
            if (0 <= index && index < matchStates.Length) return matchStates[index];
            return null;
        }

        private MatchState GetMatchStateByConnectionIndex(int index)
        {
            for (int i = 0; i < matchStates.Length; ++i)
            {
                if (matchStates[i] == null) continue;
                if (matchStates[i].userConnection.index == index) return matchStates[i];
            }
            return null;
        }

        public void Send(string data)
        {
            int[] userIndex = new int[MAX_USER];
            for(int i = 0; i < MAX_USER; ++i)
            {
                if (matchStates[i] == null) continue;
                userIndex[i] = matchStates[i].userConnection.index;
            }
            ServerManager.Send(data, CastType.MULTICAST, userIndex);
        }

        public void Send(string data, CastType castType, params int[] index)
        {
            List<int> checkIndex = new List<int>();
            for (int i = 0; i < index.Length; ++i)
            {
                if (!CheckIndex(index[i])) continue;
                checkIndex.Add(index[i]);
            }
            List<int> receiverIndex = new List<int>();
            switch (castType)
            {
                case CastType.UNICAST:
                    {
                        for(int i = 0; i < matchStates.Length; ++i)
                        {
                            for(int j = 0; j < checkIndex.Count; ++j)
                            {
                                if(matchStates[i].userConnection.index == checkIndex[j])
                                {
                                    receiverIndex.Add(checkIndex[j]);
                                }
                            }
                        }
                        break;
                    }
                case CastType.XORCAST:
                    {
                        for (int i = 0; i < matchStates.Length; ++i)
                        {
                            bool isXOR = true;
                            for (int j = 0; j < checkIndex.Count; ++j)
                            {
                                if (matchStates[i].userConnection.index == checkIndex[j])
                                {
                                    isXOR = false;
                                    break;
                                }
                            }
                            if (isXOR) receiverIndex.Add(matchStates[i].userConnection.index);
                        }
                        break;
                    }
            }
            if(receiverIndex.Count > 0)
            {
                ServerManager.Send(data, CastType.UNICAST, receiverIndex.ToArray());
            }
        }

        public void Execute(int senderIndex, MatchType matchType, string data)
        {
            switch (matchType)
            {
                case MatchType.READY:
                    {
                        GetMatchStateByConnectionIndex(senderIndex).SetVar("Ready", true);
                        int readyCount = 0;
                        for(int i = 0; i < MAX_USER; ++i)
                        {
                            if ((bool)matchStates[i].GetVar("Ready")) readyCount++;
                        }
                        if(readyCount >= MAX_USER)
                        {
                            Send("/" + MessageType.MATCH.ToString() + " " + MatchType.WAIT.ToString() + " 8");
                        }
                        break;
                    }
                case MatchType.HAND:
                    {
                        if (!(bool)GetMatchStateByConnectionIndex(senderIndex).GetVar("TimeEnd"))
                        {
                            GetMatchStateByConnectionIndex(senderIndex).SetVar("Battle", Convert.ToInt32(data));
                            Send("/" + MessageType.MATCH.ToString() + " " + MatchType.HAND.ToString() + " " + ((int)GetMatchStateByConnectionIndex(senderIndex).GetVar("Battle")).ToString(), CastType.XORCAST, senderIndex);
                        }
                        break;
                    }
                case MatchType.BATTLE:
                    {
                        GetMatchStateByConnectionIndex(senderIndex).SetVar("TimeEnd", true);
                        int timeCount = 0;
                        for (int i = 0; i < MAX_USER; ++i)
                        {
                            if ((bool)matchStates[i].GetVar("TimeEnd")) timeCount++;
                        }
                        if (timeCount >= MAX_USER)
                        {
                            int p0 = (int)GetMatchStateByIndex(0).GetVar("Battle");
                            int p1 = (int)GetMatchStateByIndex(1).GetVar("Battle");
                            int p0index = GetMatchStateByIndex(0).userConnection.index;
                            int p1index = GetMatchStateByIndex(1).userConnection.index;
                            Send("/" + MessageType.MATCH.ToString() + " " + MatchType.HAND.ToString() + " " + p1.ToString(), CastType.UNICAST, p0index);
                            Send("/" + MessageType.MATCH.ToString() + " " + MatchType.HAND.ToString() + " " + p0.ToString(), CastType.UNICAST, p1index);
                            if(p0 == 0)
                            {
                                if(p1 == 0)
                                {
                                    GetMatchStateByIndex(0).SetVar("Battle", -1);
                                    GetMatchStateByIndex(1).SetVar("Battle", -1);
                                    GetMatchStateByIndex(0).SetVar("TimeEnd", false);
                                    GetMatchStateByIndex(1).SetVar("TimeEnd", false);
                                    Send("/" + MessageType.MATCH.ToString() + " " + MatchType.DRAW.ToString());
                                }
                                else if(p1 == 1)
                                {
                                    GetMatchStateByIndex(0).userData.Lose++;
                                    MyUser.SaveUserData(GetMatchStateByIndex(0).userData);
                                    GetMatchStateByIndex(1).userData.Win++;
                                    MyUser.SaveUserData(GetMatchStateByIndex(1).userData);
                                    Send("/" + MessageType.MATCH.ToString() + " " + MatchType.LOSE.ToString(), CastType.UNICAST, p0index);
                                    Send("/" + MessageType.MATCH.ToString() + " " + MatchType.WIN.ToString(), CastType.UNICAST, p1index);
                                    MatchManager.EndMatch(this);
                                }
                                else if(p1 == 2)
                                {
                                    GetMatchStateByIndex(0).userData.Win++;
                                    MyUser.SaveUserData(GetMatchStateByIndex(0).userData);
                                    GetMatchStateByIndex(1).userData.Lose++;
                                    MyUser.SaveUserData(GetMatchStateByIndex(1).userData);
                                    Send("/" + MessageType.MATCH.ToString() + " " + MatchType.WIN.ToString(), CastType.UNICAST, p0index);
                                    Send("/" + MessageType.MATCH.ToString() + " " + MatchType.LOSE.ToString(), CastType.UNICAST, p1index);
                                    MatchManager.EndMatch(this);
                                }
                            }
                            else if(p0 == 1)
                            {
                                if (p1 == 0)
                                {
                                    GetMatchStateByIndex(0).userData.Win++;
                                    MyUser.SaveUserData(GetMatchStateByIndex(0).userData);
                                    GetMatchStateByIndex(1).userData.Lose++;
                                    MyUser.SaveUserData(GetMatchStateByIndex(1).userData);
                                    Send("/" + MessageType.MATCH.ToString() + " " + MatchType.WIN.ToString(), CastType.UNICAST, p0index);
                                    Send("/" + MessageType.MATCH.ToString() + " " + MatchType.LOSE.ToString(), CastType.UNICAST, p1index);
                                    MatchManager.EndMatch(this);
                                }
                                else if (p1 == 1)
                                {
                                    GetMatchStateByIndex(0).SetVar("Battle", -1);
                                    GetMatchStateByIndex(1).SetVar("Battle", -1);
                                    GetMatchStateByIndex(0).SetVar("TimeEnd", false);
                                    GetMatchStateByIndex(1).SetVar("TimeEnd", false);
                                    Send("/" + MessageType.MATCH.ToString() + " " + MatchType.DRAW.ToString());
                                }
                                else if (p1 == 2)
                                {
                                    GetMatchStateByIndex(0).userData.Lose++;
                                    MyUser.SaveUserData(GetMatchStateByIndex(0).userData);
                                    GetMatchStateByIndex(1).userData.Win++;
                                    MyUser.SaveUserData(GetMatchStateByIndex(1).userData);
                                    Send("/" + MessageType.MATCH.ToString() + " " + MatchType.LOSE.ToString(), CastType.UNICAST, p0index);
                                    Send("/" + MessageType.MATCH.ToString() + " " + MatchType.WIN.ToString(), CastType.UNICAST, p1index);
                                    MatchManager.EndMatch(this);
                                }
                            }
                            else if (p0 == 2)
                            {
                                if (p1 == 0)
                                {
                                    GetMatchStateByIndex(0).userData.Lose++;
                                    MyUser.SaveUserData(GetMatchStateByIndex(0).userData);
                                    GetMatchStateByIndex(1).userData.Win++;
                                    MyUser.SaveUserData(GetMatchStateByIndex(1).userData);
                                    Send("/" + MessageType.MATCH.ToString() + " " + MatchType.LOSE.ToString(), CastType.UNICAST, p0index);
                                    Send("/" + MessageType.MATCH.ToString() + " " + MatchType.WIN.ToString(), CastType.UNICAST, p1index);
                                    MatchManager.EndMatch(this);
                                }
                                else if (p1 == 1)
                                {
                                    GetMatchStateByIndex(0).userData.Win++;
                                    MyUser.SaveUserData(GetMatchStateByIndex(0).userData);
                                    GetMatchStateByIndex(1).userData.Lose++;
                                    MyUser.SaveUserData(GetMatchStateByIndex(1).userData);
                                    Send("/" + MessageType.MATCH.ToString() + " " + MatchType.WIN.ToString(), CastType.UNICAST, p0index);
                                    Send("/" + MessageType.MATCH.ToString() + " " + MatchType.LOSE.ToString(), CastType.UNICAST, p1index);
                                    MatchManager.EndMatch(this);
                                }
                                else if (p1 == 2)
                                {
                                    GetMatchStateByIndex(0).SetVar("Battle", -1);
                                    GetMatchStateByIndex(1).SetVar("Battle", -1);
                                    GetMatchStateByIndex(0).SetVar("TimeEnd", false);
                                    GetMatchStateByIndex(1).SetVar("TimeEnd", false);
                                    Send("/" + MessageType.MATCH.ToString() + " " + MatchType.DRAW.ToString());
                                }
                            }
                        }
                        break;
                    }
            }
        }

        private void OnDisConnected(NetworkConnection networkConnection)
        {
            int index = networkConnection.index;
            for(int i = 0; i < MAX_USER; ++i)
            {
                if (matchStates[i] == null) continue;
                if (matchStates[i].userConnection.index == index)
                {
                    matchStates[i] = null;
                }
            }
            Send("/" + MessageType.MATCH + " " + MatchType.STOP);
            MatchManager.EndMatch(this);
        }
    }

    static class MatchManager
    {
        private static Queue<MyUser> matchUsers = new Queue<MyUser>();
        private static List<Match> matches = new List<Match>();

        public static void StartMatch(MyUser newUser)
        {
            lock (matchUsers)
            {
                if (!matchUsers.Contains(newUser))
                {
                    NetworkConnection.GetConnectionByUser(newUser).OnDisConnected += OnUserDisconnected;
                    matchUsers.Enqueue(newUser);
                }
                // 아니면 MMR 별로 Queue를 만들어셔 여기서 분류?
                if (matchUsers.Count >= 2)
                {
                    // MMR를 넣으려면 여기서 계산.
                    try
                    {
                        MyUser[] users = new MyUser[2];
                        for(int i = 0; i < users.Length; ++i)
                        {
                            users[i] = matchUsers.Dequeue();
                            NetworkConnection.GetConnectionByUser(users[i]).OnDisConnected -= OnUserDisconnected;
                        }

                        Match newMatch = new Match(users[0], users[1]);
                        lock (matches)
                        {
                            LogManager.WriteLog("Start Match : " + users[0].ID + "/" + NetworkConnection.GetConnectionByUser(newUser).Address + " " + users[1].ID + "/" + NetworkConnection.GetConnectionByUser(newUser).Address);
                            matches.Add(newMatch);
                        }
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        LogManager.WriteLog(e.ToString());
                    }
                }
            }
        }

        private static void OnUserDisconnected(NetworkConnection connection)
        {
            CancelMatch(connection.user);
        }

        public static void CancelMatch(MyUser cancelUser)
        {
            lock (matchUsers)
            {
                if (matchUsers.Contains(cancelUser))
                {
                    Queue<MyUser> newQueue = new Queue<MyUser>();
                    foreach(MyUser userData in matchUsers)
                    {
                        if(userData != cancelUser)
                        {
                            newQueue.Enqueue(userData);
                        }
                        else LogManager.WriteLog("Cancel Match : " + cancelUser.ID + "/" + NetworkConnection.GetConnectionByUser(cancelUser).Address);
                    }
                    matchUsers = newQueue;
                }
            }
        }

        public static void EndMatch(Match endMatch)
        {
            lock (matches)
            {
                if (matches.Contains(endMatch))
                {
                    
                    matches.Remove(endMatch);
                }
            }
        }

        public static Match GetMatchByIndex(int index)
        {
            if (matches == null) return null;
            for(int i = 0; i < matches.Count; ++i)
            {
                if (matches[i] == null) return null;
                if (matches[i].CheckIndex(index)) return matches[i];
            }
            return null;
        }

        public static void Execute(int senderIndex, string message)
        {
            string matchType = message;
            string messageData = string.Empty;
            if (message.Contains(' '))
            {
                matchType = message.Substring(0, message.IndexOf(' '));
                messageData = message.Substring(message.IndexOf(' ') + 1);
            }
            switch (Parse<MatchType>(matchType))
            {
                case MatchType.DEFAULT: break;
                case MatchType.START:
                    {
                        StartMatch(NetworkConnection.GetConnection(senderIndex).user);
                        break;
                    }
                case MatchType.CANCEL:
                    {
                        CancelMatch(NetworkConnection.GetConnection(senderIndex).user);
                        break;
                    }
                case MatchType.READY:
                case MatchType.HAND:
                case MatchType.BATTLE:
                    {
                        GetMatchByIndex(senderIndex).Execute(senderIndex, Parse<MatchType>(matchType), messageData);
                        break;
                    }
            }
        }
    }
}
