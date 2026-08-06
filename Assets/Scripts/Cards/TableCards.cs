using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

public class TableCards : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private List<CardVisuals> cardPrefabs = new();
    [SerializeField] private Transform deckPosition;
    
    [FormerlySerializedAs("peekCardPosition")]
    [FormerlySerializedAs("sunglassesCardPosition")]
    [Header("Peek Card")]    
    [SerializeField] private Transform peekCardTransform;
    
    [Header("Player Cards")]
    [SerializeField] private List<Transform> playerCardPositions = new();
    [Tooltip("Offsets the player cards to create the staircase layout.")]
    [SerializeField] private Vector3 playerCardsOffset = new(0.03f, 0.034f, -0.001f);
    
    [Header("Dealer Cads")]
    [SerializeField] private Transform dealerCardPosition;
    [Tooltip("Offsets the dealer cards to create a horizontal line.")]
    [SerializeField] private Vector3 dealerCardsOffset = new(0.13f, 0f, -0.001f);
    
    // General
    private BlackjackGame game;
    private Deck gameDeck;
    private List<GameObject> activeCardObjects = new();
    private Dictionary<(Card.Rank, Card.Suit), GameObject> cardPrefabLookup;
    public static readonly Vector3 CardScaleVector = Vector3.one * 0.05f;
    public static readonly float CardAnimationDuration = 0.25f;
    private int maxSplits = 3;

    // Peek
    [HideInInspector] public CardInstance PeekCardInstance = null;
    
    // Player
    private List<List<CardInstance>> playerHands = new();
    private int currentHandIndex = 0;
    
    // Dealer
    private List<CardInstance> dealerHand = new();
    [HideInInspector] public bool isDealerCardFlipped = false;
    
    #region Monobehaviour Methods

    private void Awake()
    {
        gameDeck = new Deck();
        InitializeCardLookup();
    }

    #endregion

    #region Getters & Setters

    public void SetBlackjackGame(BlackjackGame blackjackGame) => game = blackjackGame;
    public Deck GameDeck => gameDeck;
    public List<GameObject> ActiveCardObjects => activeCardObjects;
    public Transform CurrentHandPosition => playerCardPositions[currentHandIndex];
    public int CurrentHandIndex => currentHandIndex;
    public List<List<CardInstance>> PlayerHands => playerHands;
    public List<CardInstance> CurrentHand
    {
        get => playerHands[currentHandIndex];
        set => playerHands[currentHandIndex] = value;
    }

    public List<CardInstance> DealerHand
    {
        get => dealerHand;
        set => dealerHand = value;
    }
    
    #endregion
    
    #region Check Table Conditions

    public bool IsDealerHandFull => dealerHand.Count >= 7;
    public bool IsPlayerHandFull => CurrentHand.Count >= 7;
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

        isDealerCardFlipped = false;
    }

    public void ResetCards()
    {
        playerHands.Clear();
        playerHands.Add(new List<CardInstance>());
        currentHandIndex = 0;
    }
    
    public void ShuffleCards()
    {
        StartCoroutine(PlayShuffleSoundCoroutine());

        gameDeck.InitializeDeck();
        gameDeck.Shuffle();
    }

    #endregion

    #region Animate Cards

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

    public Coroutine FlipCard(CardDisplay cardDisplay, float duration) 
        => StartCoroutine(FlipCardCoroutine(cardDisplay, duration));
    
    private static IEnumerator FlipCardCoroutine(CardDisplay cardDisplay, float duration)
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

    #endregion
    
    #region Dealing Cards
    
    public Card DealCard() => gameDeck.DealCard();

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
    
    public IEnumerator DealCardToPlayerCoroutine() => DealCardCoroutine(true, false);
    public IEnumerator DealCardToDealerCoroutine(bool isHidden) => DealCardCoroutine(false, isHidden);

    private IEnumerator DealCardCoroutine(bool isForPlayer, bool isHidden)
    {
        List<CardInstance> hand = null; Transform handTransform = null; Vector3 cardsOffset = new();
        ProcessCardPlacement(isForPlayer, ref hand, ref handTransform, ref cardsOffset);
        
        int handValue = CalculateHandValue(hand, true);
        int idealValue = game.CalculateIdealNextValue(isForPlayer, handValue);
        
        var card = CrucifixItem.TryPrayForCard(gameDeck, idealValue);
        // var card = isForPlayer ? gameDeck.DealBestCard(5) : DealCard(); // test split
        
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
        
        UpdateHandVisuals(hand, isForPlayer);
        game.UpdateUI();
        
        if (isForPlayer)
        {
            KeepsakeManager.instance.OnDealPlayerCard(newCardInstance);
            UpdateSplitOutlines();
        }
    }
    
    private CardInstance DealCardInstance(Card newCardData, List<CardInstance> hand, bool isHidden)
    {
        var cardInstance = DealCardInstance(newCardData, isHidden);

        activeCardObjects.Add(cardInstance.CardObject);
        hand?.Insert(0, cardInstance);

        return cardInstance;
    }

    public CardInstance DealCardInstance(Card newCardData, bool isHidden) 
        => DealCardInstance(newCardData, isHidden, deckPosition);

    private CardInstance DealCardInstance(Card newCardData, bool isHidden, Transform spawnPosition)
    {
        if (newCardData == null || !cardPrefabLookup.TryGetValue(
                (newCardData.rank, newCardData.suit), out GameObject cardPrefabToUse)) return null;

        GameObject cardObject = Instantiate(cardPrefabToUse, spawnPosition);
        cardObject.transform.localScale = CardScaleVector;

        CardDisplay cardDisplay = cardObject.GetComponent<CardDisplay>();

        CardInstance newCardInstance = new CardInstance(newCardData, cardDisplay, isHidden);
        CardEffects.SetVisualEffects(newCardInstance, isHidden, false, false);
        
        if(newCardInstance.cardData.rank == Card.Rank.Joker)
            newCardInstance.cardData.jokerValue = Random.Range(-10, 11);

        return newCardInstance;
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

        int cardOrderIndex = currentHand.Count - 1;
        yield return PlaceCardAtIndex(cardOrderIndex, newCardInstance, currentParent, offset);
        
        game.UpdateUI();
    }

    public IEnumerator PlaceCardAtPlayerHandIndex(int cardOrderIndex, CardInstance newCardInstance)
        => PlaceCardAtIndex(cardOrderIndex, newCardInstance, playerCardPositions[currentHandIndex], playerCardsOffset);
    
    public IEnumerator PlaceCardAtIndex(int cardOrderIndex, CardInstance newCardInstance,
        Transform position, Vector3 offset)
    {
        if (newCardInstance == null) yield break;
        
        AudioManager.instance.Play("CardHit");

        Vector3 targetLocalPos = offset * cardOrderIndex;
        Quaternion targetRotation = Quaternion.identity;
    
        newCardInstance.displayComponent.transform.SetParent(position.parent);
    
        yield return StartCoroutine(CardAnimationCoroutine(
            newCardInstance.displayComponent.transform,
            position.TransformPoint(targetLocalPos),
            position.rotation * targetRotation,
            CardScaleVector,
            CardAnimationDuration
        ));
    
        newCardInstance.displayComponent.transform.SetParent(position);
        newCardInstance.displayComponent.transform.localPosition = targetLocalPos;
        newCardInstance.displayComponent.transform.localRotation = targetRotation;
        newCardInstance.displayComponent.transform.localScale = CardScaleVector;
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
    
    #endregion
    
    #region Modify Table Cards

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

    #endregion

    #region Update Visuals

    public void UpdateAllHandsVisuals()
    {
        playerHands.ForEach(hand => UpdateHandVisuals(hand, true));
        UpdateHandVisuals(dealerHand, false);
    }

    //The dealer hand is in a straight line, the player hand creates a staircase effect.
    private void UpdateHandVisuals(List<CardInstance> hand, bool isPlayerHand)
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

        if (!PeekCardInstance?.CardObject) return;
        
        Card topCard = gameDeck.PeekCard();
        if (topCard == null) return;
        
        PeekCardInstance.cardData = topCard;
        CardEffects.SetVisualEffects(PeekCardInstance, false, false, true);
    }

    #endregion

    #region Calculate Hand Values

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

    #endregion

    #region Create Jokers

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

    public void ProcessCardPlacement(bool isPlayer, ref List<CardInstance> hand, ref Transform position, ref Vector3 offset)
    {
        hand = isPlayer ? CurrentHand : dealerHand;
        position = isPlayer ? playerCardPositions[currentHandIndex] : dealerCardPosition;
        offset = isPlayer ? playerCardsOffset : dealerCardsOffset;
    }
    
    public void GoNextHand() => currentHandIndex++;

    public void ResetLastActiveHand()
    {
        currentHandIndex = Mathf.Min(currentHandIndex, playerHands.Count - 1);
    }


    private IEnumerator PlayShuffleSoundCoroutine()
    {
        while(Elevator.isElevatorActive)
        {
            yield return null;
        }

        AudioManager.instance.Play("Shuffle");
    }

    #endregion

    #region Reveal Cards

    public IEnumerator FlipDealerHiddenCard(float delayAfterFlip=1f)
    {
        CardInstance hiddenCard = dealerHand.FirstOrDefault(x => x.isHidden);

        if(hiddenCard != null)
        {
            yield return StartCoroutine(FlipCardCoroutine(hiddenCard.displayComponent, 0.4f));

            hiddenCard.isHidden = false;
            isDealerCardFlipped = true;

            game.UpdateUI(true);

            yield return GameUtils.WaitForSecondsScaled(delayAfterFlip);
        }
    }
    
    public bool RevealNextCard()
    {
        if (PeekCardInstance != null) return false;
        
        Card nextCard = gameDeck.PeekCard();
        PeekCardInstance = DealCardInstance(nextCard, false, peekCardTransform);
        
        if(PeekCardInstance == null) return false;
        
        StartCoroutine(CardAnimationCoroutine(
            PeekCardInstance.CardObject.transform,
            peekCardTransform.position,
            peekCardTransform.rotation,
            CardScaleVector,
            CardAnimationDuration
        ));
        
        activeCardObjects.Add(PeekCardInstance.CardObject);

        return true;
    }

    #endregion
    
    #region TODO Region

    // TODO: maybe move to cursor
    public void UpdateSplitOutlines(bool updateDealer=false)
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

        if (!updateDealer) return;
        
        foreach (var card in dealerHand)
        {
            ClickableCard clickable = card.displayComponent.GetComponentInChildren<ClickableCard>();
            clickable?.OnRemoveOutline(false);
        }
    }
    
    #endregion
}
