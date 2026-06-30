using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Deck
{
    private List<Card> cards = new List<Card>();
    private List<Card.Rank> removedRanks = new List<Card.Rank>();
    private List<Card.Suit> removedSuits = new List<Card.Suit>();
    private List<Tuple<Card.Rank, Card.Suit>> removedCards = new ();

    private bool jokersInDeck = false;
    private bool isDissolved = false;

    public Deck()
    {
        InitializeDeck();
    }

    public void InitializeDeck()
    {
        cards.Clear();
        isDissolved = false;

        foreach(Card.Suit s in System.Enum.GetValues(typeof(Card.Suit)))
        {
            if(removedSuits.Contains(s)) continue;

            for(int r = (int)Card.Rank.Ace; r <= (int)Card.Rank.King; r++)
            {
                Card.Rank rank = (Card.Rank)r;

                if(removedRanks.Contains(rank)) continue;

                var card = new Tuple<Card.Rank, Card.Suit>(rank, s);
                if(removedCards.Contains(card)) continue;
                
                cards.Add(new Card { rank = rank, suit = s });
            }
        }

        if(jokersInDeck)
        {
            foreach(Card.Suit s in System.Enum.GetValues(typeof(Card.Suit)))
            {
                if(!removedSuits.Contains(s))
                {
                    cards.Add(new Card { rank = Card.Rank.Joker, suit = s });
                }
            }
        }
    }

    public void Shuffle()
    {
        int n = cards.Count;

        while(n > 1)
        {
            n--;

            int k = Random.Range(0, n + 1);

            Card value = cards[k];

            cards[k] = cards[n];
            cards[n] = value;
        }
    }

    public Card DealCard()
    {
        if(cards.Count == 0)
        {
            InitializeDeck();
            Shuffle();
        }

        Card dealtCard = cards[0];

        cards.RemoveAt(0);

        return dealtCard;
    }

    //Sunglasses ability: Peek at the next card without removing it from the deck
    public Card? PeekCard()
    {
        if(cards.Count == 0) return null;

        return cards[0];
    }

    public Card? PeekCardAt(int index)
    {
        if(index < 0 || index >= cards.Count) return null;

        return cards[index];
    }

    //Prayer Beads ability: Try to deal a specific card rank if available
    public Card? DealSpecificCard(Card.Rank rank)
    {
        Card? dealtCard = null;

        int cardIndex = -1;

        for(int i = 0; i < cards.Count; i++)
        {
            if(cards[i].rank == rank)
            {
                dealtCard = cards[i];
                cardIndex = i;

                break;
            }
        }

        if(cardIndex != -1)
        {
            cards.RemoveAt(cardIndex);

            return dealtCard;
        }

        return null;
    }

    public void AddRemovedValue(Card.Rank rank)
    {
        if(!removedRanks.Contains(rank)) removedRanks.Add(rank);

        InitializeDeck();
        Shuffle();
    }

    public void AddRemovedSuit(Card.Suit suit)
    {
        if(!removedSuits.Contains(suit)) removedSuits.Add(suit);

        InitializeDeck();
        Shuffle();
    }

    public void AddRemovedCard(Card.Rank rank, Card.Suit suit)
    {
        var card = new Tuple<Card.Rank, Card.Suit>(rank, suit);
        if (!removedCards.Contains(card)) removedCards.Add(card);
        isDissolved = true;
        
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].rank == card.Item1 && cards[i].suit == card.Item2)
            {
                cards.RemoveAt(i);
                return;
            }
        }
    }

    public void AddJokersToDeck()
    {
        if(!jokersInDeck)
        {
            jokersInDeck = true;

            InitializeDeck();
            Shuffle();
        }
    }
}
