using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class EventThreshold
{
    public BlackjackEvent.EventSeverity severityToTrigger;

    public int moneyAmount;
}

public class BlackjackGame : MonoBehaviour
{
    #region Attributes
    [Header("Set-Up")]
    [SerializeField] private ItemManager itemManager;
    [SerializeField] private CursorDetection cursorDetection;
    private Coroutine currentBustCoroutine = null;
    private Deck gameDeck;
    private int blackjackGoal = 21;
    private int scissorsValueReduction = 0;
    private bool isKnifeActive = false;
    private bool isCrucifixActive = false;
    private bool isActionLocked = false;
    [HideInInspector] public bool canDoubleDown = false;
    [HideInInspector] public bool isRoundActive = false;

    [Header("Event System")]
    [SerializeField] private List<EventThreshold> eventThresholds;
    [SerializeField] private List<BlackjackEvent> lowSeverityEvents;
    [SerializeField] private List<BlackjackEvent> mediumSeverityEvents;
    [SerializeField] private List<BlackjackEvent> highSeverityEvents;
    private List<BlackjackEvent> availableLowEvents;
    private List<BlackjackEvent> availableMediumEvents;
    private List<BlackjackEvent> availableHighEvents;
    private List<EventThreshold> triggeredThresholds = new List<EventThreshold>();
    private List<Card.Suit> negativeSuits = new List<Card.Suit>();
    public enum AceValueRule { Flexible, Always1, Always11 }
    private AceValueRule currentAceRule = AceValueRule.Flexible;
    private int targetMoneyBalance;
    private bool isDoubleLowActive = false;
    private bool isHalfHighActive = false;
    private bool isRouletteBlackjackActive = false;

    [Header("Money")]
    [Tooltip("Money the player starts with.")]
    [SerializeField] private int playerMoney = 500;
    [Tooltip("The minimum amount a bet can be.")]
    [SerializeField] private int minBet = 100;
    [Tooltip("Amount the bet increases / decreases.")]
    [SerializeField] private int betStep = 100;
    private int currentBet = 100;
    private int hand1Bet;
    private int hand2Bet;

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

    [Header("VFX")]
    [SerializeField] private Animator standHandAnimator;
    [SerializeField] private Animator hitHandAnimator;
    [SerializeField] private Animator buttonAnimator;
    [SerializeField] private GameObject greenParticlePrefab;
    [SerializeField] private GameObject redParticlePrefab;
    [SerializeField] private Transform particleSpawnPoint;
    private GameObject peekedCardObject = null;
    private const float zOverlap = 0.001f;
    private const float cardAnimationDuration = 0.25f;

    [Header("Visual Setup")]
    [SerializeField] private List<CardVisuals> cardPrefabs = new List<CardVisuals>();
    [SerializeField] private Transform playerCardPosition;
    [SerializeField] private Transform dealerCardPosition;
    [SerializeField] private Transform sunglassesCardPosition;
    [SerializeField] private Transform deckPosition;
    [Tooltip("Offsets the player cards to create the staircase layout.")]
    [SerializeField] private Vector2 playerCardOffset = new Vector2(10f, -10f);
    [Tooltip("Space between the dealers cards.")]
    [SerializeField] private float dealerCardHorizontalSpacing = 35f;
    private Dictionary<(Card.Rank, Card.Suit), GameObject> cardPrefabLookup;
    private readonly Vector3 cardScaleVector = Vector3.one * 0.05f;
    private List<CardInstance> playerHand = new List<CardInstance>();
    private List<CardInstance> dealerHand = new List<CardInstance>();
    private List<GameObject> activeCardObjects = new List<GameObject>();

    [Header("Split")]
    [SerializeField] private Transform splitHandPosition;
    [SerializeField] private TMPro.TextMeshProUGUI splitTotalText;
    private List<CardInstance> splitHand = new List<CardInstance>();
    private bool isPlayingSplitHand = false;
    private bool isSplitting = false;

    //Delete when you get rid of keyboard controls.
    private float nextKeyBetTime = 0f;
    private float keyRepeatDelay = 0.5f;
    private float keyRepeatRate = 0.1f;
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
        //TODO: Delete keyboard binds after adding click-only gameplay.
        if(currentBustCoroutine != null || isActionLocked) return;

