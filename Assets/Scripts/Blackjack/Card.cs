using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Card
{
    public enum Rank { None = 0, Ace, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Joker }
    public enum Suit { Clubs, Diamonds, Hearts, Spades, Tarot }

    public Rank rank;
    public Suit suit;
    public int jokerValue;

    //Calculates the numerical value of the card (Ace = 11, J/Q/K Faces = 10)
    public int GetValue(bool useJokers=false)
    {
        if (rank >= Rank.Ten && rank <= Rank.King) return 10;
        if (rank == Rank.Ace) return (EventManager.currentAceRule == AceValueRule.Always1) ? 1 : 11;
        if (!useJokers && rank == Rank.Joker) return 0;
        if (useJokers && rank == Rank.Joker) return jokerValue;
        
        return (int)rank;
    }

    public int GetValueNoJokers()
    {
        if (rank >= Rank.Ten && rank <= Rank.King) return 10;
        if (rank == Rank.Ace) return (EventManager.currentAceRule == AceValueRule.Always1) ? 1 : 11;

        return (int)rank;
    }

    public float GetCardValueForSplit()
    {
        float cardValue = GetValueNoJokers();

        if (EventManager.isDoubleLowActive && cardValue < 6 && rank != Rank.Joker) cardValue *= 2;

        if (EventManager.isHalfHighActive && cardValue > 5 && rank != Rank.Joker) cardValue = Mathf.CeilToInt(cardValue / 2f);

        return cardValue;
    }

    public override string ToString()
    {
        return $"{rank} of {suit}";
    }

    public static Rank GetRankForValue(int value)
    {
        return value switch
        {
            11 or 1 => Rank.Ace,
            10 => Rank.Ten,
            9 => Rank.Nine,
            8 => Rank.Eight,
            7 => Rank.Seven,
            6 => Rank.Six,
            5 => Rank.Five,
            4 => Rank.Four,
            3 => Rank.Three,
            2 => Rank.Two,
            _ => Rank.None
        };
    }
    
    public static List<Rank> GetRanksForValue(int value)
    {
        return value != 10 ? 
            new List<Rank> { GetRankForValue(value) } :
            new List<Rank> { Rank.Ten, Rank.Jack, Rank.Queen, Rank.King };
    }

    public static string GetRankString(Rank rank)
    {
        switch(rank)
        {
            case Rank.Ace: return "A";
            case Rank.Two: return "2";
            case Rank.Three: return "3";
            case Rank.Four: return "4";
            case Rank.Five: return "5";
            case Rank.Six: return "6";
            case Rank.Seven: return "7";
            case Rank.Eight: return "8";
            case Rank.Nine: return "9";
            case Rank.Ten: return "10";
            case Rank.Jack: return "J";
            case Rank.Queen: return "Q";
            case Rank.King: return "K";
            default: return " ";
        }
    }

    public static string GetSuitString(Suit suit)
    {
        switch(suit)
        {
            case Suit.Clubs: return "♣";
            case Suit.Diamonds: return "♦";
            case Suit.Hearts: return "♥";
            case Suit.Spades: return "♠";
            default: return " ";
        }
    }
}
