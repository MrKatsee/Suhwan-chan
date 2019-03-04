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

        public delegate void EventHandler_Start();
        public delegate void EventHandler_Draw(SuHwan.Card drawCard);
        public delegate void EventHandler_Summon(SuHwan.Card summonCard);
        public delegate void EventHandler_Battle(SuHwan.Card attackCard, SuHwan.Card defendCard);
        public delegate void EventHandler_End();
        public event EventHandler_Start OnStart;
        public event EventHandler_Draw OnDraw;
        public event EventHandler_Summon OnSummon;
        public event EventHandler_Battle OnBattle;
        public event EventHandler_End OnEnd;

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
            while (true)        // 승패 조건?
            {
                OnStart?.Invoke();

                OnDraw?.Invoke(null);

                OnSummon?.Invoke(null);

                OnBattle?.Invoke(null, null);

                OnEnd?.Invoke();
                yield return null;
            }
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

