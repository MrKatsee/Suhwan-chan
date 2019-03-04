using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum CardState
{
    SUMMON
}

public enum CardType
{

}

public class Card
{
    public CardState cardState;
    public AbilityTime abilityTime;
    public AbilityCondition abilityCondition;
    public Ability ability;

    public int cardID;

    private float atk;
    private float hp;
    private float cost;

    public float Atk
    {
        get
        {
            return atk;
        }
        set
        {
            atk = value;
        }
    }
    public float Hp
    {
        get
        {
            return hp;
        }
        set
        {
            hp = value;
        }
    }

    public float Cost
    {
        get
        {
            return cost;
        }
        set
        {
            cost = value;
        }
    }

    private string cardName;
    private string description;

    public string CardName
    {
        get { return cardName; } set { cardName = value; }
    }
    public string Description
    {
        get { return description; }
        set { description = value; }
    }

    public Card(int _id, int _atk, int _hp, int _cost, string _cardName, string _description, string _abilityTime, string _abilityCondition, string _ability)
    {
        cardID = _id;
        atk = _atk;
        hp = _hp;
        cost = _cost;
        cardName = _cardName;
        description = _description;

        switch(_abilityTime)
        {
            case "Summon" :
                abilityTime = new AbilityTime_OnSummon();
                break;
            case "Null":
                abilityTime = null;
                break;
        }
        switch(_abilityCondition)
        {
            case "MyHand" :
                break;
        }
        switch(_ability)
        {
            case "DamageAll" :
                break;
        }
    }

    public void CheckCardAbility()
    {
        if (abilityTime != null)
        {
            if (abilityTime.CheckTime(this)) { }
                //if (abilityCondition != null)
                    //if (abilityTime.CheckCondition(~~~~))
                        //ability.PlayAbility(~~~);
        }

    }

    public override string ToString()
    {
        return String.Format("ID={0} ATK={1} HP={2} COST={3} NAME={4} DES={5}", cardID, atk, hp, cost, cardName, description);
    }

}
