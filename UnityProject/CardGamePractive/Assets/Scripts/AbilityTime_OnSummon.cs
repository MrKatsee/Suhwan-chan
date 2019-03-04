using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityTime_OnSummon : AbilityTime
{
    public override bool CheckTime(Card card)
    {
        return card.cardState == CardState.SUMMON;
    }
}
