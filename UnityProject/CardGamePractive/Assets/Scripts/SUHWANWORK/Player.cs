using System.Collections.Generic;

namespace SuHwan
{
    public class Player : SuHwan.Card          // 카드 - 플레이어 직접 공격을 구현!
    {
        public List<SuHwan.Card> deck = new List<SuHwan.Card>();

        public Player(string _name, int _hp)
        {
            Name = _name;
            hp = _hp;
            //DualManager.Instance.OnStart += OnStart;
            //DualManager.Instance.OnDraw += OnDraw;
            //DualManager.Instance.OnSummon += OnSummon;
            DualManager.Instance.OnBattle += OnBattle;
            //DualManager.Instance.OnEnd += OnEnd;
        }

        public List<Card> GetCardData(CardState cardState)  // 인자로 CardState.DECK, CardState.HAND 등을 넣으면 해당하는 카드들을 받아올 수 있따!
        {
            List<Card> cardData = new List<Card>();
            foreach(Card card in deck)
            {
                if(card.cardState == cardState)
                {
                    cardData.Add(card);
                }
            }
            return cardData;
        }

        public List<Card> ParseDeckData(int[] indexValue)
        {
            List<SuHwan.Card> newDeck = new List<SuHwan.Card>();

            // 여기서 섞든지 말든지...

            return newDeck;
        }

        protected override void OnBattle(SuHwan.Card attackCard, SuHwan.Card defendCard) 
        {
            if (attackCard == this)
            {
                HP -= defendCard.ATK;                                           // 이 이벤트는 방어 카드에게도 똑같이 발동될 것입니다. 그러면 자기의 체력만 빼면 되겠지?
            }
            if (defendCard == this)
            {
                HP -= attackCard.ATK;                                           // 다만, 공격 시 공격력 증가라던가 같은 효과를 발동하려는 경우 순서가 중요해지기 때문에 이 부분은 좀 더 생각해봐야 할듯.
            }
        }

        protected override void OnDead()
        {
            SuHwan.DualManager.Instance.EndDual(this);
        }


    }
}
