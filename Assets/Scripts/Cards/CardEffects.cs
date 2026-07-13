using System.Collections.Generic;

public static class CardEffects
{
    public static Dictionary<CardInstance, int> cutCards = new();
    private static List<Card.Suit> negativeSuits = new();
    private static HashSet<(Card.Rank, Card.Suit)> antiMatterCards = new();
    private static HashSet<CardInstance> alcoholCards = new();

    #region Card Collection
    
    public static void AddCutCard(CardInstance cardInstance, int reduction)
    {
        if (cutCards.TryAdd(cardInstance, reduction)) return;
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

    #endregion

    #region Set Visuals

    private static void SetCutVisual(CardDisplay cardDisplay, bool active) => cardDisplay.SetCutVisual(active);
    private static void SetDoubledVisual(CardDisplay cardDisplay, bool active) => cardDisplay.SetDoubledVisual(active);

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

    #endregion





}