        if(!isRoundActive)
        {
            //Can delete
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

            //Keep this
            if(Input.mouseScrollDelta.y > 0f)
            {
                IncreaseBet();
            }
            else if(Input.mouseScrollDelta.y < 0f)
            {
                DecreaseBet();
            }

            //Can delete
            bool canDeal = PlayerMoney >= currentBet;

            if(Input.GetKeyDown(KeyCode.H) && canDeal) StartCoroutine(DealRoundCoroutine());
        }
        else //Handle playing actions. //Can delete
        {
            if(Input.GetKeyDown(KeyCode.H)) StartCoroutine(HitCoroutine());

            if(Input.GetKeyDown(KeyCode.S)) StartCoroutine(StandCoroutine());

            if(Input.GetKeyDown(KeyCode.D) && canDoubleDown)
            {
                StartCoroutine(DoubleDownCoroutine());
            }

            if(Input.GetKeyDown(KeyCode.P) && CanSplit())
            {
                StartCoroutine(SplitCoroutine());
            }
        }
    }
    #endregion
    
    #region Player Actions
    // These methods will be added as unity events in the Clickable components
    public void OnStartGame()
    {
        if(!isRoundActive && PlayerMoney >= currentBet)
            StartCoroutine(DealRoundCoroutine());
    }
        
    public void OnHit() => StartCoroutine(HitCoroutine());
        
    public void OnStand() => StartCoroutine(StandCoroutine());

    public void OnIncreaseBet() => IncreaseBet();
        
    public void OnDecreaseBet() => DecreaseBet();

    public void OnDoubleDown()
    {
        if(canDoubleDown) StartCoroutine(DoubleDownCoroutine());
    }

    public void OnSplit()
    {
        if(CanSplit()) StartCoroutine(SplitCoroutine());
    }

    public void ResetItemsAvailable()
    {
        IsScissorsAvailable = true;
        IsCrucifixAvailable = true;
        IsSunglassesAvailable = true;
    }
    #endregion

    #region Betting Methods
    public int PlayerMoney
    {
        get { return playerMoney; }
        private set { playerMoney = value; }
    }

    public void UpdateBettingUI()
    {
        if(currentBet > playerMoney)
        {
            currentBet = playerMoney;
        }

        moneyText.text = $"${PlayerMoney}";
        betText.text = $"${currentBet}";
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

    //Animates the change in player's money when winning or losing.
    private IEnumerator AnimateBetChange(int targetAmount, float duration)
    {
        float elapsedTime = 0;
        int startingAmount = PlayerMoney;

        AudioManager.instance.Play("MoneyCounter");

        while(elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float percent = elapsedTime / duration;
            float smoothedPercent = percent * percent * (3f - 2f * percent);

            playerMoney = Mathf.RoundToInt(Mathf.Lerp(startingAmount, targetAmount, smoothedPercent));
            moneyText.text = $"${PlayerMoney}";

            yield return null;
        }

        playerMoney = targetAmount;

        UpdateBettingUI();
    }
    #endregion

    #region Camera Methods
    private void EnableCamera(CinemachineCamera camera)
    {
        camera.Priority = 10;
    }

    private void DisableCamera(CinemachineCamera camera)
    {
        camera.Priority = 0;
    }
    #endregion

    #region Item Methods
    //Decreases the player's money by the amount and updates the bet if necessary.
    public void BuyItem(int amount)
    {
        playerMoney -= amount;

        if(currentBet > playerMoney) currentBet = playerMoney;

        if(currentBet < minBet && playerMoney >= minBet) currentBet = minBet;

        UpdateBettingUI();
    }

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

        CardInstance visibleDealerCard = dealerHand[0];

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

            if(negativeSuits.Contains(visibleDealerCard.cardData.suit))
            {
                originalValue *= -1;
            }
        }

        int halvedValue = Mathf.CeilToInt((float)Mathf.Abs(originalValue) / 2f);

        scissorsValueReduction = Mathf.Abs(originalValue) - halvedValue;
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

    public bool IsKnifeAvailable { get; private set; } = true;

    public bool IsScissorsAvailable { get; private set; } = true;

    public bool IsCrucifixAvailable { get; private set; } = true;

    public bool IsSunglassesAvailable { get; private set; } = true;
    #endregion

    #region Event Methods
    private IEnumerator CheckForEventTriggerCoroutine()
    {
        bool eventTriggered = false;
        bool introPlayed = false;

        while(true)
        {
            EventThreshold thresholdToTrigger = null;

            foreach(EventThreshold threshold in eventThresholds)
            {
                if(targetMoneyBalance >= threshold.moneyAmount && !triggeredThresholds.Contains(threshold))
                {
                    thresholdToTrigger = threshold;

                    break;
                }
            }

            if(thresholdToTrigger == null) break;

            if(!introPlayed)
            {
                DisableCamera(playingCamera);
                EnableCamera(eventCamera);

                statusText.text = "Lets make it more interesting";

                AudioManager.instance.Play("Laugh");

                yield return new WaitForSeconds(5.0f);

                introPlayed = true;
                eventTriggered = true;
            }

            eventTriggered = true;

            triggeredThresholds.Add(thresholdToTrigger);

            List<BlackjackEvent> eventPool = null;

            switch(thresholdToTrigger.severityToTrigger)
            {
                case BlackjackEvent.EventSeverity.Low: eventPool = availableLowEvents; break;
                case BlackjackEvent.EventSeverity.Medium: eventPool = availableMediumEvents; break;
                case BlackjackEvent.EventSeverity.High: eventPool = availableHighEvents; break;
            }

            if(eventPool != null && eventPool.Count > 0)
            {
                int randomIndex = Random.Range(0, eventPool.Count);

                BlackjackEvent chosenEvent = eventPool[randomIndex];

                chosenEvent.Apply(this);
                eventPool.RemoveAt(randomIndex);

                AudioManager.instance.Play("NewEvent");

                statusText.text = $"New Event: {chosenEvent.eventName}";

                yield return new WaitForSeconds(6.0f);
            }
        }

        if(eventTriggered)
        {
            DisableCamera(eventCamera);
            EnableCamera(sittingCamera);
        }
    }

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

    #region Game Flow
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
        splitHand.Clear();

        if(peekedCardObject != null)
        {
            Destroy(peekedCardObject);

            peekedCardObject = null;
        }
    }

    private void StartGame()
    {
        buttonAnimator.SetBool("StartActive", true);

        ClearTable();
        DisableCamera(playingCamera);
        EnableCamera(sittingCamera);

        if(isRouletteBlackjackActive)
        {
            RandomizeBlackjackGoal();
        }

        AudioManager.instance.Play("Shuffle");

        gameDeck.InitializeDeck();
        gameDeck.Shuffle();
        statusText.text = "Place your bet...";
        cursorDetection.OnRoundInactive();
        itemManager.SpawnPowerUps();
        isRoundActive = false;
        isActionLocked = false;
        canDoubleDown = false;
        isSplitting = false;
        isPlayingSplitHand = false;
        isKnifeActive = false;
        IsKnifeAvailable = true;
        IsScissorsAvailable = true;
        scissorsValueReduction = 0;
        IsCrucifixAvailable = true;
        isCrucifixActive = false;
        IsSunglassesAvailable = true;
        playerTotalText.text = "";
        dealerTotalText.text = "";
        splitTotalText.text = "";

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

        hand1Bet = currentBet;
        buttonAnimator.SetBool("StartActive", false);

        if(isRouletteBlackjackActive)
        {
            DisableCamera(sittingCamera);
            EnableCamera(eventCamera);

            statusText.text = $"New Blackjack goal: {blackjackGoal}";

            yield return new WaitForSeconds(3.0f);

            DisableCamera(eventCamera);
            EnableCamera(playingCamera);
        }

        DisableCamera(sittingCamera);
        EnableCamera(playingCamera);

        statusText.text = "Dealing cards...";
        isActionLocked = true;
        isRoundActive = true;
        cursorDetection.OnRoundActive();
        itemManager.DespawnPowerUps();

        yield return StartCoroutine(DealCardToPlayerCoroutine());
        yield return StartCoroutine(DealCardToDealerCoroutine(true));
        yield return StartCoroutine(DealCardToPlayerCoroutine());
        yield return StartCoroutine(DealCardToDealerCoroutine(false));

        UpdateUI();

        if(IsBlackjack(CalculateHandValue(playerHand, true)))
        {
            canDoubleDown = false;
            statusText.text = "Blackjack!";

            yield return new WaitForSeconds(2.0f);

            StartCoroutine(DealerTurnCoroutine(true));
        }
        else
        {
            statusText.text = "";
            isActionLocked = false;
            canDoubleDown = playerMoney > currentBet;
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

        List<CardInstance> currentHand = isPlayingSplitHand ? splitHand : playerHand;
        Transform currentParent = isPlayingSplitHand ? splitHandPosition : playerCardPosition;
        CardInstance newCardInstance = DealCardInstance(newCardData, currentHand, currentParent, false);
        AudioManager.instance.Play("CardHit");

        if(newCardInstance != null)
        {
            UpdateHandVisuals(currentHand);

            Vector3 targetLocalPos = newCardInstance.displayComponent.transform.localPosition;
            Quaternion targetRotation = newCardInstance.displayComponent.transform.localRotation;

            newCardInstance.displayComponent.transform.SetParent(currentParent.parent);

            yield return StartCoroutine(CardAnimationCoroutine(
                newCardInstance.displayComponent.transform,
                currentParent.TransformPoint(targetLocalPos),
                currentParent.rotation * targetRotation,
                cardScaleVector,
                cardAnimationDuration
            ));

            newCardInstance.displayComponent.transform.SetParent(currentParent);
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

    private IEnumerator HitCoroutine()
    {
        if(!isRoundActive || isActionLocked) yield break;

        isActionLocked = true;
        canDoubleDown = false;
        hitHandAnimator.SetTrigger("hitTrigger");

        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(DealCardToPlayerCoroutine());

        UpdateUI(true);

        List<CardInstance> activeHand = isPlayingSplitHand ? splitHand : playerHand;

        int handValue = CalculateHandValue(activeHand, true);

        if(activeHand.Count == 7 && handValue <= blackjackGoal)
        {
            statusText.text = "Hand full.";

            yield return new WaitForSeconds(2.0f);

            isActionLocked = false;

            StartCoroutine(StandCoroutine());
        }
        else if(handValue > blackjackGoal || handValue < -blackjackGoal)
        {
            if(isSplitting && !isPlayingSplitHand)
            {
                statusText.text = "First hand bust! Playing next...";

                yield return new WaitForSeconds(1.0f);

                isPlayingSplitHand = true;
                isActionLocked = false;

                UpdateUI(true);
            }
            else
            {
                StartCoroutine(BustCheckCoroutine());
            }
        }
        else
        {
            isActionLocked = false;
        }
    }

    private IEnumerator StandCoroutine()
    {
        if(!isRoundActive || isActionLocked) yield break;

        isActionLocked = true;

        if(isSplitting && !isPlayingSplitHand)
        {
            statusText.text = "You stand";
            standHandAnimator.SetTrigger("standTrigger");

            yield return new WaitForSeconds(1.5f);

            statusText.text = "Playing next hand...";
            
            yield return new WaitForSeconds(2.0f);

            statusText.text = "";
            isPlayingSplitHand = true;
            isActionLocked = false;
            canDoubleDown = playerMoney > currentBet;

            UpdateUI(true);
        }
        else
        {
            statusText.text = "You stand";
            standHandAnimator.SetTrigger("standTrigger");

            yield return new WaitForSeconds(1.5f);

            StartCoroutine(DealerTurnCoroutine());
        }
    }

    private IEnumerator DoubleDownCoroutine()
    {
        if(!isRoundActive || isActionLocked || !canDoubleDown) yield break;

        isActionLocked = true;
        canDoubleDown = false;

        int originalHandBet = isPlayingSplitHand ? hand2Bet : hand1Bet;
        int additionalBet = Mathf.Min(originalHandBet, playerMoney);

        if(isSplitting && isPlayingSplitHand)
        {
            hand2Bet += additionalBet;
        }
        else
        {
            hand1Bet += additionalBet;
        }

        currentBet = isSplitting ? (hand1Bet + hand2Bet) : hand1Bet;

        UpdateBettingUI();

        statusText.text = "You Double Down...";

        AudioManager.instance.Play("BetUp");

        hitHandAnimator.SetTrigger("hitTrigger"); //double down animation

        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(DealCardToPlayerCoroutine());

        UpdateUI(true);

        int playerValue = CalculateHandValue(playerHand, true);

        if(isSplitting && !isPlayingSplitHand)
        {
            statusText.text = "Playing next hand...";

            yield return new WaitForSeconds(2.0f);

            statusText.text = "";
            isPlayingSplitHand = true;
            isActionLocked = false;
            canDoubleDown = playerMoney > currentBet;

            UpdateUI(true);
        }
        else
        {
            if(playerValue <= blackjackGoal && playerValue >= -blackjackGoal)
            {
                yield return new WaitForSeconds(1.0f);
            }

            StartCoroutine(DealerTurnCoroutine());
        }
    }

    private IEnumerator SplitCoroutine()
    {
        isActionLocked = true;
        isSplitting = true;
        isPlayingSplitHand = false;
        hand1Bet = currentBet;
        hand2Bet = currentBet;
        currentBet = hand1Bet + hand2Bet;

        UpdateBettingUI();

        AudioManager.instance.Play("BetUp");

        statusText.text = "Splitting Hand...";
        standHandAnimator.SetTrigger("standTrigger"); //split animation

        yield return new WaitForSeconds(2.0f);

        statusText.text = "";

        CardInstance cardToMove = playerHand[0];

        playerHand.RemoveAt(0);
        splitHand.Add(cardToMove);

        AudioManager.instance.Play("CardHit");

        yield return StartCoroutine(CardAnimationCoroutine(
            cardToMove.displayComponent.transform,
            splitHandPosition.position,
            splitHandPosition.rotation,
            cardScaleVector,
            cardAnimationDuration
        ));

        cardToMove.displayComponent.transform.SetParent(splitHandPosition);
        cardToMove.displayComponent.transform.localPosition = Vector3.zero;

        UpdateHandVisuals(playerHand);
        UpdateHandVisuals(splitHand);

        yield return new WaitForSeconds(0.5f);

        isActionLocked = false;

        UpdateUI();
    }

    private IEnumerator DealerTurnCoroutine(bool playerHasBlackjack = false)
    {
        statusText.text = "Dealer turn...";

        yield return new WaitForSeconds(1.0f);

        int playerValue = CalculateHandValue(playerHand, true);
        int splitValue = isSplitting ? CalculateHandValue(splitHand, true) : -999;
        bool playerBust = (playerValue > blackjackGoal || playerValue < -blackjackGoal);
        bool splitBust = isSplitting && (splitValue > blackjackGoal || splitValue < -blackjackGoal);
        bool allHandsBust = isSplitting ? (playerBust && splitBust) : playerBust;

        if(allHandsBust && !isKnifeActive)
        {
            statusText.text = "Bust";

            yield return new WaitForSeconds(1.0f);
        }
        else
        {
            CardInstance hiddenCard = dealerHand.FirstOrDefault(x => x.isHidden);

            if(hiddenCard != null)
            {
                yield return StartCoroutine(FlipCardCoroutine(hiddenCard.displayComponent, 0.4f));

                hiddenCard.isHidden = false;

                UpdateUI(true);

                yield return new WaitForSeconds(1.5f);
            }

            int dealerValueInit = CalculateHandValue(dealerHand, false);

            if(playerHasBlackjack)
            {
                if(!IsBlackjack(dealerValueInit))
                {
                    StartCoroutine(EndGameCoroutine("Blackjack! You win", true));

                    yield break;
                }
                else
                {
                    statusText.text = "Dealer also has Blackjack";

                    yield return new WaitForSeconds(1.5f);

                    StartCoroutine(EndGameCoroutine("Both have Blackjack. Its a tie", true));

                    yield break;
                }
            }

            if(!isKnifeActive)
            {
                int dealerAIValue = CalculateHandValue(dealerHand, false);

                while(Mathf.Abs(dealerAIValue) < (blackjackGoal - 4) && dealerHand.Count < 7)
                {
                    yield return StartCoroutine(DealCardToDealerCoroutine(false));

                    UpdateUI(true);

                    dealerAIValue = CalculateHandValue(dealerHand, false);

                    yield return new WaitForSeconds(1.5f);
                }

                if(dealerHand.Count == 7)
                {
                    statusText.text = "Dealer hand full";

                    yield return new WaitForSeconds(1.0f);
                }
                else
                {
                    statusText.text = "Dealer stands";
                }
            }
        }

        UpdateUI(false);

        yield return StartCoroutine(RevealJokers());

        int finalDealerValue = CalculateHandValue(dealerHand, true);
        int finalPlayerValue = CalculateHandValue(playerHand, true);

        if(isSplitting)
        {
            yield return StartCoroutine(HandleSplitBet(finalPlayerValue, splitValue, finalDealerValue));
        }
        else
        {
            string resultMessage = DetermineWinner(finalPlayerValue, finalDealerValue);

            yield return StartCoroutine(EndGameCoroutine(resultMessage));
        }
    }

    private IEnumerator HandleSplitBet(int val1, int val2, int dealerVal)
    {
        string res1 = DetermineWinner(val1, dealerVal);

        yield return StartCoroutine(ProcessPayout(res1, hand1Bet));

        string res2 = DetermineWinner(val2, dealerVal);

        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(ProcessPayout(res2, hand2Bet));
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(EndRoundSequence());
    }

    private IEnumerator ProcessPayout(string message, int betAmount)
    {
        statusText.text = message;

        if(message.Contains("You win"))
        {
            targetMoneyBalance = playerMoney + betAmount;

            AudioManager.instance.Play("MoneyGained");

            Instantiate(greenParticlePrefab, particleSpawnPoint.position, particleSpawnPoint.rotation);

            yield return StartCoroutine(AnimateBetChange(targetMoneyBalance, 3f));
        }
        else if(message.Contains("Dealer wins") || message.Contains("Bust"))
        {
            targetMoneyBalance = playerMoney - betAmount;

            AudioManager.instance.Play("MoneyLost");

            Instantiate(redParticlePrefab, particleSpawnPoint.position, particleSpawnPoint.rotation);

            yield return StartCoroutine(AnimateBetChange(targetMoneyBalance, 3f));
        }
        else
        {
            targetMoneyBalance = playerMoney;

            yield return new WaitForSeconds(2.0f);
        }
    }

    private IEnumerator BustCheckCoroutine()
    {
        yield return new WaitForSeconds(2f);

        UpdateUI(true);

        if(isSplitting)
        {
            statusText.text = "Hand 2 Bust";

            yield return new WaitForSeconds(1.5f);

            StartCoroutine(DealerTurnCoroutine());

            currentBustCoroutine = null;

            yield break;
        }

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
        if(revealHand) UpdateUI(false);

        yield return StartCoroutine(ProcessPayout(message, currentBet));
        yield return StartCoroutine(EndRoundSequence());
    }
    #endregion

    #region Card Visuals
    //The dealer hand is in a straight line, the player hand creates a staircase effect.
    private void UpdateHandVisuals(List<CardInstance> hand)
    {
        int cardCount = hand.Count;

        if(cardCount == 0) return;

        bool isPlayerHand = hand == playerHand || hand == splitHand;

        for(int i = 0; i < cardCount; i++)
        {
            int cardOrderIndex = cardCount - 1 - i;

            CardInstance cardInstance = hand[i];

            float xOffset, yOffset;

            if(isPlayerHand)
            {
                xOffset = cardOrderIndex * playerCardOffset.x;
                yOffset = cardOrderIndex * playerCardOffset.y;
            }
            else
            {
                xOffset = cardOrderIndex * dealerCardHorizontalSpacing;
                yOffset = 0f;
            }

            float zOffset = cardOrderIndex * -zOverlap;

            Vector3 targetLocalPos = new Vector3(xOffset, yOffset, zOffset);

            cardInstance.displayComponent.transform.localPosition = targetLocalPos;
            cardInstance.displayComponent.transform.localRotation = Quaternion.identity;
        }
    }

    //Animates a card moving from the deck to its position in the hand.
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
    #endregion

    //Calculates the total value of a hand. Aces are 1 or 11.
    private int CalculateHandValue(List<CardInstance> hand, bool countJoker)
    {
        float value = 0f;
        int aceCount = 0;

        CardInstance targetedCardInstance = null;

        if(scissorsValueReduction > 0 && dealerHand.Count > 1)
        {
            targetedCardInstance = dealerHand[0];
        }

        for(int i = 0; i < hand.Count; i++)
        {
            CardInstance cardInstance = hand[i];

            Card card = cardInstance.cardData;

            float cardValue = card.GetValue();

            if(card.rank == Card.Rank.Joker)
            {
                if(countJoker)
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
                if(cardValue > 0)
                {
                    cardValue -= scissorsValueReduction;
                }
                else if(cardValue < 0)
                {
                    cardValue += scissorsValueReduction;
                }
            }

            value += cardValue;
        }

        //adjust aces
        if(currentAceRule == AceValueRule.Flexible)
        {
            while((value > blackjackGoal || value < -blackjackGoal) && aceCount > 0)
            {
                value += (value > 0) ? -10 : 10;
                aceCount--;
            }
        }

        return Mathf.RoundToInt(value);
    }

    private IEnumerator RevealJokers()
    {
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
    }

    //Updates the score, money, and checks for busts.
    private void UpdateUI(bool dealerHidden = true)
    {
        int playerValue = CalculateHandValue(playerHand, true);
        bool revealJokers = !dealerHidden;

        playerTotalText.text = FormatHandText("Your hand: ", playerHand, revealJokers, false);

        if(isSplitting)
        {
            int splitValue = CalculateHandValue(splitHand, true);

            splitTotalText.text = "Split hand: " + splitValue.ToString();
        }
        else
        {
            splitTotalText.text = "";
        }

        if(dealerHand.Count > 0)
        {
            if(dealerHidden && dealerHand.Any(c => c.isHidden))
            {
                List<CardInstance> visibleCards = dealerHand.Where(x => !x.isHidden).ToList();

                dealerTotalText.text = FormatHandText("Dealer hand: ", visibleCards, revealJokers, true);
            }
            else
            {
                dealerTotalText.text = FormatHandText("Dealer hand: ", dealerHand, revealJokers, false);
            }
        }
        else
        {
            dealerTotalText.text = "";
        }

        UpdateBettingUI();

        if((playerValue > blackjackGoal || playerValue < -blackjackGoal) && isRoundActive && currentBustCoroutine == null)
        {
            if(isPlayingSplitHand && !isSplitting)
            {
                currentBustCoroutine = StartCoroutine(BustCheckCoroutine());
            }
        }
    }

    private string FormatHandText(string prefix, List<CardInstance> cards, bool revealJokers, bool dealerHasHiddenCard)
    {
        if(cards.Count == 0) return "";

        int totalValue = CalculateHandValue(cards, true);
        bool hasJoker = cards.Any(c => c.cardData.rank == Card.Rank.Joker);

        if(revealJokers || !hasJoker)
        {
            return prefix + (dealerHasHiddenCard ? $"{totalValue} + ?" : totalValue.ToString());
        }

        int baseValue = CalculateHandValue(cards, false);

        return prefix + (dealerHasHiddenCard ? $"{baseValue} + ? + ?" : $"{baseValue} + ?");
    }

    private IEnumerator EndRoundSequence()
    {
        isRoundActive = false;
        cursorDetection.OnRoundInactive();

        yield return StartCoroutine(CheckForEventTriggerCoroutine());

        if(PlayerMoney <= 0)
        {
            SceneManager.LoadSceneAsync(3);

            yield break;
        }

        if(PlayerMoney >= 100000)
        {
            SceneManager.LoadSceneAsync(2);

            yield break;
        }

        isSplitting = false;
        isPlayingSplitHand = false;

        StartGame();
    }

    private bool IsBlackjack(int handValue)
    {
        if(handValue == blackjackGoal || handValue == -blackjackGoal) return true;

        return false;
    }

    public bool CanSplit()
    {
        if(isSplitting || !isRoundActive || isActionLocked || playerHand.Count != 2) return false;

        float[] finalValues = new float[2];

        for(int i = 0; i < 2; i++)
        {
            Card card = playerHand[i].cardData;

            float cardValue = card.GetValue();

            if(card.rank >= Card.Rank.Ten && card.rank <= Card.Rank.King)
            {
                cardValue = 10;
            }
            else if(card.rank == Card.Rank.Ace)
            {
                cardValue = 11;
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

            finalValues[i] = cardValue;
        }

        bool hasMatchingValues = Mathf.RoundToInt(finalValues[0]) == Mathf.RoundToInt(finalValues[1]);
        bool hasEnoughMoney = playerMoney >= (currentBet * 2);

        return hasMatchingValues && hasEnoughMoney;
    }
}
