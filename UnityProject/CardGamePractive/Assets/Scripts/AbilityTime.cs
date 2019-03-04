using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbilityTime : MonoBehaviour
{
    public AbilityCondition abilityCondition;

    public abstract bool CheckTime(Card card);
    
}
