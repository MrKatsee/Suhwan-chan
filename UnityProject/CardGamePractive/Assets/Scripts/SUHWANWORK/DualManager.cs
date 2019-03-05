using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuHwan
{
    public class DualManager : MonoBehaviour
    {
        private static DualManager instance = null;
        public static DualManager Instance
        {
            get
            {
                if(instance == null)
                {
                    instance.Init();
                }
                return instance;
            }
        }


        public delegate void EventHandler_Chain(SuHwan.Chain chain);
        public event EventHandler_Chain OnChain;
        private List<Chain> chains;

        public Player player_1;
        public Player player_2;

        public int[] p1Deck = { 1, 2, 3, 4, 5, 6, 7 };
        public int[] p2Deck = { 2, 2, 4, 5, 2, 7, 1 };

        public string p1Name = "호성쿤";
        public string p2Name = "수환쿤";

        public int p1HP = 30;
        public int p2HP = 30;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            instance = this;

            player_1 = new Player(p1Name, p1HP);
            player_2 = new Player(p2Name, p2HP);

            player_1.ParseDeckData(p1Deck);
            player_2.ParseDeckData(p2Deck);

            StartCoroutine(DualRoutine());
        }

        private IEnumerator DualRoutine()
        {
            bool playerTurn = true;
            while (true)        // 승패 조건?
            {
                Player player = playerTurn ? player_1 : player_2;
                AddChain(new Chain_Start(player));

                AddChain(new Chain_Draw(player, player.Draw()));
                yield return null;
                AddChain(new Chain_End(player));
            }
        }

        public void AddChain(Chain newChain)
        {
            chains.Add(newChain);
            if(chains.Count > 0)
            {
                OnChain?.Invoke(chains[chains.Count - 1]);
            }
            chains.Clear();
        }

        public void EndDual(Player deadPlayer)
        {
            StopAllCoroutines();
            Player winPlayer = deadPlayer == player_1 ? player_2 : player_1;
            Debug.Log(winPlayer.Name + "승리!");
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}

