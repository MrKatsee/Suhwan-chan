using System.Collections.Generic;

namespace SuHwan
{
    public class Player : SuHwan.Card          // 카드 - 플레이어 직접 공격을 구현!
    {
        public List<SuHwan.Card> deck = new List<SuHwan.Card>();
        public List<SuHwan.Card> hand = new List<Card>();
        public List<SuHwan.Card> grave = new List<Card>();

        public Player(string _name, int _hp)
        {
            Name = _name;
            hp = _hp;
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

        public Card Draw()
        {
            Card drawCard = deck[0];
            hand.Add(drawCard);
            deck.RemoveAt(0);
            return drawCard;
        }

        public List<Card> ParseDeckData(int[] indexValue)
        {
            List<SuHwan.Card> newDeck = new List<SuHwan.Card>();

            // 여기서 섞든지 말든지...

            foreach(SuHwan.Card card in newDeck)
            {
                card.SetMaster(this);
            }

            return newDeck;
        }

        protected override void OnDead()
        {
            DualManager.Instance.EndDual(this);
        }


    }
}
