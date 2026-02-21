using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BlackjackGame : MonoBehaviour
{
    #region Attributes
    [SerializeField] private ItemManager itemManager;
    
    [System.Serializable]
    public class EventThreshold
    {
        public BlackjackEvent.EventSeverity severityToTrigger;

        public int moneyAmount;
    }

    [Header("Event System")]
    [SerializeField] private List<EventThreshold> eventThresholds;
    [SerializeField] private List<BlackjackEvent> lowSeverityEvents;
    [SerializeField] private List<BlackjackEvent> mediumSeverityEvents;
    [SerializeField] private List<BlackjackEvent> highSeverityEvents;
    private AceValueRule currentAceRule = AceValueRule.Flexible;
    public enum AceValueRule { Flexible, Always1, Always11 }
    private bool isDoubleLowActive = false;
    private bool isHalfHighActive = false;
    private bool isRouletteBlackjackActive = false;
    private int blackjackGoal = 21;
    private List<Card.Suit> negativeSuits = new List<Card.Suit>();

    private List<BlackjackEvent> availableLowEvents;
    private List<BlackjackEvent> availableMediumEvents;
    private List<BlackjackEvent> availableHighEvents;
    private List<EventThreshold> triggeredThresholds = new List<EventThreshold>();

    private Deck gameDeck;

    private List<CardInstance> playerHand = new List<CardInstance>();
    private List<CardInstance> dealerHand = new List<CardInstance>();
    private List<GameObject> activeCardObjects = new List<GameObject>();

    [SerializeField] private Animator standHandAnimator;
    [SerializeField] private Animator hitHandAnimator;
    [SerializeField] private Animator buttonAnimator;

    [Header("Camera")]
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private CinemachineCamera sittingCamera;
    [SerializeField] private CinemachineCamera playingCamera;
    [SerializeField] private CinemachineCamera eventCamera;
    [SerializeField] private float cameraTransitionTime;

    [Header("UI")]
    [SerializeField] private TMPro.TextMeshProUGUI moneyText;
    [SerializeField] private TMPro.TextMeshProUGUI betText;
    [SerializeField] private TMPro.TextMeshProUGUI statusText;
    [SerializeField] private TMPro.TextMeshProUGUI playerTotalText;
    [SerializeField] private TMPro.TextMeshProUGUI dealerTotalText;
    [SerializeField] private GameObject greenParticlePrefab;
    [SerializeField] private GameObject redParticlePrefab;
    [SerializeField] private Transform particleSpawnPoint;

    [SerializeField] private GameObject dealerSmile;
    [SerializeField] private CursorDetection cursorDetection;

    //Betting Variables
    private int playerMoney = 500;
    private int currentBet = 100;
    private int minBet = 100;
    private const int betStep = 100;

    [HideInInspector] public bool isRoundActive = false;
    private bool isActionLocked = false;

    private Coroutine currentBustCoroutine = null;

    //Abilities
    private bool isKnifeActive = false;
    private int scissorsValueReduction = 0;
    private bool isCrucifixActive = false;
    private GameObject peekedCardObject = null;
    public bool IsKnifeAvailable { get; private set; } = true;
    public bool IsScissorsAvailable { get; private set; } = true;
    public bool IsCrucifixAvailable { get; private set; } = true;
    public bool IsSunglassesAvailable { get; private set; } = true;

    public int PlayerMoney
    {
        get { return playerMoney; }
        private set { playerMoney = value; }
    }

    [Header("Visual Setup")]
    [SerializeField] private List<CardVisuals> cardPrefabs = new List<CardVisuals>();

    private Dictionary<(Card.Rank, Card.Suit), GameObject> cardPrefabLookup;

    [SerializeField] private Transform playerCardPosition;
    [SerializeField] private Transform dealerCardPosition;
    [SerializeField] private Transform sunglassesCardPosition;
    [SerializeField] private Transform deckPosition;

    [SerializeField] private float cardSpacing = 30.0f;
    [SerializeField] private float cardRotationAngle = 5.0f;
    private float cardArcHeight = 0f;
    private const float zOverlap = 0.001f;
    private const float cardAnimationDuration = 0.25f;

    private float nextKeyBetTime = 0f;
    private float keyRepeatDelay = 0.5f;
    private float keyRepeatRate = 0.1f;

    private readonly Vector3 cardScaleVector = Vector3.one * 0.05f;
    #endregion

    #region Monobehaviour Methods

    private void Start()
    {
        gameDeck = new Deck();
        availableLowEvents = new List<BlackjackEvent>(lowSeverityEvents);
        availableMediumEvents = new List<BlackjackEvent>(mediumSeverityEvents);
        availableHighEvents = new List<BlackjackEvent>(highSeverityEvents);

        cinemachineBrain.DefaultBlend.Time = cameraTransitionTime;

        InitializeCardLookup();
        StartGame();

        AudioManager.instance.Play("MainTheme");
    }

    private void Update()
    {
        if(currentBustCoroutine != null || isActionLocked) return;

        if(!isRoundActive)
        {
            if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                IncreaseBet();

                nextKeyBetTime = Time.time + keyRepeatDelay;
            }
            else if(Input.GetKey(KeyCode.UpArrow) && Time.time >= nextKeyBetTime)
            {
                IncreaseBet();

                nextKeyBetTime = Time.time + keyRepeatRate;
            }

            if(Input.GetKeyDown(KeyCode.DownArrow))
            {
                DecreaseBet();

                nextKeyBetTime = Time.time + keyRepeatDelay;
            }
            else if(Input.GetKey(KeyCode.DownArrow) && Time.time >= nextKeyBetTime)
            {
                DecreaseBet();
                nextKeyBetTime = Time.time + keyRepeatRate;
            }

            bool canDeal = PlayerMoney >= currentBet;

            if(Input.GetKeyDown(KeyCode.H) && canDeal) StartCoroutine(DealRoundCoroutine());
        }
        else //Handle playing actions.
        {
            // TODO: Delete after adding click-only gameplay
            
            if(Input.GetKeyDown(KeyCode.H)) StartCoroutine(HitCoroutine());

            if(Input.GetKeyDown(KeyCode.S)) StartCoroutine(StandCoroutine());
        }
    }
    #endregion
    
    #region Player Actions
    
        // These methods will be added as unity events in the Clickable components
        public void OnStartGame()
        {
            if (!isRoundActive && PlayerMoney >= currentBet)
                StartCoroutine(DealRoundCoroutine());
        }
        
        public void OnHit() => StartCoroutine(HitCoroutine());
        
        public void OnStand() => StartCoroutine(StandCoroutine());

        public void OnIncreaseBet() => IncreaseBet();
        
        public void OnDecreaseBet() => DecreaseBet();

    #endregion
    
    private void ClearTable()
    {
        foreach(GameObject cardObject in activeCardObjects)
        {
            if(cardObject != null)
            {
                Destroy(cardObject);
            }
        }

        activeCardObjects.Clear();
        playerHand.Clear();
        dealerHand.Clear();

        if(peekedCardObject != null)
        {
            Destroy(peekedCardObject);

            peekedCardObject = null;
        }
    }

    //Helper function to update all betting related text and button states
    public void UpdateBettingUI()
    {
        moneyText.text = $"${PlayerMoney}";
        betText.text = $"${currentBet}";
    }

    public void ResetItemsAvailable()
    {
        IsCrucifixAvailable = true;
        IsScissorsAvailable = true;
        IsSunglassesAvailable = true;
    }
    
    public void IncreaseBet()
    {
        if(isRoundActive) return;

        if(currentBet < PlayerMoney)
        {
            AudioManager.instance.Play("BetUp");

            int nextBet = currentBet + betStep;

            if(nextBet > PlayerMoney)
            {
                currentBet = PlayerMoney;
            }
            else
            {
                currentBet = nextBet;
            }
        }

        UpdateBettingUI();
    }

    public void DecreaseBet()
    {
        if(isRoundActive) return;

        if(currentBet > minBet)
        {
            AudioManager.instance.Play("BetDown");

            currentBet -= betStep;
        }

        if(currentBet < minBet)
        {
            currentBet = minBet;
        }

        UpdateBettingUI();
    }

    private void EnableCamera(CinemachineCamera camera)
    {
        camera.Priority = 10;
    }

    private void DisableCamera(CinemachineCamera camera)
    {
        camera.Priority = 0;
    }

    #region Item Methods
    public bool ActivateKnife()
    {
        if(!isRoundActive || isKnifeActive || !IsKnifeAvailable) return false;

        isKnifeActive = true;
        IsKnifeAvailable = false;
        return true;
    }

    public bool ActivateScissors()
    {
        if(!isRoundActive || !IsScissorsAvailable) return false;

        if(CalculateHandValue(playerHand, true) > blackjackGoal) return false;

        if(dealerHand.Count < 2) return false;

        CardInstance visibleDealerCard = dealerHand[1];

        int originalValue;

        if(visibleDealerCard.cardData.rank == Card.Rank.Joker)
        {
            originalValue = 0;
        }
        else
        {
            originalValue = visibleDealerCard.cardData.GetValue();

            if(isDoubleLowActive && originalValue < 6)
            {
                originalValue *= 2;
            }

            if(isHalfHighActive && originalValue > 5)
            {
                originalValue = Mathf.CeilToInt(originalValue / 2f);
            }
        }

        int halvedValue = Mathf.CeilToInt((float)originalValue / 2f);

        scissorsValueReduction = originalValue - halvedValue;
        IsScissorsAvailable = false;

        UpdateUI(true);
        return true;
    }

    public bool ActivateCrucifix()
    {
        if(!isRoundActive || isCrucifixActive || !IsCrucifixAvailable) return false;

        if(CalculateHandValue(playerHand, true) > blackjackGoal) return false;

        isCrucifixActive = true;
        IsCrucifixAvailable = false;
        return true;
    }

    public bool ActivateSunglasses()
    {
        if(!isRoundActive || !IsSunglassesAvailable || peekedCardObject != null) return false;

        if(CalculateHandValue(playerHand, true) > blackjackGoal) return false;

        Card? nextCard = gameDeck.PeekCard();

        if(!nextCard.HasValue) return false;

        Card newCardData = nextCard.Value;

        if(!cardPrefabLookup.TryGetValue((newCardData.rank, newCardData.suit), out GameObject cardPrefabToUse)) return false;

        peekedCardObject = Instantiate(cardPrefabToUse, deckPosition);
        peekedCardObject.transform.localScale = cardScaleVector;

        StartCoroutine(CardAnimationCoroutine(
            peekedCardObject.transform,
            sunglassesCardPosition.position,
            sunglassesCardPosition.rotation,
            cardScaleVector,
            cardAnimationDuration
        ));

        CardDisplay cardDisplay = peekedCardObject.GetComponent<CardDisplay>();

        if(cardDisplay != null)
        {
            cardDisplay.SetHidden(false);
        }

        activeCardObjects.Add(peekedCardObject);
        IsSunglassesAvailable = false;
        return true;
    }

    //Helper method for Crucifix.
    private Card.Rank GetBestRankForValue(int bestValue)
    {
        if(bestValue >= 11 || bestValue == 1)
        {
            return Card.Rank.Ace;
        }

        switch(bestValue)
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
    #endregion

    #region Event Methods
    public void RemoveValueFromDeck(Card.Rank rank)
    {
        gameDeck.AddRemovedValue(rank);
    }

    public void RemoveSuitFromDeck(Card.Suit suit)
    {
        gameDeck.AddRemovedSuit(suit);
    }

    public void SetAceRule(AceValueRule newRule)
    {
        currentAceRule = newRule;
    }

    public void AddJokers()
    {
        gameDeck.AddJokersToDeck();
    }

    public void SetNegativeSuit(Card.Suit suit)
    {
        negativeSuits.Add(suit);
    }

    public void SetDoubleLowActive(bool active)
    {
        isDoubleLowActive = active;
    }

    public void SetHalfHighActive(bool active)
    {
        isHalfHighActive = active;
    }

    public void SetRouletteBlackjackActive(bool active)
    {
        isRouletteBlackjackActive = active;
    }

    private void RandomizeBlackjackGoal()
    {
        blackjackGoal = Random.Range(21, 37); //from 21 to 36
    }
    #endregion

    //Calculates the total value of a hand. Aces are 1 or 11.
    private int CalculateHandValue(List<CardInstance> hand, bool aiCountsJoker)
    {
        float value = 0f;
        int aceCount = 0;

        CardInstance targetedCardInstance = null;

        if(scissorsValueReduction > 0 && dealerHand.Count > 1)
        {
            targetedCardInstance = dealerHand[1];
        }

        for(int i = 0; i < hand.Count; i++)
        {
            CardInstance cardInstance = hand[i];

            Card card = cardInstance.cardData;

            float cardValue = card.GetValue();

            if(card.rank == Card.Rank.Joker)
            {
                if(aiCountsJoker)
                {
                    cardValue = cardInstance.jokerValue;
                }
                else
                {
                    cardValue = 0;
                }
            }
            else if(card.rank == Card.Rank.Ace)
            {
                aceCount++;

                if(currentAceRule == AceValueRule.Always1)
                {
                    cardValue = 1;
                }
                else
                {
                    cardValue = 11;
                }
            }
            else if(card.rank >= Card.Rank.Ten && card.rank <= Card.Rank.King)
            {
                cardValue = 10;
            }
            else
            {
                cardValue = (int)card.rank;
            }

            if(isDoubleLowActive && cardValue < 6 && card.rank != Card.Rank.Joker)
            {
                cardValue *= 2;
            }

            if(isHalfHighActive && cardValue > 5 && card.rank != Card.Rank.Joker)
            {
                cardValue = Mathf.CeilToInt(cardValue / 2f);
            }

            if(negativeSuits.Contains(card.suit))
            {
                cardValue *= -1;
            }

            if(targetedCardInstance != null && cardInstance == targetedCardInstance)
            {
                cardValue -= scissorsValueReduction;
            }

            value += cardValue;
        }

        //adjust aces
        if(currentAceRule == AceValueRule.Flexible)
        {
            while(value > blackjackGoal && aceCount > 0)
            {
                value -= 10;
                aceCount--;
            }
        }

        return Mathf.RoundToInt(value);
    }

    //Initializes the card prefab lookup dictionary for quick access.
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

    //Resets the game and enters the betting phase
    public void StartGame()
    {
        buttonAnimator.SetBool("StartActive", true);

        ClearTable();
        DisableCamera(playingCamera);
        EnableCamera(sittingCamera);

        if(isRouletteBlackjackActive)
        {
            RandomizeBlackjackGoal();
        }

        //doorAnimator.SetBool("open", false);

        AudioManager.instance.Play("Shuffle");

        gameDeck.Shuffle();
        statusText.text = "Place your bet...";

        cursorDetection.OnRoundInactive();
        itemManager.SpawnPowerUps();
        isRoundActive = false;
        isActionLocked = false;

        //Reset abilities
        isKnifeActive = false;
        IsKnifeAvailable = true;
        IsScissorsAvailable = true;
        scissorsValueReduction = 0;
        IsCrucifixAvailable = true;
        isCrucifixActive = false;
        IsSunglassesAvailable = true;

        playerTotalText.text = "Your hand: ";
        dealerTotalText.text = "Dealer hand: ";

        //Set bet to the last valid bet
        if(PlayerMoney < minBet) currentBet = PlayerMoney;
        else if(currentBet > PlayerMoney) currentBet = PlayerMoney;
        else if(currentBet < minBet) currentBet = minBet;

        UpdateBettingUI();
    }

    //Locks the bet and starts the round
    public IEnumerator DealRoundCoroutine()
    {
        if(isRoundActive || PlayerMoney < currentBet || isActionLocked) yield break;

        buttonAnimator.SetBool("StartActive", false);

        if(isRouletteBlackjackActive)
        {
            statusText.text = $"New Blackjack goal: {blackjackGoal}";

            yield return new WaitForSeconds(3.0f);
        }

        DisableCamera(sittingCamera);
        EnableCamera(playingCamera);

        statusText.text = "Dealing cards...";
        isActionLocked = true;

        //doorAnimator.SetBool("open", true);
        isRoundActive = true;
        cursorDetection.OnRoundActive();
        itemManager.DespawnPowerUps();

        yield return StartCoroutine(DealCardToPlayerCoroutine());
        yield return StartCoroutine(DealCardToDealerCoroutine(false));
        yield return StartCoroutine(DealCardToPlayerCoroutine());
        yield return StartCoroutine(DealCardToDealerCoroutine(true));

        UpdateUI();

        if(IsBlackjack(CalculateHandValue(playerHand, true)))
        {
            statusText.text = "Blackjack!";

            yield return new WaitForSeconds(2.0f);

            StartCoroutine(DealerTurnCoroutine(true));
        }
        else
        {
            statusText.text = "";
            isActionLocked = false;
        }
    }

    private void UpdateHandVisuals(List<CardInstance> hand)
    {
        int cardCount = hand.Count;

        if(cardCount == 0) return;

        float midPoint = (cardCount - 1) / 2.0f;

        for(int i = 0; i < cardCount; i++)
        {
            CardInstance cardInstance = hand[i];

            float xPos = (i - (cardCount - 1)) * cardSpacing;
            float distanceFromCenter = i - midPoint;
            float rotationAngle = distanceFromCenter * -cardRotationAngle;

            Quaternion targetRotation = Quaternion.Euler(0, 0, rotationAngle);

            float yPos = (midPoint * midPoint - distanceFromCenter * distanceFromCenter) * cardArcHeight;
            float zPos = i * zOverlap;

            cardInstance.displayComponent.transform.localPosition = new Vector3(xPos, yPos, zPos);
            cardInstance.displayComponent.transform.localRotation = targetRotation;
        }
    }

    //Instantiates a card, sets its data, and adds it to the specified hand.
    private CardInstance DealCardInstance(Card newCardData, List<CardInstance> hand, Transform parentTransform, bool isHidden)
    {
        if(!cardPrefabLookup.TryGetValue((newCardData.rank, newCardData.suit), out GameObject cardPrefabToUse)) return null;

        GameObject cardObject = Instantiate(cardPrefabToUse, deckPosition);

        cardObject.transform.localScale = cardScaleVector;

        activeCardObjects.Add(cardObject);

        CardDisplay cardDisplay = cardObject.GetComponent<CardDisplay>();

        if(cardDisplay == null) return null;

        cardDisplay.SetHidden(isHidden);

        CardInstance newCardInstance = new CardInstance(newCardData, cardDisplay, isHidden);

        if(newCardInstance.cardData.rank == Card.Rank.Joker)
        {
            newCardInstance.jokerValue = Random.Range(-10, 11); //Joker value between -10 and 10
        }

        hand.Insert(0, newCardInstance);

        UpdateHandVisuals(hand);

        return newCardInstance;
    }
    
    private IEnumerator CheckForEventTriggerCoroutine()
    {
        EventThreshold thresholdToTrigger = null;

        foreach(EventThreshold threshold in eventThresholds)
        {
            if(PlayerMoney >= threshold.moneyAmount && !triggeredThresholds.Contains(threshold))
            {
                thresholdToTrigger = threshold;

                break;
            }
        }

        if(thresholdToTrigger != null)
        {
            triggeredThresholds.Add(thresholdToTrigger);

            List<BlackjackEvent> eventPool = null;

            switch(thresholdToTrigger.severityToTrigger)
            {
                case BlackjackEvent.EventSeverity.Low:
                    eventPool = availableLowEvents;
                break;

                case BlackjackEvent.EventSeverity.Medium:
                    eventPool = availableMediumEvents;
                break;

                case BlackjackEvent.EventSeverity.High:
                    eventPool = availableHighEvents;
                break;
            }

            if(eventPool != null && eventPool.Count > 0)
            {
                DisableCamera(playingCamera);
                EnableCamera(eventCamera);

                int randomIndex = Random.Range(0, eventPool.Count);

                BlackjackEvent chosenEvent = eventPool[randomIndex];

                chosenEvent.Apply(this);
                eventPool.RemoveAt(randomIndex);

                statusText.text = "Lets make it more interesting";

                AudioManager.instance.Play("Laugh");

                dealerSmile.SetActive(true);

                yield return new WaitForSeconds(5.0f);

                AudioManager.instance.Play("NewEvent");

                statusText.text = $"New Event: {chosenEvent.eventName}";

                yield return new WaitForSeconds(6.0f);

                dealerSmile.SetActive(false);

                DisableCamera(eventCamera);
                EnableCamera(sittingCamera);
            }
        }
    }

    private IEnumerator CardAnimationCoroutine(Transform cardTransform, Vector3 targetPosition, Quaternion targetRotation, Vector3 targetScale, float duration)
    {
        Vector3 startPosition = cardTransform.position;
        Quaternion startRotation = cardTransform.rotation;
        Vector3 startScale = cardTransform.localScale;

        float time = 0;

        while(time < duration)
        {
            if(cardTransform == null) yield break;

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

    //Flip animation for revealing the hidden card.
    private IEnumerator FlipCardCoroutine(CardDisplay cardDisplay, float duration)
    {
        Transform cardTransform = cardDisplay.transform;

        Quaternion startRotation = cardTransform.localRotation;
        Quaternion ninetyDegrees = Quaternion.Euler(0, 90f, startRotation.eulerAngles.z);

        float halfDuration = duration / 2.0f;
        float elapsedTime = 0;

        AudioManager.instance.Play("Flip");

        while(elapsedTime < halfDuration)
        {
            cardTransform.localRotation = Quaternion.Slerp(startRotation, ninetyDegrees, elapsedTime / halfDuration);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        cardDisplay.SetHidden(false);

        Quaternion flippedStartRotation = Quaternion.Euler(0, -90f, startRotation.eulerAngles.z);

        cardTransform.localRotation = flippedStartRotation;
        elapsedTime = 0;

        while(elapsedTime < halfDuration)
        {
            cardTransform.localRotation = Quaternion.Slerp(flippedStartRotation, startRotation, elapsedTime / halfDuration);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        cardTransform.localRotation = startRotation;
    }

    private IEnumerator DealCardToPlayerCoroutine()
    {
        var savedPosition = deckPosition.position;

        if(peekedCardObject != null)
        {
            deckPosition.position = sunglassesCardPosition.position;
            activeCardObjects.Remove(peekedCardObject);

            Destroy(peekedCardObject);

            peekedCardObject = null;
        }

        Card newCardData = new Card { rank = Card.Rank.None };

        bool cardFound = false;

        if(isCrucifixActive)
        {
            isCrucifixActive = false;

            int playerValue = CalculateHandValue(playerHand, true);
            int idealValue = blackjackGoal - playerValue;

            Card? dealtCard = null;
            Card.Rank targetRank = GetBestRankForValue(idealValue);

            dealtCard = gameDeck.DealSpecificCard(targetRank);

            if(!dealtCard.HasValue)
            {
                int searchStart = Mathf.Min(idealValue, 10);

                for(int v = searchStart; v >= 2; v--)
                {
                    if(v == 10)
                    {
                        Card.Rank[] faces = { Card.Rank.Ten, Card.Rank.Jack, Card.Rank.Queen, Card.Rank.King };

                        foreach(var f in faces)
                        {
                            dealtCard = gameDeck.DealSpecificCard(f);

                            if(dealtCard.HasValue) break;
                        }
                    }
                    else
                    {
                        dealtCard = gameDeck.DealSpecificCard((Card.Rank)v);
                    }

                    if(dealtCard.HasValue) break;
                }
            }

            if(!dealtCard.HasValue)
            {
                dealtCard = gameDeck.DealSpecificCard(Card.Rank.Ace);
            }

            if(dealtCard.HasValue)
            {
                newCardData = dealtCard.Value;
                cardFound = true;
            }
        }

        if(!cardFound)
        {
            newCardData = gameDeck.DealCard();
        }

        CardInstance newCardInstance = DealCardInstance(newCardData, playerHand, playerCardPosition, false);
        AudioManager.instance.Play("CardHit");

        if(newCardInstance != null)
        {
            UpdateHandVisuals(playerHand);

            Vector3 targetLocalPos = newCardInstance.displayComponent.transform.localPosition;
            Quaternion targetRotation = newCardInstance.displayComponent.transform.localRotation;

            newCardInstance.displayComponent.transform.SetParent(playerCardPosition.parent);

            yield return StartCoroutine(CardAnimationCoroutine(
                newCardInstance.displayComponent.transform,
                playerCardPosition.TransformPoint(targetLocalPos),
                playerCardPosition.rotation * targetRotation,
                cardScaleVector,
                cardAnimationDuration
            ));

            newCardInstance.displayComponent.transform.SetParent(playerCardPosition);
            newCardInstance.displayComponent.transform.localPosition = targetLocalPos;
            newCardInstance.displayComponent.transform.localRotation = targetRotation;
            newCardInstance.displayComponent.transform.localScale = cardScaleVector;

            UpdateUI(true);
        }

        deckPosition.position = savedPosition;
    }

    private IEnumerator DealCardToDealerCoroutine(bool isHidden)
    {
        if(peekedCardObject != null)
        {
            activeCardObjects.Remove(peekedCardObject);

            Destroy(peekedCardObject);

            peekedCardObject = null;
        }

        Card newCardData = gameDeck.DealCard();

        CardInstance newCardInstance = DealCardInstance(newCardData, dealerHand, dealerCardPosition, isHidden);
        AudioManager.instance.Play("CardHit");

        if(newCardInstance != null)
        {
            UpdateHandVisuals(dealerHand);

            Vector3 targetLocalPos = newCardInstance.displayComponent.transform.localPosition;
            Quaternion targetRotation = newCardInstance.displayComponent.transform.localRotation;

            newCardInstance.displayComponent.transform.SetParent(dealerCardPosition.parent);

            yield return StartCoroutine(CardAnimationCoroutine(
                newCardInstance.displayComponent.transform,
                dealerCardPosition.TransformPoint(targetLocalPos),
                dealerCardPosition.rotation * targetRotation,
                cardScaleVector,
                cardAnimationDuration
            ));

            newCardInstance.displayComponent.transform.SetParent(dealerCardPosition);
            newCardInstance.displayComponent.transform.localPosition = targetLocalPos;
            newCardInstance.displayComponent.transform.localRotation = targetRotation;
            newCardInstance.displayComponent.transform.localScale = cardScaleVector;

            UpdateUI(true);
        }
    }

    //Updates the score, money, and checks for busts.
    private void UpdateUI(bool dealerHidden = true)
    {
        int playerValue = CalculateHandValue(playerHand, true);
        bool revealJokers = !dealerHidden;
        bool playerHasJoker = playerHand.Any(c => c.cardData.rank == Card.Rank.Joker);

        if(playerHand.Count == 0)
        {
            playerTotalText.text = "Your hand: ";
        }
        else if(revealJokers || !playerHasJoker)
        {
            playerTotalText.text = "Your hand: " + playerValue.ToString();
        }
        else
        {
            int jokerSum = playerHand.Where(c => c.cardData.rank == Card.Rank.Joker).Sum(c => c.jokerValue);
            int baseValue = playerValue - jokerSum;

            playerTotalText.text = "Your hand: " + $"{baseValue} + ?";
        }

        if(dealerHand.Count > 0)
        {
            if(dealerHidden && dealerHand.Any(c => c.isHidden))
            {
                List<CardInstance> visibleCards = dealerHand.Where(x => !x.isHidden).ToList();

                int dealerVisibleValue = CalculateHandValue(visibleCards, true);
                bool dealerHasVisibleJoker = visibleCards.Any(c => c.cardData.rank == Card.Rank.Joker);

                if(dealerHasVisibleJoker)
                {
                    int jokerSum = visibleCards.Where(c => c.cardData.rank == Card.Rank.Joker).Sum(c => c.jokerValue);
                    int baseValue = dealerVisibleValue - jokerSum;

                    dealerTotalText.text = "Dealer hand: " + $"{baseValue} + ? + ?";
                }
                else
                {
                    dealerTotalText.text = "Dealer hand: " + $"{dealerVisibleValue} + ?";
                }
            }
            else
            {
                int dealerFullValue = CalculateHandValue(dealerHand, true);
                bool dealerHasJoker = dealerHand.Any(c => c.cardData.rank == Card.Rank.Joker);

                if(revealJokers || !dealerHasJoker)
                {
                    dealerTotalText.text = "Dealer hand: " + dealerFullValue.ToString();
                }
                else
                {
                    int jokerSum = dealerHand.Where(c => c.cardData.rank == Card.Rank.Joker).Sum(c => c.jokerValue);
                    int baseValue = dealerFullValue - jokerSum;

                    dealerTotalText.text = "Dealer hand: " + $"{baseValue} + ?";
                }
            }
        }
        else
        {
            dealerTotalText.text = "Dealer hand: ";
        }

        UpdateBettingUI();

        if((playerValue > blackjackGoal || playerValue < -blackjackGoal ) && isRoundActive && currentBustCoroutine == null)
        {
            currentBustCoroutine = StartCoroutine(BustCheckCoroutine());
        }
    }

    private IEnumerator BustCheckCoroutine()
    {
        yield return new WaitForSeconds(2f);

        UpdateUI(true);

        var playerJokers = playerHand.Where(c => c.cardData.rank == Card.Rank.Joker).ToList();

        string revealMessage = "";

        if(playerJokers.Count > 0)
        {
            revealMessage += "Your Joker(s): ";
            revealMessage += string.Join(", ", playerJokers.Select(j => j.jokerValue.ToString()));
            revealMessage += ". ";
        }

        if(!string.IsNullOrEmpty(revealMessage))
        {
            statusText.text = revealMessage;

            yield return new WaitForSeconds(2.5f);
        }

        statusText.text = "Bust... You lose";

        yield return StartCoroutine(EndGameCoroutine("Bust... You lose", false));

        currentBustCoroutine = null;
    }

    private IEnumerator HitCoroutine()
    {
        if(!isRoundActive || isActionLocked) yield break;

        isActionLocked = true;

        if(hitHandAnimator != null) hitHandAnimator.SetTrigger("hitTrigger");

        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(DealCardToPlayerCoroutine());

        UpdateUI(true);

        if(scissorsValueReduction > 0)
        {
            CardInstance visibleDealerCard = dealerHand[1];

            int originalValue = visibleDealerCard.cardData.GetValue();
            int halvedValue = Mathf.CeilToInt((float)originalValue / 2f);
        }

        int playerValue = CalculateHandValue(playerHand, true);

        if(playerHand.Count == 7 && playerValue <= blackjackGoal)
        {
            statusText.text = "Hand full. Forced to stand...";

            standHandAnimator.SetTrigger("standTrigger");

            yield return new WaitForSeconds(2.0f);

            StartCoroutine(DealerTurnCoroutine());
        }
        else if(playerValue <= blackjackGoal)
        {
            isActionLocked = false;
        }
    }

    private IEnumerator StandCoroutine()
    {
        if(!isRoundActive || isActionLocked) yield break;

        isActionLocked = true;

        statusText.text = "You stand";

        if(standHandAnimator != null) standHandAnimator.SetTrigger("standTrigger");

        yield return new WaitForSeconds(1.5f);

        StartCoroutine(DealerTurnCoroutine());
    }

    private IEnumerator DealerTurnCoroutine(bool playerHasBlackjack = false)
    {
        statusText.text = "Dealer turn...";

        yield return new WaitForSeconds(1.0f);

        CardInstance hiddenCard = dealerHand.FirstOrDefault(x => x.isHidden);

        if(hiddenCard != null)
        {
            yield return StartCoroutine(FlipCardCoroutine(hiddenCard.displayComponent, 0.4f));

            hiddenCard.isHidden = false;

            UpdateUI(true);
            UpdateHandVisuals(dealerHand);

            yield return new WaitForSeconds(2f);
        }

        int dealerValue = CalculateHandValue(dealerHand, false);
        int playerValue = CalculateHandValue(playerHand, true);

        if(playerHasBlackjack && !IsBlackjack(dealerValue))
        {
            StartCoroutine(EndGameCoroutine("Blackjack! You win", true));

            yield break;
        }
        else if(playerHasBlackjack && IsBlackjack(dealerValue))
        {
            statusText.text = "Dealer also has Blackjack!";

            yield return new WaitForSeconds(1.5f);

            StartCoroutine(EndGameCoroutine("Both have Blackjack! Its a tie", true));

            yield break;
        }

        int dealerAIValue = CalculateHandValue(dealerHand, false);
        int playerAIValue = CalculateHandValue(playerHand, true);

        if(isKnifeActive)
        {
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            if(playerValue > blackjackGoal)
            {
                yield return new WaitForSeconds(1f);
            }
            else
            {
                int dealerDiff = Mathf.Abs(Mathf.Abs(dealerAIValue) - blackjackGoal);
                int playerDiff = Mathf.Abs(Mathf.Abs(playerAIValue) - blackjackGoal);

                while(Mathf.Abs(dealerAIValue) < (blackjackGoal - 4) && dealerDiff >= playerDiff && dealerHand.Count < 7)
                {
                    yield return StartCoroutine(DealCardToDealerCoroutine(false));

                    UpdateUI(true);

                    dealerAIValue = CalculateHandValue(dealerHand, false);
                    dealerDiff = Mathf.Abs(Mathf.Abs(dealerAIValue) - blackjackGoal);

                    yield return new WaitForSeconds(2f);
                }

                if(dealerHand.Count == 7 && dealerAIValue <= blackjackGoal)
                {
                    statusText.text = "Hand full! Forced to stand...";

                    yield return new WaitForSeconds(2.0f);
                }
            }

            if(!isKnifeActive && dealerAIValue <= blackjackGoal)
            {
                statusText.text = "Dealer stands";

                UpdateUI(true);

                yield return new WaitForSeconds(0.5f);
            }
        }

        UpdateUI(false);

        var playerJokers = playerHand.Where(c => c.cardData.rank == Card.Rank.Joker).ToList();
        var dealerJokers = dealerHand.Where(c => c.cardData.rank == Card.Rank.Joker).ToList();

        string revealMessage = "";

        if(playerJokers.Count > 0)
        {
            revealMessage += "Your Joker(s): ";
            revealMessage += string.Join(", ", playerJokers.Select(j => j.jokerValue.ToString()));
            revealMessage += ". ";
        }

        if(dealerJokers.Count > 0)
        {
            revealMessage += "Dealers Joker(s): ";
            revealMessage += string.Join(", ", dealerJokers.Select(j => j.jokerValue.ToString()));
            revealMessage += ".";
        }

        if(!string.IsNullOrEmpty(revealMessage))
        {
            statusText.text = revealMessage;

            yield return new WaitForSeconds(4f);
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
        }

        dealerValue = CalculateHandValue(dealerHand, true);
        playerValue = CalculateHandValue(playerHand, true);

        string resultMessage = DetermineWinner(playerValue, dealerValue);

        statusText.text = resultMessage;

        yield return StartCoroutine(EndGameCoroutine(resultMessage));
    }

    private string DetermineWinner(int playerValue, int dealerValue)
    {
        bool playerBust = (playerValue > blackjackGoal || playerValue < -blackjackGoal) && !IsBlackjack(playerValue);
        bool dealerBust = (dealerValue > blackjackGoal || dealerValue < -blackjackGoal) && !IsBlackjack(dealerValue);
        int playerDiff = Mathf.Abs(Mathf.Abs(playerValue) - blackjackGoal);
        int dealerDiff = Mathf.Abs(Mathf.Abs(dealerValue) - blackjackGoal);

        if(playerBust) return "Bust... You lose";

        if(dealerBust) return "Dealer busts... You win";

        if(playerDiff < dealerDiff) return "You win";

        if(dealerDiff < playerDiff) return "Dealer wins";
        
        return "Its a tie";
    }

    private IEnumerator EndGameCoroutine(string message, bool revealHand = true)
    {
        isRoundActive = false;
        cursorDetection.OnRoundInactive();

        if(message.Contains("You win"))
        {
            PlayerMoney += currentBet;

            AudioManager.instance.Play("MoneyGained");

            Instantiate(greenParticlePrefab, particleSpawnPoint.position, particleSpawnPoint.rotation);

            yield return new WaitForSeconds(3f);
        }
        else if(message.Contains("Its a tie"))
        {
            yield return new WaitForSeconds(3f);
        }
        else
        {
            dealerSmile.SetActive(true);

            PlayerMoney -= currentBet;

            AudioManager.instance.Play("MoneyLost");

            Instantiate(redParticlePrefab, particleSpawnPoint.position, particleSpawnPoint.rotation);

            yield return new WaitForSeconds(3f);

            dealerSmile.SetActive(false);
        }

        if(revealHand)
        {
            UpdateUI(false);
        }

        yield return StartCoroutine(CheckForEventTriggerCoroutine());

        if(PlayerMoney <= 0)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadSceneAsync(3);

            yield break;
        }

        if(PlayerMoney >= 100000)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadSceneAsync(2);

            yield break;
        }

        StartGame();
    }

    public void LoseAmount(int amount)
    {
        playerMoney -= amount;

        if(currentBet > playerMoney) currentBet = playerMoney;

        if(currentBet < minBet && playerMoney >= minBet) currentBet = minBet;

        UpdateBettingUI();
    }

    private bool IsBlackjack(int handValue)
    {
        if(handValue == blackjackGoal || handValue == -blackjackGoal) return true;

        return false;
    }
}
