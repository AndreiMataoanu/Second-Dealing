using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;



public class BlackjackGame : MonoBehaviour
{
    #region Attributes
    [SerializeField] private GameObject powerUpManager;
    
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
    private bool dealerWinsTies = false;

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

    [Header("UI")]
    [SerializeField] private TMPro.TextMeshProUGUI moneyText;
    [SerializeField] private TMPro.TextMeshProUGUI betText;
    [SerializeField] private TMPro.TextMeshProUGUI statusText;
    [SerializeField] private TMPro.TextMeshProUGUI playerTotalText;
    [SerializeField] private TMPro.TextMeshProUGUI dealerTotalText;

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
    private bool isPrayerBeadsActive = false;
    private GameObject peekedCardObject = null;
    public bool IsKnifeAvailable { get; private set; } = true;
    public bool IsScissorsAvailable { get; private set; } = true;
    public bool IsPrayerBeadsAvailable { get; private set; } = true;
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
    private const float zOverlap = 0.01f;
    private const float cardAnimationDuration = 0.25f;

    private readonly Vector3 cardScaleVector = Vector3.one * 0.05f;
    private PowerUpShop powerUpShop;
    #endregion

    #region Monobehaviour Methods
    private void Awake()
    {
        powerUpShop = powerUpManager.GetComponent<PowerUpShop>();
    }

    private void Start()
    {
        gameDeck = new Deck();
        availableLowEvents = new List<BlackjackEvent>(lowSeverityEvents);
        availableMediumEvents = new List<BlackjackEvent>(mediumSeverityEvents);
        availableHighEvents = new List<BlackjackEvent>(highSeverityEvents);

        InitializeCardLookup();
        StartGame();

        AudioManager.instance.Play("MainTheme");
    }

    private void Update()
    {
        if(currentBustCoroutine != null || isActionLocked) return;

        if(!isRoundActive)
        {
            if(Input.GetKeyDown(KeyCode.UpArrow)) IncreaseBet();

            if(Input.GetKeyDown(KeyCode.DownArrow)) DecreaseBet();

            bool canDeal = PlayerMoney >= currentBet;

            if(Input.GetKeyDown(KeyCode.Space) && canDeal) StartCoroutine(DealRoundCoroutine());
        }
        else //Handle playing actions.
        {
            if(Input.GetKeyDown(KeyCode.H)) StartCoroutine(HitCoroutine());

            if(Input.GetKeyDown(KeyCode.S)) StartCoroutine(StandCoroutine());
        }
    }
    #endregion

