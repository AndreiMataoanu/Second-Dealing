public struct Card
{
    public enum Rank { None = 0, Ace, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King }
    public enum Suit { Clubs, Diamonds, Hearts, Spades }

    public Rank rank;
    public Suit suit;

    //Calculates the numerical value of the card (Ace = 11, J/Q/K Faces = 10)
    public int GetValue()
    {
        if(rank >= Rank.Ten && rank <= Rank.King) return 10;
        if(rank == Rank.Ace) return 11;

        return (int)rank;
    }

    public override string ToString()
    {
        return $"{rank} of {suit}";
    }
}
