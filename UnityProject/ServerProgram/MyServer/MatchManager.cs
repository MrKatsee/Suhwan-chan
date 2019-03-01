using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyServer
{
    class Match
    {
        MyUser user_1;
        MyUser user_2;

        public Match(MyUser newUser_1, MyUser newUser_2)
        {
            user_1 = newUser_1;
            user_2 = newUser_2;
            // 여기서 메시지 보내자.

            ServerManager.Send(
                "",
                MyEnum.CastType.MULTICAST,
                NetworkConnection.GetConnectionByUser(user_1).index,
                NetworkConnection.GetConnectionByUser(user_2).index);

        }
    }

    static class MatchManager
    {
        private static Queue<MyUser> matchUsers = new Queue<MyUser>();

        private static bool isAwake = false;
        private static Thread thread_MatchMaking; 

        public static void SyncEnqueue(MyUser newUser)
        {
            lock (matchUsers)
            {
                // 아니면 MMR 별로 Queue를 만들어셔 여기서 분류?
                matchUsers.Enqueue(newUser);
            }
        }

        public static void Start()
        {
            isAwake = true;
            thread_MatchMaking = new Thread(MatchMaking);
            thread_MatchMaking.IsBackground = true;
            thread_MatchMaking.Start();
        }

        public static void Close()
        {
            isAwake = false;
        }

        public static void Execute(int senderIndex, string message)
        {

        }

        private static void MatchMaking()
        {
            while (isAwake)
            {
                lock (matchUsers)
                {
                    if(matchUsers.Count >= 2)
                    {
                        // MMR를 넣으려면 여기서 계산.
                        MyUser user_1 = matchUsers.Dequeue();
                        MyUser user_2 = matchUsers.Dequeue();
                        Match newMatch = new Match(user_1, user_2);
                    }
                }
                Thread.Sleep(50);
            }
        }
    }
}
