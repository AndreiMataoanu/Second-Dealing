using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CardEffects
{
    private static Dictionary<CardInstance, int> cutCards = new();
    private static List<Card.Suit> negativeSuits = new();
    private static HashSet<(Card.Rank, Card.Suit)> antiMatterCards = new();
    private static HashSet<CardInstance> alcoholCards = new();

    #region Card Collection

    public static int? GetCutReduction(CardInstance cardInstance)
    {
        if (cutCards.TryGetValue(cardInstance, out int reduction))
            return reduction;

        return null;
    }
    
    public static void AddCutCard(CardInstance cardInstance, int reduction)
    {
        if (!cutCards.TryAdd(cardInstance, reduction))
            cutCards[cardInstance] *= reduction;

        SetCutVisual(cardInstance.displayComponent, true);
    }
    public static void RemoveCutCard(CardInstance cardInstance) => cutCards.Remove(cardInstance);
    public static void ClearCutCards() => cutCards.Clear();
    
    public static void AddNegativeSuit(Card.Suit suit) => negativeSuits.Add(suit);
    
    private static void AddAlcoholCard(CardInstance cardInstance)
    {
        alcoholCards.Add(cardInstance);
        SetDoubledVisual(cardInstance.displayComponent, true);
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
    
    #endregion

    #region Set Visuals

    private static void SetCutVisual(CardDisplay cardDisplay, bool active) => cardDisplay.SetCutVisual(active);
    
    private static void SetDoubledVisual(CardDisplay cardDisplay, bool active) => cardDisplay.SetDoubledVisual(active);
    
    public static Coroutine SetDissolvedVisual(CardDisplay cardDisplay, float dissolveTime, Color color,float border) 
        => cardDisplay.StartCoroutine(cardDisplay.SetDissolvedVisual(dissolveTime, color, border));

    #endregion

    #region Check Effects

    public static bool IsCardCut(CardInstance cardInstance) => cutCards.ContainsKey(cardInstance);
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
    
    #endregion
    
    #region Add effects to cards
    
    // returns 2 possible values for ace, 1 possible value for the rest
    public static List<float> GetCardValuesFromEffects(CardInstance cardInstance, bool countJoker)
    {
        var card = cardInstance.cardData;
        var values = new List<float> { card.GetValue(countJoker) };
        if (card.rank == Card.Rank.Ace) values.Add(1); // ace can have value GetValue() - 11 or 1

        if (EventManager.isDoubleLowActive && values.First() < 6)
            values = values.ConvertAll(cardValue => cardValue * 2);

        if (EventManager.isHalfHighActive && values.First() > 5)
            values = values.ConvertAll(cardValue => (float)Mathf.CeilToInt(cardValue / 2f));

        if (IsCardNegative(card))
            values = values.ConvertAll(cardValue => -cardValue);

        var cut = GetCutReduction(cardInstance);
        if (cut != null)
            values = values.ConvertAll(cardValue => cardValue / (float)cut);

        if (IsCardDrunk(cardInstance))
            values = values.ConvertAll(cardValue => cardValue * 2);

        return values;
    }
    
    public static void SetVisualEffects(CardInstance cardInstance, bool isHidden, bool countAlcohol, bool countScissors)
    {
        bool isNegative = IsCardNegative(cardInstance.cardData);
        bool isDoubled = IsCardDoubled(cardInstance.cardData) || (countAlcohol && AlcoholItem.isAlcoholActive);
        bool isHalved = IsCardHalved(cardInstance.cardData) || (countScissors && IsCardCut(cardInstance));

        var display = cardInstance.displayComponent;
        display?.SetNegativeVisual(isNegative);
        display?.SetDoubledVisual(isDoubled);
        display?.SetCutVisual(isHalved);
        display?.SetHidden(isHidden);
    }
    
    #endregion

    public static void Reset()
    {
        cutCards.Clear();
        negativeSuits.Clear();
        antiMatterCards.Clear();
        alcoholCards.Clear();
    }
}
