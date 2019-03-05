using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuHwan
{
    public enum ChainType
    {
        START,
        END,
        DRAW,
        DEAD,
    }

    public class Chain
    {
        public ChainType chainType { get; protected set; }
        public Player chainPlayer;
    }

    public class Chain_Start : Chain
    {
        public Chain_Start(Player _chainPlayer)
        {
            chainType = ChainType.START;
            chainPlayer = _chainPlayer;
        }
    }

    public class Chain_End : Chain
    {
        public Chain_End(Player _chainPlayer)
        {
            chainType = ChainType.END;
            chainPlayer = _chainPlayer;
        }
    }

    public class Chain_Draw : Chain
    {
        public Card drawCard { get; private set; }
        public Chain_Draw(Player _chainPlayer, Card _drawCard)
        {
            chainType = ChainType.DRAW;
            chainPlayer = _chainPlayer;
            drawCard = _drawCard;
        }
    }

    public class Chain_Dead : Chain
    {
        public Card deadCard { get; private set; }
        public Chain_Dead(Player _chainPlayer, Card _deadCard)
        {
            chainType = ChainType.DEAD;
            chainPlayer = _chainPlayer;
            deadCard = _deadCard;
        }
    }
}
