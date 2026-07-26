public struct Card
{
    public enum Rank { None = 0, Ace, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Joker }
    public enum Suit { Clubs, Diamonds, Hearts, Spades, Tarot }

    public Rank rank;
    public Suit suit;

    //Calculates the numerical value of the card (Ace = 11, J/Q/K Faces = 10)
    public int GetValue()
    {
        if(rank >= Rank.Ten && rank <= Rank.King) return 10;
        if(rank == Rank.Ace) return 11;
        if(rank == Rank.Joker) return 0;

        return (int)rank;
    }

    public override string ToString()
    {
        return $"{rank} of {suit}";
    }

    public static Rank GetRankForValue(int value)
    {
        if(value >= 11 || value == 1)
        {
            return Rank.Ace;
        }

        switch(value)
        {
            case 10: return Rank.Ten;
            case 9: return Rank.Nine;
            case 8: return Rank.Eight;
            case 7: return Rank.Seven;
            case 6: return Rank.Six;
            case 5: return Rank.Five;
            case 4: return Rank.Four;
            case 3: return Rank.Three;
            case 2: return Rank.Two;
            default: return Rank.None;
        }
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
