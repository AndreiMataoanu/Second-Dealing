using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Deck
{
    private List<Card> cards = new();
    private List<Card.Rank> removedRanks = new();
    private List<Card.Suit> removedSuits = new();
    private List<Tuple<Card.Rank, Card.Suit>> removedCards = new();
    private Dictionary<Card, int> copies = new();

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

        foreach (var card in copies.Keys)
        {
            var copyNumber = copies[card];
            for(var i = 0; i < copyNumber; i++)
                cards.Add(new Card{rank = card.rank, suit = card.suit});
        }
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

    public Card DealSuit(Card.Suit suit)
    {
        var card = cards.Find(card => card.suit == suit);
        cards.Remove(card);
        return card ?? DealCard();
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

    public void AddCardCopy(Card card) => AddCardCopies(card, 1);
    
    public void AddCardCopies(Card card, int copyNumber)
    {
        if (copies.TryGetValue(card, out _))
            copies[card] += copyNumber;
        else
            copies.Add(card, copyNumber);
        
        cards.Add(card);
    }

    public Card DealSecondDealingCard(Card.Rank rank, Card.Suit suit)
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