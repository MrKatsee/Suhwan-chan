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
        User user_1;
        User user_2;

        public Match(User newUser_1, User newUser_2)
        {
            user_1 = newUser_1;
            user_2 = newUser_2;
            // 여기서 메시지 보내자.

            ServerManager.Send(
                NetworkMessage.MessageType.MATCH,
                NetworkMessage.CastingType.MULTICAST,
                "",
                NetworkConnection.GetConnectionByUser(user_1).index,
                NetworkConnection.GetConnectionByUser(user_2).index);

        }
    }

    static class MatchManager
    {
        private static Queue<User> matchUsers = new Queue<User>();

        private static bool isAwake = false;
        private static Thread thread_MatchMaking; 

        public static void SyncEnqueue(User newUser)
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

        private static void MatchMaking()
        {
            while (isAwake)
            {
                lock (matchUsers)
                {
                    if(matchUsers.Count >= 2)
                    {
                        // MMR를 넣으려면 여기서 계산.
                        User user_1 = matchUsers.Dequeue();
                        User user_2 = matchUsers.Dequeue();
                        Match newMatch = new Match(user_1, user_2);
                    }
                }
                Thread.Sleep(50);
            }
        }
    }
}
