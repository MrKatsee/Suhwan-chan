using Newtonsoft.Json;

namespace SuHwan
{
    public enum CardState
    {
        Default,
        DECK,       // 덱에 있다.
        HAND,       // 손에 있다.
        FIELD,      // 필드에 있다.
        GRAVE       // 묘지에 있다.
    }

    public class Card
    {
        public static SuHwan.Card ParseData(string data)
        {
            return JsonConvert.DeserializeObject<SuHwan.Card>(data);
        }

        public SuHwan.CardState cardState { get; private set; }

        public string Name { get; protected set; }
        public string Description { get; protected set; }

        protected int hp;
        public int HP {                                     // 이러면 hp와 HP 두 개의 인자가 생기려나..?
            get
            {
                return hp;
            }
            protected set
            {
                hp = value;
                if (hp <= 0) OnDead();
            }
        }
        public int ATK { get; protected set; }                // Json으로 변환하려면 모두 public이어야 합니다.

        public Player master { get; private set; }

        public Card()
        {

        }

        [JsonConstructor]   // 이 키워드는 JsonConvert.DeserializeObject를 사용할 때 사용하는 생성자를 지정해 줍니다.
        public Card(string _name, string _description, int _hp, int _atk)
        {
            Name = _name;
            Description = _description;
            hp = _hp;
            ATK = _atk;
            cardState = SuHwan.CardState.DECK;      // 시작할 때 모든 카드는 덱에 있을 것이다.
            DualManager.Instance.OnChain += OnChain;
        }

        public void SetMaster(Player _player)
        {
            master = _player;
        }

        protected virtual void OnChain(Chain chain)
        {
            if(chain.chainPlayer == master)
            {
                switch (chain.chainType)
                {
                    case ChainType.START:
                        {
                            break;
                        }
                    case ChainType.END:
                        {
                            break;
                        }
                    case ChainType.DRAW:
                        {
                            Chain_Draw chain_Draw = (Chain_Draw)chain;
                            if (chain_Draw.drawCard == this)
                            {
                                cardState = CardState.HAND;
                            }
                            break;
                        }
                }
            }
            else
            {

            }
        }

        protected virtual void OnDead()
        {
            DualManager.Instance.AddChain(new Chain_Dead(master, this));
        }

        public string ToData()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
    }
}
