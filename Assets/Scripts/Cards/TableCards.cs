using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

public class TableCards : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private List<CardVisuals> cardPrefabs = new();
    [SerializeField] private Transform deckPosition;
    
    [Header("Peek Card")]    
    [SerializeField] private Transform sunglassesCardPosition;
    
    [Header("Player Cards")]
    [SerializeField] private List<Transform> playerCardPositions = new();
    [Tooltip("Offsets the player cards to create the staircase layout.")]
    [SerializeField] private Vector3 playerCardsOffset = new(0.03f, 0.034f, -0.001f);
    
    [Header("Dealer Cads")]
    [SerializeField] private Transform dealerCardPosition;
    [Tooltip("Offsets the dealer cards to create a horizontal line.")]
    [SerializeField] private Vector3 dealerCardsOffset = new(0.13f, 0f, -0.001f);
    
    // General
    private Deck gameDeck;
    private List<GameObject> activeCardObjects = new();
    private Dictionary<(Card.Rank, Card.Suit), GameObject> cardPrefabLookup;
    public static readonly Vector3 CardScaleVector = Vector3.one * 0.05f;
    public static readonly float CardAnimationDuration = 0.25f;
    private int maxSplits = 3;

    // Peek
    public CardInstance PeekCardInstance = null;
    
    // Player
    private List<List<CardInstance>> playerHands = new();
    private int currentHandIndex = 0;
    
    // Dealer
    private List<CardInstance> dealerHand = new();
    
    // TODO: remove
    private BlackjackGame game; //maybe

    #region Getters

    public Deck GameDeck => gameDeck;
    public List<List<CardInstance>> PlayerHands => playerHands;
    public int CurrentHandIndex => currentHandIndex;
    public List<CardInstance> CurrentHand => playerHands[currentHandIndex];
    public Transform CurrentHandPosition => playerCardPositions[currentHandIndex];

    public List<CardInstance> DealerHand => dealerHand;
    
    #endregion
    
    #region Check Table Conditions

    public bool IsDealerHandFull => dealerHand.Count >= 7;
    public bool IsPlayerTurn => currentHandIndex < playerHands.Count;
    public int PlayerHandsCount => playerHands.Count;
    public bool AreSplitHandsFull => playerHands.Count >= maxSplits + 1;

    public bool IsPlayerHandEqual()
    {
        if(CurrentHand.Count != 2) return false;

        var val1 = CurrentHand[0].cardData.GetCardValueForSplit();
        var val2 = CurrentHand[1].cardData.GetCardValueForSplit();

        return Mathf.Approximately(val1, val2);
    }
    
    #endregion
    
    #region Monobehaviour Methods

    private void Awake()
    {
        gameDeck = new Deck();
        InitializeCardLookup();
    }

    #endregion

    #region Table Setup

    private void InitializeCardLookup()
    {
        cardPrefabLookup = new Dictionary<(Card.Rank, Card.Suit), GameObject>();

        foreach(var cardVisual in cardPrefabs)
        {
            if(cardVisual.rank != Card.Rank.None)
            {
                cardPrefabLookup.Add((cardVisual.rank, cardVisual.suit), cardVisual.cardPrefab);
            }
        }
    }
    
    public void ClearTable()
    {
        CardEffects.ClearAlcoholCards();
        CardEffects.ClearCutCards();
        
        DestroyActiveCards();
        DestroyPeekCard();
        
        playerHands.ForEach(hand => hand.Clear());
        playerHands.Clear();
        dealerHand.Clear();
    }

    public void ResetCards()
    {
        playerHands.Clear();
        playerHands.Add(new List<CardInstance>());
        currentHandIndex = 0;
    }

    #endregion

    #region Helper Methods

    private void DestroyPeekCard()
    {
        if (PeekCardInstance == null) return;
        
        Destroy(PeekCardInstance.CardObject);
        PeekCardInstance = null;
    }

    private void DestroyActiveCards()
    {
        foreach(GameObject cardObject in activeCardObjects)
            if(cardObject) Destroy(cardObject);
        
        activeCardObjects.Clear();
    }

    #endregion
    
    #region Dealing Cards
    
    public void ShuffleCards()
    {
        AudioManager.instance.Play("Shuffle");

        gameDeck.InitializeDeck();
        gameDeck.Shuffle();
    }

    public IEnumerator PlaceCardInPlayerHandCoroutine(CardInstance cardInstance)
    {
        List<CardInstance> currentHand = playerHands[currentHandIndex];
        Transform currentParent = playerCardPositions[currentHandIndex];
        CardInstance newCardInstance = DealCardInstance(cardInstance.cardData, currentHand, false);

        yield return PlaceCardInHand(newCardInstance, currentHand, currentParent, playerCardsOffset);
        UpdateHandVisuals(currentHand, true);
        UpdateSplitOutlines();
    }
    
    private IEnumerator PlaceCardInHand(CardInstance newCardInstance, List<CardInstance> currentHand,
        Transform currentParent, Vector3 offset)
    {
        if (newCardInstance == null) yield break;
        
        AudioManager.instance.Play("CardHit");

        int cardOrderIndex = currentHand.Count - 1;
        Vector3 targetLocalPos = offset * cardOrderIndex;
        Quaternion targetRotation = Quaternion.identity;
    
        newCardInstance.displayComponent.transform.SetParent(currentParent.parent);
    
        yield return StartCoroutine(CardAnimationCoroutine(
            newCardInstance.displayComponent.transform,
            currentParent.TransformPoint(targetLocalPos),
            currentParent.rotation * targetRotation,
            CardScaleVector,
            CardAnimationDuration
        ));
    
        newCardInstance.displayComponent.transform.SetParent(currentParent);
        newCardInstance.displayComponent.transform.localPosition = targetLocalPos;
        newCardInstance.displayComponent.transform.localRotation = targetRotation;
        newCardInstance.displayComponent.transform.localScale = CardScaleVector;
    
        game.UpdateUI();
    }
    
    public void DrawCardAnimation(Transform cardTransform, Vector3 targetPosition, Quaternion targetRotation,
        Vector3 targetScale, float duration)
    {
        StartCoroutine(CardAnimationCoroutine(cardTransform, targetPosition, targetRotation, targetScale, duration));
    }
    
    public IEnumerator CardAnimationCoroutine(Transform cardTransform, 
        Vector3 targetPosition, Quaternion targetRotation, Vector3 targetScale, float duration)
    {
        Vector3 startPosition = cardTransform.position;
        Quaternion startRotation = cardTransform.rotation;
        Vector3 startScale = cardTransform.localScale;

        float time = 0;

        while(time < duration)
        {
            if(!cardTransform) yield break;

            time += Time.deltaTime;

            float t = time / duration;

            t = t * t * (3f - 2f * t);

            cardTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
            cardTransform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            cardTransform.localScale = Vector3.Lerp(startScale, targetScale, t);

            yield return null;
        }

        cardTransform.position = targetPosition;
        cardTransform.rotation = targetRotation;
        cardTransform.localScale = targetScale;
    }

    public static IEnumerator FlipCardCoroutine(CardDisplay cardDisplay, float duration)
    {
        Transform cardTransform = cardDisplay.transform;

        Quaternion startRotation = cardTransform.localRotation;
        Quaternion ninetyDegrees = Quaternion.Euler(0, 90f, startRotation.eulerAngles.z);
        Quaternion flippedStartRotation = Quaternion.Euler(0, -90f, startRotation.eulerAngles.z);

        float halfDuration = duration / 2.0f;

        if(!CigarettesItem.isCigaretteActive) AudioManager.instance.Play("Flip");

        yield return RotateCardCoroutine(halfDuration, cardTransform, startRotation, ninetyDegrees);

        cardDisplay.SetHidden(false);
        cardTransform.localRotation = flippedStartRotation;
        
        yield return RotateCardCoroutine(halfDuration, cardTransform, flippedStartRotation, startRotation);
    }

    private static IEnumerator RotateCardCoroutine(float duration, Transform card,
        Quaternion startRotation, Quaternion targetRotation)
    {
        float elapsedTime = 0;
        while(elapsedTime < duration)
        {
            card.localRotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        card.localRotation = targetRotation;
    }

    // TODO: move to cursor
    public void UpdateSplitOutlines()
    {
        if(playerHands.Count <= 1) return;

        for(int i = 0; i < playerHands.Count; i++)
        {
            foreach(CardInstance card in playerHands[i])
            {
                ClickableCard clickable = card.displayComponent.GetComponentInChildren<ClickableCard>();

                if(clickable)
                {
                    if(i == currentHandIndex) clickable.ApplyOutline();
                    else clickable.OnRemoveOutline(false);
                }
            }
        }
    }

    public IEnumerator DealRoundCoroutine(bool rigPlayerHand=false)
    {
        if(rigPlayerHand) RigPlayerHand();
        ResetCards();

        yield return StartCoroutine(DealCardToPlayerCoroutine());
        yield return StartCoroutine(DealCardToDealerCoroutine(true));
        yield return StartCoroutine(DealCardToPlayerCoroutine());
        yield return StartCoroutine(DealCardToDealerCoroutine(false));

        game.UpdateUI();
    }
    
    private CardInstance DealCardInstance(Card newCardData, List<CardInstance> hand, bool isHidden)
    {
        var cardInstance = DealCardInstance(newCardData, isHidden);

        activeCardObjects.Add(cardInstance.CardObject);
        hand?.Insert(0, cardInstance);

        return cardInstance;
    }
    
    public CardInstance DealCardInstance(Card newCardData, bool isHidden)
    {
        if(!cardPrefabLookup.TryGetValue((newCardData.rank, newCardData.suit), out GameObject cardPrefabToUse)) return null;

        GameObject cardObject = Instantiate(cardPrefabToUse, deckPosition);
        cardObject.transform.localScale = CardScaleVector;

        CardDisplay cardDisplay = cardObject.GetComponent<CardDisplay>();

        CardInstance newCardInstance = new CardInstance(newCardData, cardDisplay, isHidden);
        CardEffects.SetVisualEffects(newCardInstance, isHidden, false, false);
        
        if(newCardInstance.cardData.rank == Card.Rank.Joker)
            newCardInstance.cardData.jokerValue = Random.Range(-10, 11);

        return newCardInstance;
    }
    
    // temp
    public Card DealCard()
    {
        return gameDeck.DealCard();
    }

    public IEnumerator DealCardToPlayerCoroutine() => DealCardCoroutine(true, false);
    public IEnumerator DealCardToDealerCoroutine(bool isHidden) => DealCardCoroutine(false, isHidden);

    private IEnumerator DealCardCoroutine(bool isForPlayer, bool isHidden)
    {
        List<CardInstance> hand = isForPlayer ? CurrentHand : dealerHand;
        Transform handTransform = isForPlayer ? CurrentHandPosition : dealerCardPosition;
        Vector3 cardsOffset = isForPlayer ? playerCardsOffset : dealerCardsOffset;

        int handValue = CalculateHandValue(hand, true);
        int idealValue = CalculateIdealNextValue(isForPlayer, handValue);
        
        var card = CrucifixItem.TryPrayForCard(gameDeck, idealValue);

        CardInstance newCardInstance;
        if (PeekCardInstance == null)
            newCardInstance = DealCardInstance(card, hand, isHidden);
        else
        {
            newCardInstance = PeekCardInstance;
            hand.Insert(0, newCardInstance);
            PeekCardInstance = null;
        }
        
        yield return PlaceCardInHand(newCardInstance, hand, handTransform, cardsOffset);
        
        UpdateHandVisuals(dealerHand, isForPlayer);
        game.UpdateUI();
        
        if (isForPlayer)
        {
            KeepsakeManager.instance.OnDealPlayerCard(newCardInstance);
            UpdateSplitOutlines();
        }
    }
    
    // TODO: maybe move to blackjack game
    private int CalculateIdealNextValue(bool isForPlayer, int currentValue)
    {
        if (isForPlayer) return BlackjackGame.blackjackGoal - currentValue;
        
        var idealValue = 10;
        if (currentValue >= BlackjackGame.blackjackGoal - 15 && currentValue < BlackjackGame.blackjackGoal - 9)
            idealValue = BlackjackGame.blackjackGoal - 5 - currentValue;

        return idealValue;
    }

    public int CalculateDealerHandValue(bool countJoker) => CalculateHandValue(dealerHand, countJoker);

    //Calculates the total value of a hand. Aces are 1 or 11.
    public int CalculateHandValue(List<CardInstance> hand, bool countJoker)
    {
        List<float> aceReductions = new List<float>();
        float handValue = 0f;

        foreach (var cardInstance in hand)
        {
            var possibleValues = CardEffects.GetCardValuesFromEffects(cardInstance, countJoker);
            
            handValue += possibleValues[0];
            
            if (possibleValues.Count == 2) 
                aceReductions.Add(possibleValues[0] - possibleValues[1]);
        }

        if(EventManager.currentAceRule == AceValueRule.Flexible)
        {
            aceReductions.Sort((a, b) => b.CompareTo(a));

            foreach(float reduction in aceReductions)
                if(handValue > BlackjackGame.blackjackGoal || handValue < -BlackjackGame.blackjackGoal)
                    handValue += (handValue > 0) ? -reduction : reduction;
        }

        return Mathf.RoundToInt(handValue);
    }

    public IEnumerator SplitCardsCoroutine()
    {
        List<CardInstance> activeHand = playerHands[currentHandIndex];
        CardInstance cardToMove = activeHand[0];
        game.CursorDetection.SetCardActive(cardToMove, false);

        activeHand.RemoveAt(0);

        List<CardInstance> newHand = new List<CardInstance> { cardToMove };

        playerHands.Insert(currentHandIndex + 1, newHand);

        for(int i = currentHandIndex + 2; i < playerHands.Count; i++)
        {
            Transform shiftTarget = playerCardPositions[i];

            foreach(var card in playerHands[i])
            {
                card.displayComponent.transform.SetParent(shiftTarget);
            }

            UpdateHandVisuals(playerHands[i], true);
        }

        AudioManager.instance.Play("CardHit");
        Transform targetPosition = playerCardPositions[currentHandIndex + 1];

        yield return StartCoroutine(CardAnimationCoroutine(
            cardToMove.displayComponent.transform,
            targetPosition.position,
            targetPosition.rotation,
            CardScaleVector,
            CardAnimationDuration
        ));

        cardToMove.displayComponent.transform.SetParent(targetPosition);
        cardToMove.displayComponent.transform.localPosition = Vector3.zero;

        UpdateHandVisuals(activeHand, true);
        UpdateHandVisuals(newHand, true);

        yield return GameUtils.WaitForSecondsScaled(0.5f);

        UpdateSplitOutlines();
    }

    public IEnumerator FlipDealerHiddenCard()
    {
        CardInstance hiddenCard = dealerHand.FirstOrDefault(x => x.isHidden);

        if(hiddenCard != null)
        {
            yield return StartCoroutine(FlipCardCoroutine(hiddenCard.displayComponent, 0.4f));

            hiddenCard.isHidden = false;
            
            yield return GameUtils.WaitForSecondsScaled(1f);
        }
    }
    
    //The dealer hand is in a straight line, the player hand creates a staircase effect.
    public void UpdateHandVisuals(List<CardInstance> hand, bool isPlayerHand)
    {
        int cardCount = hand.Count;

        if(cardCount == 0) return;

        for(int i = 0; i < cardCount; i++)
        {
            CardInstance cardInstance = hand[i];
            int cardOrderIndex = cardCount - 1 - i;

            var offset = isPlayerHand ? playerCardsOffset : dealerCardsOffset;
            var targetLocalPos = cardOrderIndex * offset;

            cardInstance.displayComponent.transform.localPosition = targetLocalPos;
            cardInstance.displayComponent.transform.localRotation = Quaternion.identity;
        }
    }

    public void UpdateCardVFX()
    {
        foreach (var hand in playerHands)
            foreach (CardInstance card in hand)
                CardEffects.SetVisualEffects(card, false, true, true);

        foreach(CardInstance card in dealerHand)
            CardEffects.SetVisualEffects(card, false, false, true);

        if (!PeekCardInstance.CardObject) return;
        
        Card topCard = gameDeck.PeekCard();
        if (topCard == null) return;
        
        PeekCardInstance.cardData = topCard;
        CardEffects.SetVisualEffects(PeekCardInstance, false, false, true);
    }

    public (List<CardInstance>, List<Coroutine>) CreatePlayerJokers()
    {
        var allPlayerJokers = new List<CardInstance>();
        var coroutines = new List<Coroutine>();
        for (int i = 0; i < playerHands.Count; i++)
        {
            var (jokers, jokerCoroutines) = CreateJokers(playerHands[i], playerCardPositions[i]);
            allPlayerJokers.AddRange(jokers);
            coroutines.AddRange(jokerCoroutines);
        }

        return (allPlayerJokers, coroutines);
    }

    public (List<CardInstance>, List<Coroutine>) CreateDealerJokers() => CreateJokers(dealerHand, dealerCardPosition);

    public (List<CardInstance>, List<Coroutine>) CreateJokers(List<CardInstance> cards, Transform position)
    {
        var jokers = new List<CardInstance>();
        List<Coroutine> jokerCoroutines = new List<Coroutine>();

        foreach (var card in cards)
        {
            if (card.cardData.rank == Card.Rank.Joker)
            {
                jokerCoroutines.Add(CreateRealJokerCard(card, position));
                jokers.Add(card);
            }
        }

        return (jokers, jokerCoroutines);
    }
    
    private Coroutine CreateRealJokerCard(CardInstance card, Transform parent)
    {
        int realValue = card.cardData.jokerValue;
        if(realValue > 11 || realValue < -11)
            realValue /= 2;
        
        if(card.cardData.jokerValue != 0)
        {
            cardPrefabLookup.TryGetValue((Card.GetRankForValue(Mathf.Abs(realValue)), card.cardData.suit), out GameObject realCard);
            GameObject realCardObject = Instantiate(realCard, card.CardObject.transform.position, card.CardObject.transform.rotation, parent);
            var display = realCardObject.GetComponent<CardDisplay>();
            
            if(card.cardData.jokerValue < 0)
                display?.SetNegativeVisual(true);

            if(card.cardData.jokerValue > 11 || card.cardData.jokerValue < -11)
                display?.SetDoubledVisual(true);

            activeCardObjects.Add(realCardObject);      
        }
        
        return CardEffects.SetDissolvedVisual(card.displayComponent, 2.0f, Color.aliceBlue,1.2f);                
    }
    
    private void RigPlayerHand()
    {
        int maxAttempts = 50;
        for (int attempts = 0; attempts < maxAttempts; attempts++)
        {
            Card firstCard = gameDeck.PeekCardAt(0);
            Card secondCard = gameDeck.PeekCardAt(2);

            if(firstCard == null || secondCard == null) break;

            int simulatedValue = SimulateInitialHandValue(firstCard, secondCard);

            if (simulatedValue >= 12 && simulatedValue <= 16)
                gameDeck.Shuffle();
            else
                break;
        }
    }

    private int SimulateInitialHandValue(Card c1, Card c2)
    {
        List<CardInstance> tempHand = new List<CardInstance>();
        CardInstance tempCard1 = new CardInstance(c1, null, false);
        CardInstance tempCard2 = new CardInstance(c2, null, false);

        tempHand.Add(tempCard1);
        tempHand.Add(tempCard2);

        return CalculateHandValue(tempHand, true);
    }

    public void DestroyCard(CardInstance cardInstance)
    {
        var cardObject = cardInstance.displayComponent.gameObject;
        CardEffects.RemoveCutCard(cardInstance);
        CardEffects.RemoveAlcoholCard(cardInstance);
        activeCardObjects.Remove(cardObject);
        GameDeck.AddRemovedCard(cardInstance.cardData.rank, cardInstance.cardData.suit); // TODO: move to card effects

        if (dealerHand.Remove(cardInstance))
        {
            KeepsakeUnlockProgression.instance.AddStat(ChallengeType.AlterDealerHand);
            UpdateHandVisuals(dealerHand, false);
        }
        
        playerHands.ForEach(hand =>
        {
            hand.Remove(cardInstance);
            UpdateHandVisuals(hand, true);
        });

        if (cardInstance == PeekCardInstance)
            PeekCardInstance = null;
        
        Destroy(cardObject);
        game.UpdateUI();
    }

    public void GoNextHand() => currentHandIndex++;
    
    #endregion
    
}
