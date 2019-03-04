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
            DualManager.Instance.OnStart += OnStart;
            DualManager.Instance.OnDraw += OnDraw;
            DualManager.Instance.OnSummon += OnSummon;
            DualManager.Instance.OnBattle += OnBattle;
            DualManager.Instance.OnEnd += OnEnd;
        }

        protected virtual void OnStart()                       // 턴의 시작에 발동
        {

        }

        protected virtual void OnDraw(SuHwan.Card drawCard)                        // 드로우 시점에 발동 (상대 턴이던, 내 턴이던)
        {
            if(drawCard == this) cardState = SuHwan.CardState.HAND;
        }

        protected virtual void OnSummon(SuHwan.Card summonCard)                                   // 소환 시에 발동
        {
            if (summonCard == this) cardState = SuHwan.CardState.FIELD;
        }

        protected virtual void OnBattle(SuHwan.Card attackCard, SuHwan.Card defendCard)                  // 배틀 시에 발동
        {
            if(attackCard == this)
            {
                HP -= defendCard.ATK;                                           // 이 이벤트는 방어 카드에게도 똑같이 발동될 것입니다. 그러면 자기의 체력만 빼면 되겠지?
            }
            if(defendCard == this)
            {
                HP -= attackCard.ATK;                                           // 다만, 공격 시 공격력 증가라던가 같은 효과를 발동하려는 경우 순서가 중요해지기 때문에 이 부분은 좀 더 생각해봐야 할듯.
            }
        }

        protected virtual void OnEnd()                                                     // 턴 종료 시에 발동
        {

        }

        protected virtual void OnDead()                                                   // 뒤지면 발동
        {
            cardState = SuHwan.CardState.GRAVE;                                             // 묘지에서 효과를 발휘할 수도 있기 때문에 위의 이벤트들을 제거하지는 않는다.
        }
        
        public string ToData()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
    }
}
