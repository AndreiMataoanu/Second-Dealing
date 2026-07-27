using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Deck
{
    private List<Card> cards = new List<Card>();
    private List<Card.Rank> removedRanks = new List<Card.Rank>();
    private List<Card.Suit> removedSuits = new List<Card.Suit>();
    private List<Tuple<Card.Rank, Card.Suit>> removedCards = new ();
    private Tuple<Card, int> copies = new(null, 0);

    private bool jokersInDeck = false;

    public Deck()
    {
        InitializeDeck();
    }

    public void InitializeDeck()
    {
        cards.Clear();

        foreach(Card.Suit s in Enum.GetValues(typeof(Card.Suit)))
        {
            if(s == Card.Suit.Tarot && !Tarot.isTarotActive) continue;

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
            foreach(Card.Suit s in Enum.GetValues(typeof(Card.Suit)))
            {
                if(s == Card.Suit.Tarot) continue;

                if(!removedSuits.Contains(s))
                {
                    cards.Add(new Card { rank = Card.Rank.Joker, suit = s });
                }
            }
        }

        if (copies is not { Item2: > 0 } || copies.Item1 == null) return;

        var copy = copies.Item1;
        for(var i = 0; i < copies.Item2; i++)
            cards.Add(new Card{rank = copy.rank, suit = copy.suit});
    }

    public void Shuffle()
    {
        int n = cards.Count;

        while(n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);

            (cards[k], cards[n]) = (cards[n], cards[k]);
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
    public Card PeekCard()
    {
        if(cards.Count == 0) return null;

        return cards[0];
    }

    public Card PeekCardAt(int index)
    {
        if(index < 0 || index >= cards.Count) return null;

        return cards[index];
    }

    public Card DealSpecificCard(Card.Suit suit)
    {
        Card dealtCard = cards.Find(card => card.suit == suit);
        if (dealtCard != null) cards.Remove(dealtCard);

        dealtCard = DealCard();
        return dealtCard;
    }
    
    public Card DealSpecificCard(Card.Rank rank)
    {
        Card dealtCard = cards.Find(card => card.rank == rank);
        if (dealtCard != null) cards.Remove(dealtCard);

        return dealtCard;
    }

    public Card DealBestCard(int value)
    {
        while (value > 0)
        {
            var ranks = Card.GetRanksForValue(value);
            var dealtCard = cards.Find(card => ranks.Contains(card.rank));
            if (dealtCard != null)
            {
                cards.Remove(dealtCard);
                return dealtCard;
            }

            value--;
        }

        return DealCard();
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

    public int GetCopyCount(int minValue, int maxValue)
    {
        copies = new Tuple<Card?, int>(null, Random.Range(minValue, maxValue + 1));
        return copies.Item2;
    }
    
    public void AddCardCopies(Card card)
    {
        copies = new Tuple<Card?, int>(new Card { rank = card.rank, suit = card.suit }, copies.Item2);
    }
    
    public void AddCardCopies(Card card, int copyNumber)
    {
        copies = new Tuple<Card, int>(new Card { rank = card.rank, suit = card.suit }, copyNumber);
    }

    public void AddPrintedCard(Card card)
    {
        copies = new Tuple<Card?, int>(new Card { rank = card.rank, suit = card.suit }, copies.Item2 + 1);
        cards.Add(card);
    }

    public static Card.Rank GetRankForValue(int value)
    {
        if(value >= 11 || value == 1)
        {
            return Card.Rank.Ace;
        }

        switch(value)
        {
            case 10: return Card.Rank.Ten;
            case 9: return Card.Rank.Nine;
            case 8: return Card.Rank.Eight;
            case 7: return Card.Rank.Seven;
            case 6: return Card.Rank.Six;
            case 5: return Card.Rank.Five;
            case 4: return Card.Rank.Four;
            case 3: return Card.Rank.Three;
            case 2: return Card.Rank.Two;
            default: return Card.Rank.None;
        }
    }

    public Card? DealSecondDealingCard(Card.Rank rank, Card.Suit suit)
    {
        for(int i = 0; i < cards.Count; i++)
        {
            if(cards[i].rank == rank && cards[i].suit == suit)
            {
                Card dealtCard = cards[i];
                cards.RemoveAt(i);

                return dealtCard;
            }
        }

        return null;
    }
}