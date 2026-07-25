using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TableCards : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private List<CardVisuals> cardPrefabs = new();
    [SerializeField] private Transform deckPosition;
    
    [Header("Peek Card")]    
    [SerializeField] private Transform sunglassesCardPosition;
    
    [Header("Player Cards")]
    [Tooltip("Offsets the player cards to create the staircase layout.")]
    [SerializeField] private List<Transform> playerCardPositions = new();
    [SerializeField] private Vector2 playerCardOffset = new(10f, -10f);
    
    [Header("Dealer Cads")]
    [SerializeField] private Transform dealerCardPosition;
    [Tooltip("Space between the dealers cards.")]
    [SerializeField] private float dealerCardHorizontalSpacing = 35f; //rename to offset
    
    // General
    private Deck gameDeck;
    private List<GameObject> activeCardObjects = new();
    private Dictionary<(Card.Rank, Card.Suit), GameObject> cardPrefabLookup;
    public static readonly Vector3 CardScaleVector = Vector3.one * 0.05f;
    public static readonly float CardAnimationDuration = 0.25f;
    private const float ZOverlap = 0.001f;

    // Peek
    private CardInstance peekCardInstance = null;
    
    // Player
    private List<List<CardInstance>> playerHands = new();
    private int currentHandIndex = 0;
    
    // Dealer
    private List<CardInstance> dealerHand = new();
    
    // TODO: remove
    public CursorDetection cursorDetection;

    #region Monobehaviour Methods

    private void Start()
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
        // handBets.Clear();
        
        CardEffects.ClearAlcoholCards();
        CardEffects.ClearCutCards();
        
        DestroyActiveCards(); // TODO: remove after tarot revision
        DestroyPeekCard();
        
        playerHands.ForEach(hand => hand.Clear());
        playerHands.Clear();
        dealerHand.Clear();
    }

    #endregion

    #region Helper Methods

    private void DestroyPeekCard()
    {
        if (peekCardInstance == null) return;
        
        Destroy(peekCardInstance.CardObject);
        peekCardInstance = null;
    }

    private void DestroyActiveCards()
    {
        // TODO: revise tarot cards
        // foreach(GameObject cardObject in activeCardObjects)
        // {
        //     if(cardObject != null)
        //     {
        //         ClickableCard clickable = cardObject.GetComponentInChildren<ClickableCard>();
        //
        //         if(clickable != null && cursorDetection != null)
        //             cursorDetection.RemoveRoundActiveClickable(clickable);
        //
        //         Destroy(cardObject);
        //     }
        // }
        
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
    //
    // public IEnumerator DealRoundCoroutine()
    // {
    //     
    //     yield return PlayTutorial();
    //
    //     // if(isRoundActive || PlayerMoney < currentBet) yield break;
    //
    //     // isActionLocked = true;
    //     // isRoundActive = true;
    //     playerHands.Clear();
    //     playerHands.Add(new List<CardInstance>());
    //     // handBets.Clear();
    //     // handBets.Add(isTutorialActive ? 0 : currentBet);
    //     currentHandIndex = 0;
    //     // buttonAnimator.SetBool("StartActive", false);
    //
    //     AudioManager.instance.Play("Button");
    //
    //     yield return new WaitForSeconds(0.5f);
    //     // yield return eventManager.ChangeBlackjackGoal();
    //
    //     // ChangeToCamera(CameraType.Playing);
    //
    //     // statusText.text = "Dealing cards...";
    //     cursorDetection.OnRoundActive();
    //     // itemManager.ChangeItemAction(true);
    //
    //     // if(roundsCompleted < riggedRoundsLimit)
    //     // {
    //     //     RigPlayerHand();
    //     // }
    //
    //     yield return StartCoroutine(DealCardToPlayerCoroutine());
    //     yield return StartCoroutine(DealCardToDealerCoroutine(true));
    //     yield return StartCoroutine(DealCardToPlayerCoroutine());
    //     yield return StartCoroutine(DealCardToDealerCoroutine(false));
    //
    //     // UpdateUI();
    //
    //     if(IsBlackjack(CalculateHandValue(playerHands[0], true)))
    //     {
    //         canDoubleDown = false;
    //         statusText.text = "Blackjack!";
    //         dialogueSystem.ShowPlayerBlackjackTaunt();
    //
    //         yield return new WaitWhile(() => dialogueSystem.IsPlaying);
    //         yield return StartCoroutine(CheckPowerballCurrentHand());
    //
    //         StartCoroutine(DealerTurnCoroutine(true));
    //     }
    //     else
    //     {
    //         statusText.text = "";
    //         isActionLocked = false;
    //
    //         EvaluateDoubleDownCondition();
    //
    //         if(!hasSeenSplitTutorial && CanSplit() && roundsCompleted >= 2)
    //         {
    //             isActionLocked = true;
    //             hasSeenSplitTutorial = true;
    //             dialogueSystem.PlaySplitTutorial();
    //
    //             yield return new WaitWhile(() => dialogueSystem.IsPlaying);
    //
    //             isActionLocked = false;
    //         }
    //
    //         if(!hasSeenDoubleDownTutorial && roundsCompleted >= 7 && canDoubleDown)
    //         {
    //             isActionLocked = true;
    //             hasSeenDoubleDownTutorial = true;
    //             dialogueSystem.PlayDoubleDownTutorial();
    //
    //             yield return new WaitWhile(() => dialogueSystem.IsPlaying);
    //
    //             isActionLocked = false;
    //         }
    //     }
    // }
    //
    // private IEnumerator PlayTutorial()
    // {
    //     // if(!tutorialCompleted)
    //     // {
    //     //     tutorialCompleted = true;
    //     //     dialogueSystem.PlayTutorial();
    //     //
    //     //     yield return new WaitWhile(() => dialogueSystem.IsPlaying);
    //     // }
    // }
    //
    // //Instantiates a card, sets its data, and adds it to the specified hand.
    // private CardInstance DealCardInstance(Card newCardData, List<CardInstance> hand, Transform parentTransform, bool isHidden)
    // {
    //     if(!cardPrefabLookup.TryGetValue((newCardData.rank, newCardData.suit), out GameObject cardPrefabToUse)) return null;
    //
    //     GameObject cardObject = Instantiate(cardPrefabToUse, deckPosition);
    //
    //     cardObject.transform.localScale = cardScaleVector;
    //
    //     activeCardObjects.Add(cardObject);
    //
    //     CardDisplay cardDisplay = cardObject.GetComponent<CardDisplay>();
    //
    //     bool isSuitNegative = IsCardNegative(newCardData);
    //     bool isDoubled = eventManager.CheckIfDoubled(newCardData);
    //     bool isHalved = eventManager.CheckIfHalved(newCardData);
    //
    //     cardDisplay.SetNegativeVisual(isSuitNegative);
    //     cardDisplay.SetDoubledVisual(isDoubled);
    //     cardDisplay.SetCutVisual(isHalved);
    //
    //     if(cardDisplay != null) cardDisplay.SetHidden(isHidden);
    //
    //     CardInstance newCardInstance = new CardInstance(newCardData, cardDisplay, isHidden);
    //
    //     if(newCardInstance.cardData.rank == Card.Rank.Joker)
    //     {
    //         newCardInstance.jokerValue = Rand (-10, 11); //Joker value between -10 and 10
    //     }
    //
    //     if(newCardData.suit == Card.Suit.Tarot && hand != dealerHand)
    //     {
    //         ClickableCard clickableCard = cardObject.GetComponentInChildren<ClickableCard>();
    //
    //         if(clickableCard != null)
    //         {
    //             clickableCard.SetCardInstance(newCardInstance);
    //             clickableCard.SetBlackjackGame(this);
    //
    //             cursorDetection.AddRoundActiveClickable(clickableCard);
    //
    //             clickableCard.SetActive(true);
    //         }
    //     }
    //
    //     hand?.Insert(0, newCardInstance);
    //
    //     return newCardInstance;
    // }
    //
    // public CardInstance DealCardInstanceOption(Card newCardData, bool isHidden)
    // {
    //     if(!cardPrefabLookup.TryGetValue((newCardData.rank, newCardData.suit), out GameObject cardPrefabToUse)) return null;
    //
    //     GameObject cardObject = Instantiate(cardPrefabToUse, deckPosition);
    //
    //     cardObject.transform.localScale = cardScaleVector;
    //
    //     CardDisplay cardDisplay = cardObject.GetComponent<CardDisplay>();
    //
    //     bool isSuitNegative = CardEffects.IsSuitNegative(newCardData.suit);
    //     bool isDoubled = eventManager.CheckIfDoubled(newCardData);
    //     bool isHalved = eventManager.CheckIfHalved(newCardData);
    //
    //     cardDisplay.SetNegativeVisual(isSuitNegative);
    //     cardDisplay.SetDoubledVisual(isDoubled);
    //     cardDisplay.SetCutVisual(isHalved);
    //
    //     if(cardDisplay != null) cardDisplay.SetHidden(isHidden);
    //
    //     CardInstance newCardInstance = new CardInstance(newCardData, cardDisplay, isHidden);
    //
    //     if(newCardInstance.cardData.rank == Card.Rank.Joker)
    //     {
    //         newCardInstance.jokerValue = Random.Range(-10, 11); //Joker value between -10 and 10
    //     }
    //
    //     return newCardInstance;
    // }
    //
    // public Card DealCard()
    // {
    //     return gameDeck.DealCard();
    // }
    //
    // private IEnumerator DealCardToPlayerCoroutine()
    // {
    //     var savedPosition = deckPosition.position;
    //
    //     Card newCardData = new Card { rank = Card.Rank.None };
    //
    //     bool cardFound = false;
    //
    //     List<CardInstance> currentHand = playerHands[currentHandIndex];
    //
    //     if(CrucifixItem.isCrucifixActive)
    //     {
    //         int playerValue = CalculateHandValue(currentHand, true);
    //         int idealValue = blackjackGoal - playerValue;
    //
    //         Card.Rank targetRank = GetRankForValue(idealValue);
    //         Card? dealtCard = gameDeck.DealSpecificCard(targetRank);
    //         
    //         CrucifixItem.isCrucifixActive = false;
    //
    //         if(!dealtCard.HasValue)
    //         {
    //             int searchStart = Mathf.Min(idealValue, 10);
    //
    //             for(int v = searchStart; v >= 2; v--)
    //             {
    //                 if(v == 10)
    //                 {
    //                     Card.Rank[] faces = { Card.Rank.Ten, Card.Rank.Jack, Card.Rank.Queen, Card.Rank.King };
    //
    //                     foreach(var f in faces)
    //                     {
    //                         dealtCard = gameDeck.DealSpecificCard(f);
    //
    //                         if(dealtCard.HasValue) break;
    //                     }
    //                 }
    //                 else
    //                 {
    //                     dealtCard = gameDeck.DealSpecificCard((Card.Rank)v);
    //                 }
    //
    //                 if(dealtCard.HasValue) break;
    //             }
    //         }
    //
    //         if(!dealtCard.HasValue)
    //         {
    //             dealtCard = gameDeck.DealSpecificCard(Card.Rank.Ace);
    //         }
    //
    //         if(dealtCard.HasValue)
    //         {
    //             newCardData = dealtCard.Value;
    //             cardFound = true;
    //         }
    //     }
    //
    //     if(!cardFound)
    //     {
    //         newCardData = gameDeck.DealCard();
    //     }
    //
    //     Transform currentParent = handPositions[currentHandIndex];
    //     
    //     CardInstance newCardInstance;
    //     if (peekCardInstance == null) 
    //         newCardInstance = DealCardInstance(newCardData, currentHand, currentParent, false);
    //     else
    //     {
    //         newCardInstance = peekCardInstance;
    //         currentHand.Insert(0, newCardInstance);
    //         peekCardInstance = null;
    //     }
    //     AudioManager.instance.Play("CardHit");
    //
    //     if(newCardInstance != null)
    //     {
    //         int cardOrderIndex = currentHand.Count - 1;
    //         float xOffset = cardOrderIndex * playerCardOffset.x;
    //         float yOffset = cardOrderIndex * playerCardOffset.y;
    //         float zOffset = cardOrderIndex * -zOverlap;
    //
    //         Vector3 targetLocalPos = new Vector3(xOffset, yOffset, zOffset);
    //         Quaternion targetRotation = Quaternion.identity;
    //
    //         newCardInstance.displayComponent.transform.SetParent(currentParent.parent);
    //
    //         yield return StartCoroutine(CardAnimationCoroutine(
    //             newCardInstance.displayComponent.transform,
    //             currentParent.TransformPoint(targetLocalPos),
    //             currentParent.rotation * targetRotation,
    //             cardScaleVector,
    //             cardAnimationDuration
    //         ));
    //
    //         newCardInstance.displayComponent.transform.SetParent(currentParent);
    //         newCardInstance.displayComponent.transform.localPosition = targetLocalPos;
    //         newCardInstance.displayComponent.transform.localRotation = targetRotation;
    //
    //         UpdateHandVisuals(currentHand, currentParent, true);
    //         UpdateUI(true);
    //         UpdateSplitOutlines();
    //     }
    //
    //     deckPosition.position = savedPosition;
    // }
    //
    // private IEnumerator DealCardToDealerCoroutine(bool isHidden)
    // {
    //     Card newCardData = new Card { rank = Card.Rank.None };
    //
    //     bool cardFound = false;
    //     
    //     if(CrucifixItem.isCrucifixActive)
    //     {
    //         int dealerValue = CalculateHandValue(dealerHand, true);
    //         int idealValue;
    //
    //         if (dealerValue >= 12) idealValue = 10;
    //         else if (dealerValue >= 6) idealValue = 16 - dealerValue;
    //         else idealValue = 12 - dealerValue; 
    //
    //         Card.Rank targetRank = GetRankForValue(idealValue);
    //         Card? dealtCard = gameDeck.DealSpecificCard(targetRank);
    //         
    //         CrucifixItem.isCrucifixActive = false;
    //
    //         if(!dealtCard.HasValue)
    //         {
    //             int searchStart = Mathf.Min(idealValue, 10);
    //
    //             for(int v = searchStart; v >= 2; v--)
    //             {
    //                 if(v == 10)
    //                 {
    //                     Card.Rank[] faces = { Card.Rank.Ten, Card.Rank.Jack, Card.Rank.Queen, Card.Rank.King };
    //
    //                     foreach(var f in faces)
    //                     {
    //                         dealtCard = gameDeck.DealSpecificCard(f);
    //
    //                         if(dealtCard.HasValue) break;
    //                     }
    //                 }
    //                 else
    //                 {
    //                     dealtCard = gameDeck.DealSpecificCard((Card.Rank)v);
    //                 }
    //
    //                 if(dealtCard.HasValue) break;
    //             }
    //         }
    //
    //         if(!dealtCard.HasValue)
    //         {
    //             dealtCard = gameDeck.DealSpecificCard(Card.Rank.Ace);
    //         }
    //
    //         if(dealtCard.HasValue)
    //         {
    //             newCardData = dealtCard.Value;
    //             cardFound = true;
    //         }
    //     }
    //
    //     if(!cardFound)
    //     {
    //         newCardData = gameDeck.DealCard();
    //     }
    //     
    //     CardInstance newCardInstance;
    //     if (peekCardInstance == null) 
    //         newCardInstance = DealCardInstance(newCardData, dealerHand, dealerCardPosition, isHidden);
    //     else
    //     {
    //         newCardInstance = peekCardInstance;
    //         dealerHand.Insert(0, newCardInstance);
    //         peekCardInstance = null;
    //     }
    //     AudioManager.instance.Play("CardHit");
    //
    //     if(newCardInstance != null)
    //     {
    //         int cardOrderIndex = dealerHand.Count - 1;
    //         float xOffset = cardOrderIndex * dealerCardHorizontalSpacing;
    //         float yOffset = 0f;
    //         float zOffset = cardOrderIndex * -zOverlap;
    //
    //         Vector3 targetLocalPos = new Vector3(xOffset, yOffset, zOffset);
    //         Quaternion targetRotation = Quaternion.identity;
    //
    //         newCardInstance.displayComponent.transform.SetParent(dealerCardPosition.parent);
    //
    //         yield return StartCoroutine(CardAnimationCoroutine(
    //             newCardInstance.displayComponent.transform,
    //             dealerCardPosition.TransformPoint(targetLocalPos),
    //             dealerCardPosition.rotation * targetRotation,
    //             cardScaleVector,
    //             cardAnimationDuration
    //         ));
    //
    //         newCardInstance.displayComponent.transform.SetParent(dealerCardPosition);
    //         newCardInstance.displayComponent.transform.localPosition = targetLocalPos;
    //         newCardInstance.displayComponent.transform.localRotation = targetRotation;
    //         newCardInstance.displayComponent.transform.localScale = cardScaleVector;
    //
    //         UpdateHandVisuals(dealerHand, dealerCardPosition, false);
    //         UpdateUI(true);
    //     }
    // }
    
    #endregion
    
}
