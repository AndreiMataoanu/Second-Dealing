using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CardEffects
{
    private static Dictionary<(Card.Rank,Card.Suit), int> cutCards = new();
    private static Dictionary<CardInstance, int> jokerCutCards = new();
    private static List<Card.Suit> negativeSuits = new();
    private static HashSet<(Card.Rank, Card.Suit)> antiMatterCards = new();
    private static HashSet<CardInstance> alcoholCards = new();
    private static HashSet<(Card.Rank, Card.Suit)> colorSwappedCards = new();
    public static Dictionary<CardInstance, int> hiddenAceCards = new();

    #region Card Collection

    public static int? GetCutReduction(CardInstance cardInstance)
    {
        var key = (cardInstance.cardData.rank,cardInstance.cardData.suit);
        if (cutCards.TryGetValue(key, out int reduction))
            return reduction;

        return null;
    }
    
    public static void AddCutCard(CardInstance cardInstance, int reduction)
    {
        if(cardInstance.cardData.rank == Card.Rank.Joker)
        {
            if (!jokerCutCards.TryAdd(cardInstance, reduction))
            jokerCutCards[cardInstance] *= reduction;
        }
        else
        {
            var key = (cardInstance.cardData.rank, cardInstance.cardData.suit);
            if (!cutCards.TryAdd(key, reduction))
            cutCards[key] *= reduction;    
        }
        
        SetVisualEffects(cardInstance, cardInstance.isHidden, true, true);
    }
    public static void RemoveCutCard(CardInstance cardInstance) => cutCards.Remove((cardInstance.cardData.rank,cardInstance.cardData.suit));
    public static void ClearCutCards() => cutCards.Clear();
    
    public static void AddNegativeSuit(Card.Suit suit) => negativeSuits.Add(suit);
    public static void ClearNegativeSuits() => negativeSuits.Clear();

    private static void AddAlcoholCard(CardInstance cardInstance)
    {
        alcoholCards.Add(cardInstance);
        SetVisualEffects(cardInstance, cardInstance.isHidden, true, true);
    }
    public static void AddAlcoholCardList(List<CardInstance> cards) => cards.ForEach(AddAlcoholCard);
    public static void RemoveAlcoholCard(CardInstance cardInstance) => alcoholCards.Remove(cardInstance);
    public static void ClearAlcoholCards() => alcoholCards.Clear();

    public static bool AddAntiMatterCard(CardInstance cardInstance)
    {
        return antiMatterCards.Add((cardInstance.cardData.rank, cardInstance.cardData.suit));
    }

    public static bool RemoveAntiMatterCard(CardInstance cardInstance)
    {
        return antiMatterCards.Remove((cardInstance.cardData.rank, cardInstance.cardData.suit));
    }

    public static void ClearAntiMatterCards() => antiMatterCards.Clear();

    public static void AddHiddenAce(CardInstance cardInstance, int bonus)
    {
        if(!hiddenAceCards.TryAdd(cardInstance, bonus))
            hiddenAceCards[cardInstance] += bonus;
    }

    public static void ClearHiddenAces() => hiddenAceCards.Clear();

    public static void ToggleColorSwap(Card card)
    {
        var key = (card.rank, card.suit);

        if(!colorSwappedCards.Add(key))
        {
            colorSwappedCards.Remove(key);
        }
    }

    public static void ClearColorSwappedCards() => colorSwappedCards.Clear();

    #endregion

    #region Set Visuals

    private static void SetCutVisual(CardDisplay cardDisplay, bool active) => cardDisplay.SetCutOnceVisual(active);
    
    private static void SetDoubledVisual(CardDisplay cardDisplay, bool active) => cardDisplay.SetDoubledOnceVisual(active);
    
    public static Coroutine SetDissolvedVisual(CardDisplay cardDisplay, float dissolveTime, Color color,float border) 
        => cardDisplay.StartCoroutine(cardDisplay.SetDissolvedVisual(dissolveTime, color, border));

    #endregion

    #region Check Effects

    public static bool IsCardCut(CardInstance cardInstance) => cutCards.ContainsKey((cardInstance.cardData.rank,cardInstance.cardData.suit));
    public static bool IsSuitNegative(Card.Suit suit) => negativeSuits.Contains(suit);
    public static bool IsCardAntiMatter(Card card) => antiMatterCards.Contains((card.rank, card.suit));
    
    public static bool IsCardNegative(Card card)
    {
        bool isSuitNegative = IsSuitNegative(card.suit);
        bool isAntiMatter = IsCardAntiMatter(card);

        return isSuitNegative ^ isAntiMatter;
    }

    public static bool IsCardDrunk(CardInstance cardInstance) => alcoholCards.Contains(cardInstance);

    public static bool IsCardDoubled(Card card)
    {
        if(!EventManager.isDoubleLowActive || card.rank == Card.Rank.Joker) return false;

        return card.GetValueNoJokers() < 6f;
    }
    
    public static bool IsCardHalved(Card card)
    {
        if(!EventManager.isHalfHighActive || card.rank == Card.Rank.Joker) return false;

        return card.GetValueNoJokers() > 5f;
    }

    public static bool IsColorSwapped(Card card) => colorSwappedCards.Contains((card.rank, card.suit));

    #endregion

    #region Add effects to cards

    // returns 2 possible values for ace, 1 possible value for the rest
    public static List<float> GetCardValuesFromEffects(CardInstance cardInstance, bool countJoker)
    {
        var card = cardInstance.cardData;
        var values = new List<float> { card.GetValue(countJoker) };
        var original = values.First();
        if (card.rank == Card.Rank.Ace) values.Add(1); // ace can have value GetValue() - 11 or 1
        
        if (EventManager.isDoubleLowActive && original < 6)
            values = values.ConvertAll(cardValue => cardValue * 2);

        if (EventManager.isHalfHighActive && original > 5)
            values = values.ConvertAll(cardValue => (float)Mathf.CeilToInt(cardValue / 2f));

        if (IsCardNegative(card))
            values = values.ConvertAll(cardValue => -cardValue);

        var cut = GetCutReduction(cardInstance);
        if (cut != null)
            values = values.ConvertAll(cardValue => cardValue / (float)cut);

        if (IsCardDrunk(cardInstance))
            values = values.ConvertAll(cardValue => cardValue * 2);

        if (hiddenAceCards.TryGetValue(cardInstance, out int bonus))
            values = values.ConvertAll(cardValue => cardValue + bonus);

        return values;
    }
    
    public static void SetVisualEffects(CardInstance cardInstance, bool isHidden, bool countAlcohol, bool countScissors)
    {
        bool isNegative = IsCardNegative(cardInstance.cardData);
        bool isColorSwapped = IsColorSwapped(cardInstance.cardData);
        bool isDoubledOnce = IsCardDoubled(cardInstance.cardData) || (countAlcohol && AlcoholItem.isAlcoholActive);
        bool isDoubledTwice = IsCardDoubled(cardInstance.cardData) && (countAlcohol && AlcoholItem.isAlcoholActive);
        bool isCutOnce = IsCardHalved(cardInstance.cardData) || (countScissors && IsCardCut(cardInstance));
        bool isCutTwice = IsCardHalved(cardInstance.cardData) && (countScissors && IsCardCut(cardInstance));
        var display = cardInstance.displayComponent;

        display?.SetHidden(isHidden);
        display?.SetNegativeVisual(isNegative);
        display?.SetColorSwapVisual(isColorSwapped);
        display?.SetDoubledOnceVisual(isDoubledOnce);
        display?.SetDoubledTwiceVisual(isDoubledTwice);
        display?.SetCutOnceVisual(isCutOnce);
        display?.SetCutTwiceVisual(isCutTwice);
    }
    
    #endregion

    public static void Reset()
    {
        jokerCutCards.Clear();
        alcoholCards.Clear();
        hiddenAceCards.Clear();
    }
}