    private void ClearTable()
    {
        foreach(GameObject cardObject in activeCardObjects)
        {
            if(cardObject != null) Destroy(cardObject);
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

    public void IncreaseBet()
    {
        if(isRoundActive) return;

        if(currentBet < PlayerMoney)
        {
            AudioManager.instance.Play("BetUp");

            int nextBet = currentBet + betStep;

            if(nextBet > PlayerMoney) currentBet = PlayerMoney;
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

        if(currentBet < minBet) currentBet = minBet;

        UpdateBettingUI();
    }

    #region Ability Methods
    public void ActivateKnife()
    {
        if(!isRoundActive || isKnifeActive || !IsKnifeAvailable) return;

        isKnifeActive = true;
        IsKnifeAvailable = false;
    }

    public void ActivateScissors()
    {
        if(!isRoundActive || !IsScissorsAvailable) return;

        if(CalculateHandValue(playerHand) > 21) return;

        if(dealerHand.Count < 2) return;

        CardInstance visibleDealerCard = dealerHand[1];

        int originalValue = visibleDealerCard.cardData.GetValue();
        int halvedValue = Mathf.CeilToInt((float)originalValue / 2f);

        scissorsValueReduction = originalValue - halvedValue;

        IsScissorsAvailable = false;

        UpdateUI(true);
    }

    public void ActivatePrayerBeads()
    {
        if(!isRoundActive || isPrayerBeadsActive || !IsPrayerBeadsAvailable) return;

        if(CalculateHandValue(playerHand) > 21) return;

        isPrayerBeadsActive = true;
        IsPrayerBeadsAvailable = false;
    }

    public void ActivateSunglasses()
    {
        if(!isRoundActive || !IsSunglassesAvailable || peekedCardObject != null) return;

        if(CalculateHandValue(playerHand) > 21) return;

        Card? nextCard = gameDeck.PeekCard();

        if(!nextCard.HasValue) return;

        Card newCardData = nextCard.Value;

        if(!cardPrefabLookup.TryGetValue((newCardData.rank, newCardData.suit), out GameObject cardPrefabToUse)) return;

        peekedCardObject = Instantiate(cardPrefabToUse, deckPosition);

        peekedCardObject.transform.localScale = cardScaleVector;

        StartCoroutine(CardAnimationCoroutine(
            peekedCardObject.transform,
            sunglassesCardPosition.position,
            Quaternion.identity,
            cardScaleVector,
            cardAnimationDuration
        ));

        CardDisplay cardDisplay = peekedCardObject.GetComponent<CardDisplay>();

        if(cardDisplay != null) cardDisplay.SetHidden(false);

        activeCardObjects.Add(peekedCardObject);

        IsSunglassesAvailable = false;
    }
    #endregion

    #region Event Methods
    public void IncreaseMinimumBet(int amount)
    {
        minBet += amount;

        if(currentBet < minBet) currentBet = minBet;

        UpdateBettingUI();
    }

    public void RemoveRankFromDeck(Card.Rank rank)
    {
        gameDeck.AddRemovedRank(rank);
    }

    public void RemoveSuitFromDeck(Card.Suit suit)
    {
        gameDeck.AddRemovedSuit(suit);
    }

    public void SetAceRule(AceValueRule newRule)
    {
        currentAceRule = newRule;
    }

    public void SetDealerWinsTies(bool newRule)
    {
        dealerWinsTies = newRule;
    }
    #endregion

    //Calculates the total value of a hand. Aces are 1 or 11.
    private int CalculateHandValue(List<CardInstance> hand)
    {
        int value = 0;
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

            int cardValue = card.GetValue();

            if(card.rank == Card.Rank.Ace)
            {
                aceCount++;

                if(currentAceRule == AceValueRule.Always1) cardValue = 1;
                else
                {
                    cardValue = 11;
                }
            }
            else if(card.rank >= Card.Rank.Ten && card.rank <= Card.Rank.King) cardValue = 10;
            else
            {
                cardValue = (int)card.rank;
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
            while(value > 21 && aceCount > 0)
            {
                value -= 10;
                aceCount--;
            }
        }

        return value;
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
        ClearTable();

        AudioManager.instance.Play("Shuffle");

        gameDeck.Shuffle();

        statusText.text = "Place your bet...";

        isRoundActive = false;
        isActionLocked = false;

        //Reset abilities
        isKnifeActive = false;
        IsKnifeAvailable = true;
        IsScissorsAvailable = true;
        scissorsValueReduction = 0;
        IsPrayerBeadsAvailable = true;
        isPrayerBeadsActive = false;
        IsSunglassesAvailable = true;

        playerTotalText.text = "";
        dealerTotalText.text = "";

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

        statusText.text = "Dealing cards...";

        isActionLocked = true;

        if(powerUpShop.hasSelected) powerUpShop.DestroyPowerUps();

        isRoundActive = true;

        yield return StartCoroutine(DealCardToPlayerCoroutine());
        yield return StartCoroutine(DealCardToDealerCoroutine(false));
        yield return StartCoroutine(DealCardToPlayerCoroutine());
        yield return StartCoroutine(DealCardToDealerCoroutine(true));

        UpdateUI();

        statusText.text = "";

        yield return new WaitForSeconds(1.0f);

        if(CalculateHandValue(playerHand) == 21)
        {
            statusText.text = "Blackjack!";

            yield return new WaitForSeconds(2.0f);

            StartCoroutine(DealerTurnCoroutine(true));
        }
        else
        {
            isActionLocked = false;
        }

        if(CalculateHandValue(playerHand) < 21) isActionLocked = false;
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
                int randomIndex = Random.Range(0, eventPool.Count);

                BlackjackEvent chosenEvent = eventPool[randomIndex];

                chosenEvent.Apply(this);
                eventPool.RemoveAt(randomIndex);

                statusText.text = $"New Event: {chosenEvent.eventName}";

                yield return new WaitForSeconds(3.0f);
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
        if(peekedCardObject != null)
        {
            activeCardObjects.Remove(peekedCardObject);

            Destroy(peekedCardObject);

            peekedCardObject = null;
        }

        Card newCardData;

        if(isPrayerBeadsActive)
        {
            isPrayerBeadsActive = false;

            int playerValue = CalculateHandValue(playerHand);
            int idealValue = 21 - playerValue;
            int searchMaxValue = Mathf.Min(idealValue, 10); //Max value it can find is 10

            Card? dealtCard = null;

            //Searches for the highest possible card that doesn't bust the player
            for(int value = searchMaxValue; value >= 2; value--)
            {
                if(value == 11 || value == 1) continue;

                if(value >= 10)
                {
                    Card.Rank[] faceRanks = { Card.Rank.Ten, Card.Rank.Jack, Card.Rank.Queen, Card.Rank.King };

                    foreach(var faceRank in faceRanks)
                    {
                        dealtCard = gameDeck.DealSpecificCard(faceRank);

                        if(dealtCard.HasValue) break;
                    }
                }
                else
                {
                    Card.Rank rankToSearch = (Card.Rank)value;

                    dealtCard = gameDeck.DealSpecificCard(rankToSearch);
                }

                if(dealtCard.HasValue) break; //Found a benificial card
            }

            //If no suitable card was found, try to get an Ace
            if(!dealtCard.HasValue) dealtCard = gameDeck.DealSpecificCard(Card.Rank.Ace);

            if(dealtCard.HasValue) newCardData = dealtCard.Value;
            else
            {
                newCardData = gameDeck.DealCard();
            }
        }
        else //Normal hit, deal a random card
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
        int playerValue = CalculateHandValue(playerHand);

        playerTotalText.text = playerValue > 0 ? playerValue.ToString() : "";

        if(dealerHand.Count > 0)
        {
            if(dealerHidden && dealerHand.Count > 1)
            {
                int dealerVisibleValue = CalculateHandValue(dealerHand.Where(x => !x.isHidden).ToList());

                dealerTotalText.text = $"{dealerVisibleValue} + ?";
            }
            else
            {
                int dealerFullValue = CalculateHandValue(dealerHand);

                dealerTotalText.text = dealerFullValue.ToString();
            }
        }
        else
        {
            dealerTotalText.text = "";
        }

        UpdateBettingUI();

        if(playerValue > 21 && isRoundActive && currentBustCoroutine == null)
        {
            currentBustCoroutine = StartCoroutine(BustCheckCoroutine());
        }
    }

    private IEnumerator BustCheckCoroutine()
    {
        statusText.text = "Bust! You lose.";

        yield return new WaitForSeconds(2f);

        StartCoroutine(EndGameCoroutine("Bust! You lose."));

        currentBustCoroutine = null;
    }

    private IEnumerator HitCoroutine()
    {
        if(!isRoundActive || isActionLocked) yield break;

        isActionLocked = true;

        if(hitHandAnimator != null) hitHandAnimator.SetTrigger("hitTrigger");

        yield return new WaitForSeconds(1f);

        StartCoroutine(DealCardToPlayerCoroutine());

        if(scissorsValueReduction > 0)
        {
            CardInstance visibleDealerCard = dealerHand[1];

            int originalValue = visibleDealerCard.cardData.GetValue();
            int halvedValue = Mathf.CeilToInt((float)originalValue / 2f);
        }

        int playerValue = CalculateHandValue(playerHand);

        if(playerHand.Count == 7 && playerValue <= 21)
        {
            statusText.text = "Lucky 7! You win!";

            yield return new WaitForSeconds(2.0f);
            yield return StartCoroutine(EndGameCoroutine("Lucky 7! You win!"));
        }
        else if(playerValue <= 21) isActionLocked = false;
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
        statusText.text = "Dealer's turn...";

        yield return new WaitForSeconds(1.0f);

        CardInstance hiddenCard = dealerHand.FirstOrDefault(x => x.isHidden);

        if(hiddenCard != null)
        {
            yield return StartCoroutine(FlipCardCoroutine(hiddenCard.displayComponent, 0.4f));

            hiddenCard.isHidden = false;

            UpdateUI(false);
            UpdateHandVisuals(dealerHand);

            yield return new WaitForSeconds(2f);
        }

        int dealerValue = CalculateHandValue(dealerHand);
        int playerValue = CalculateHandValue(playerHand);

        if(playerHasBlackjack && dealerValue != 21)
        {
            StartCoroutine(EndGameCoroutine("Blackjack! You win"));

            yield break;
        }
        else if(playerHasBlackjack && dealerValue == 21)
        {
            statusText.text = "Dealer also has Blackjack!";

            yield return new WaitForSeconds(1.5f);

            StartCoroutine(EndGameCoroutine("Both have Blackjack! It's a tie"));

            yield break;
        }

        if(isKnifeActive)
        {
            yield return new WaitForSeconds(2f);
        }
        else
        {
            while(dealerValue < 17)
            {
                yield return StartCoroutine(DealCardToDealerCoroutine(false));

                UpdateUI(false);

                dealerValue = CalculateHandValue(dealerHand);

                yield return new WaitForSeconds(2f);
            }

            if(!isKnifeActive && dealerValue <= 21)
            {
                statusText.text = "Dealer stands";

                yield return new WaitForSeconds(1.5f);
            }
        }

        string resultMessage = DetermineWinner(playerValue, dealerValue);

        statusText.text = resultMessage;

        yield return new WaitForSeconds(2f);

        StartCoroutine(EndGameCoroutine(resultMessage));
    }

    private string DetermineWinner(int playerValue, int dealerValue)
    {
        if(playerValue > 21) return "Bust... You lose";
        else if(dealerValue > 21) return "Dealer busts... You win";
        else if(playerValue > dealerValue) return "You win";
        else if(dealerValue > playerValue) return "Dealer wins";
        else
        {
            if(dealerWinsTies) return "Dealer wins on tie";
            else
            {
                return "It's a tie";
            }
        }
    }

    private IEnumerator EndGameCoroutine(string message)
    {
        isRoundActive = false;

        if(message.Contains("You win") || message.Contains("Blackjack! You win"))
        {
            PlayerMoney += currentBet;

            AudioManager.instance.Play("MoneyGained");

            yield return new WaitForSeconds(3f);
        }
        else if(message.Contains("It's a tie")) { }
        else
        {
            PlayerMoney -= currentBet;

            AudioManager.instance.Play("MoneyLost");
        }

        UpdateUI(false);

        isActionLocked = false;

        yield return StartCoroutine(CheckForEventTriggerCoroutine());

        if(PlayerMoney < minBet)
        {
            SceneManager.LoadSceneAsync(0);

            yield break;
        }

        if(PlayerMoney >= 10000)
        {
            SceneManager.LoadSceneAsync(0);

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
}
