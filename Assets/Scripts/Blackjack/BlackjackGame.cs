using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Prefabs.Managers;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[System.Serializable]
public class EventThreshold
{
    public BlackjackEvent.EventSeverity severityToTrigger;

    public int moneyAmount;

    public int maxTurns;
}

public enum CameraType
{
    Sitting,
    Playing,
    Event
}

public class BlackjackGame : MonoBehaviour
{
    #region Attributes
    [Header("Set-Up")]
    [SerializeField] private ItemManager itemManager;
    [SerializeField] private CursorDetection cursorDetection;
    [SerializeField] private CursorFollowManager cursorFollowManager;
    [SerializeField] private DialogueSystem dialogueSystem;
    [SerializeField] private Collider betUpCollider;
    [SerializeField] private Collider betDownCollider;
    [SerializeField] private int riggedRoundsLimit = 5;
    private Dictionary<CardInstance, int> scissoredCards = new Dictionary<CardInstance, int>();
    private Coroutine currentBustCoroutine = null;
    private List<int> lotteryNumbers = new List<int>();
    private List<int> powerballNumbers = new List<int>();
    private Deck gameDeck;
    private int blackjackGoal = 21;
    private int roundsCompleted = 0;
    private int maxSplits = 3;
    private bool isSplitting = false;
    private bool isKnifeActive = false;
    private bool isScissorsActive = false;
    private bool isAcidActive = false;
    private bool isCrucifixActive = false;
    private bool isCigaretteActive = false;
    private bool isAlcoholActive = false;
    private bool isActionLocked = false;
    private bool tutorialCompleted = false;
    private bool hasSeenSplitTutorial = false;
    private bool hasSeenDoubleDownTutorial = false;
    private bool isTutorialActive => roundsCompleted < tutorialRoundsLimit;
    [HideInInspector] public List<int> GetLotteryNumbers() => lotteryNumbers;
    [HideInInspector] public bool canDoubleDown = false;
    [HideInInspector] public bool isRoundActive = false;
    [HideInInspector] public bool isLottoActive = false;
    [HideInInspector] public bool isOrganActive = false;

    [Header("Event System")]
    [SerializeField] private bool useTurnLimit = false;
    [SerializeField] private List<EventThreshold> eventThresholds;
    [SerializeField] private List<BlackjackEvent> lowSeverityEvents;
    [SerializeField] private List<BlackjackEvent> mediumSeverityEvents;
    [SerializeField] private List<BlackjackEvent> highSeverityEvents;
    [SerializeField] public UnityEvent OnAddCardsEvent;
    [FormerlySerializedAs("OnSelectCopyCardEvent")] [SerializeField] public UnityEvent DeleteCopyOptions;
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
    private bool isPowerballTriggered = false;
    private bool isNewPowerball = false;

    [Header("Money")]
    [SerializeField] private int tutorialRoundsLimit = 3;
    [Tooltip("Money the player starts with.")]
    [SerializeField] private int playerMoney = 500;
    [Tooltip("Threshold for percentage pricing.")]
    [SerializeField] public int percentagePriceThreshold;
    [Tooltip("The minimum amount a bet can be.")]
    [SerializeField] private int minBet = 100;
    [Tooltip("Amount the bet increases / decreases.")]
    [SerializeField] private int betStep = 100;
    private int currentBet = 100;
    private bool priceChanged = false;

    [Header("Camera")]
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private CinemachineCamera sittingCamera;
    [SerializeField] private CinemachineCamera playingCamera;
    [SerializeField] private CinemachineCamera eventCamera;
    [SerializeField] private CinemachineBasicMultiChannelPerlin noise;
    [SerializeField] private float cameraTransitionTime;

    [Header("UI")]
    [SerializeField] private TMPro.TextMeshProUGUI moneyText;
    [SerializeField] private TMPro.TextMeshProUGUI betText;
    [SerializeField] private TMPro.TextMeshProUGUI statusText;
    [SerializeField] private TMPro.TextMeshProUGUI dealerTotalText;
    [SerializeField] private TMPro.TextMeshProUGUI rouletteText;
    [SerializeField] private UnityEvent ChangeProgressText;
    [SerializeField] private UnityEvent UpdatePowerballGoal;

    [Header("VFX")]
    [SerializeField] private Animator standHandAnimator;
    [SerializeField] private Animator hitHandAnimator;
    [SerializeField] private Animator buttonAnimator;
    [SerializeField] private GameObject greenParticlePrefab;
    [SerializeField] private GameObject redParticlePrefab;
    [SerializeField] private Transform particleSpawnPoint;
    [SerializeField] private ParticleSystem smokeParticle;
    [SerializeField] private GameObject distortion;
    [SerializeField] private Animator bottleAnimation;
    [SerializeField] private GameObject scissorsFollow;
    [SerializeField] private GameObject acidFollow;
    private GameObject peekedCardObject = null;
    private const float zOverlap = 0.001f;
    private const float cardAnimationDuration = 0.25f;

    [Header("Visual Setup")]
    [SerializeField] private List<CardVisuals> cardPrefabs = new List<CardVisuals>();
    [SerializeField] private List<Transform> handPositions = new List<Transform>();
    [SerializeField] private List<TMPro.TextMeshProUGUI> handTotalTexts;
    [SerializeField] private Transform dealerCardPosition;
    [SerializeField] private Transform sunglassesCardPosition;
    [SerializeField] private Transform deckPosition;
    [Tooltip("Offsets the player cards to create the staircase layout.")]
    [SerializeField] private Vector2 playerCardOffset = new Vector2(10f, -10f);
    [Tooltip("Space between the dealers cards.")]
    [SerializeField] private float dealerCardHorizontalSpacing = 35f;
    private Dictionary<(Card.Rank, Card.Suit), GameObject> cardPrefabLookup;
    private readonly Vector3 cardScaleVector = Vector3.one * 0.05f;
    private List<CardInstance> dealerHand = new List<CardInstance>();
    private List<GameObject> activeCardObjects = new List<GameObject>();
    private HashSet<CardInstance> alcoholCards = new HashSet<CardInstance>();
    private List<List<CardInstance>> playerHands = new List<List<CardInstance>>();
    private CardInstance peekCardInstance = null;
    private List<int> handBets = new List<int>();
    private int currentHandIndex = 0;
    private int triggeredThresholdsCount = 0;
    private int currentMaxTurns;
    private int currentTurns;
    private IEnumerator eventTriggerCoroutine;

    #endregion

    #region Getters & Setters
    public bool IsDoubleLowActive() => isDoubleLowActive;
    public bool IsHalfHighActive() => isHalfHighActive;
    public List<Card.Suit> GetNegativeSuits() => negativeSuits;
    public bool IsCardScissored(CardInstance cardInstance) => scissoredCards.ContainsKey(cardInstance);
    public void SetScissorsActive(bool active)
    {
        isScissorsActive = active;
        cursorFollowManager.SetCursorTypeActive(active, CursorType.Scissors);
    }
    public int GetOrganRoundsLeft() => itemManager.organRoundsLeft;
    
    public List<EventThreshold> EventThresholds => eventThresholds;
    public int TriggeredThresholdsCount => triggeredThresholdsCount;
    public bool UseTurnLimit => useTurnLimit;
    public int TurnsLeft => currentMaxTurns - currentTurns;
    public Transform CardOptionPosition => cursorDetection.GetCardOptionsPosition();
    public List<int> PowerballGoal => powerballNumbers;
    #endregion

    #region Monobehaviour Methods

    private void Awake()
    {
        currentMaxTurns = eventThresholds.First().maxTurns;
        currentTurns = 0;
    }

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
        if(currentBustCoroutine != null || isActionLocked || isRoundActive) return;

        if(Input.mouseScrollDelta.y > 0f)
        {
            IncreaseBet();
        }
        else if(Input.mouseScrollDelta.y < 0f)
        {
            DecreaseBet();
        }
    }
    #endregion
    
    #region Player Actions
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
    #endregion

    #region Betting Methods
    public int PlayerMoney
    {
        get { return playerMoney; }
        private set { playerMoney = value; }
    }

    public void UpdateBettingUI()
    {
        if(isTutorialActive)
        {
            if(moneyText != null) moneyText.text = "";

            if(betText != null) betText.text = "";

            return;
        }

        if(!isRoundActive && currentBet > playerMoney)
        {
            currentBet = playerMoney;
        }

        int displayBet = currentBet;

        if(isRoundActive && handBets != null && handBets.Count > 0)
        {
            displayBet = 0;

            foreach(int bet in handBets)
            {
                displayBet += bet;
            }
        }

        if(moneyText != null) moneyText.text = $"${playerMoney}";

        if(betText != null) betText.text = $"${displayBet}";
    }

    public void IncreaseBet()
    {
        if(isRoundActive || isTutorialActive) return;

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
        if(isRoundActive || isTutorialActive) return;

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

    public void ChangeToCamera(CameraType cameraType)
    {
        sittingCamera.Priority = 0;
        eventCamera.Priority = 0;
        playingCamera.Priority = 0;
        
        switch (cameraType)
        {
            case CameraType.Sitting:
                sittingCamera.Priority = 10;
                break;
            case CameraType.Playing:
                playingCamera.Priority = 10;
                break;
            case CameraType.Event:
                eventCamera.Priority = 10;
                break;
        }
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

    public void SellItem(int amount)
    {
        playerMoney += amount;
        
        UpdateBettingUI();
    }

    public bool ActivateKnife()
    {
        if(!isRoundActive || isKnifeActive || isActionLocked) return false;

        isKnifeActive = true;

        return true;
    }

    public bool ActivateScissors()
    {
        if(!isRoundActive || isScissorsActive || isActionLocked) return false;

        cursorFollowManager.SetCursorTypeActive(true, CursorType.Scissors);
        cursorDetection.OnUseCardItem(this, CardTrigger.Scissors);
        
        return true;
    }

    public void ApplyCutToCard(CardInstance cardInstance, int reduction)
    {
        if(scissoredCards.ContainsKey(cardInstance))
        {
            scissoredCards[cardInstance] *= reduction;
        }
        else
        {
            scissoredCards.Add(cardInstance, reduction);
        }
    }

    public bool ActivateAcid()
    {
        if(!isRoundActive || isAcidActive || isActionLocked) return false;

        cursorFollowManager.SetCursorTypeActive(true, CursorType.Acid);
        cursorDetection.OnUseCardItem(this, CardTrigger.Acid);
        
        return true;
    }

    public void ApplyDissolveToCard(CardInstance cardInstance, float delay)
    {
        cursorFollowManager.SetCursorTypeActive(false, CursorType.Acid);
        StartCoroutine(DissolveCard(cardInstance, delay));
    }

    private IEnumerator DissolveCard(CardInstance cardInstance, float delay)
    {
        yield return new WaitForSeconds(delay);

        var cardObject = cardInstance.displayComponent.gameObject;
        activeCardObjects.Remove(cardObject);
        gameDeck.AddRemovedCard(cardInstance.cardData.rank, cardInstance.cardData.suit);
        
        if (dealerHand.Remove(cardInstance))
        {
            DestroyCard(cardObject);
            yield return null;
        }

        foreach (var playerHand in playerHands)
        {
            if (playerHand.Remove(cardInstance))
            {
                DestroyCard(cardObject);
                yield return null;
            }
        }
        
        peekCardInstance = null;
        DestroyCard(cardObject);

        yield return null;
    }

    private void DestroyCard(GameObject cardObject)
    {
        Destroy(cardObject);
        isAcidActive = false;
        UpdateUI();
    }

    public bool ActivateCrucifix()
    {
        if(!isRoundActive || isActionLocked) return false;

        isCrucifixActive = true;

        return true;
    }

    public bool ActivateSunglasses()
    {
        if(!isRoundActive || peekedCardObject != null || isActionLocked) return false;

        Card? nextCard = gameDeck.PeekCard();

        if(!nextCard.HasValue) return false;

        Card newCardData = nextCard.Value;

        if(!cardPrefabLookup.TryGetValue((newCardData.rank, newCardData.suit), out GameObject cardPrefabToUse)) return false;

        peekedCardObject = Instantiate(cardPrefabToUse, sunglassesCardPosition);
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

            bool isSuitNegative = negativeSuits.Contains(newCardData.suit);
            bool isDoubled = CheckIfDoubled(newCardData) || isAlcoholActive;
            bool isHalved = CheckIfHalved(newCardData);

            cardDisplay.SetNegativeVisual(isSuitNegative);
            cardDisplay.SetDoubledVisual(isDoubled);
            cardDisplay.SetCutVisual(isHalved);
            
            peekCardInstance = new CardInstance(newCardData, cardDisplay);
        }

        activeCardObjects.Add(peekedCardObject);

        return true;
    }

    public bool ActivateOrgan()
    {
        if(isOrganActive) return false;

        isOrganActive = true;

        return true;
    }

    public void DeactivateOrgan()
    {
        isOrganActive = false;
    }

    public bool ActivateCigarette()
    {
        if(!isRoundActive || isActionLocked || isCigaretteActive || isSplitting) return false;

        isCigaretteActive = true;

        StartCoroutine(CigaretteCoroutine());

        return true;
    }

    private IEnumerator CigaretteCoroutine()
    {
        isActionLocked = true;

        List<CardInstance> tempHand = new List<CardInstance>(playerHands[currentHandIndex]);

        playerHands[currentHandIndex] = new List<CardInstance>(dealerHand);
        dealerHand = new List<CardInstance>(tempHand);

        AudioManager.instance.Play("Smoking");

        yield return new WaitForSeconds(1f);

        smokeParticle.Play();

        yield return new WaitForSeconds(1f);

        foreach(var card in playerHands[currentHandIndex])
        {
            if(card.isHidden)
            {
                yield return StartCoroutine(FlipCardCoroutine(card.displayComponent, 0.4f));

                card.isHidden = false;
            }
        }

        float animDuration = 0.5f;
        int maxCards = Mathf.Max(playerHands[currentHandIndex].Count, dealerHand.Count);

        Transform currentParent = handPositions[currentHandIndex];

        for(int i = 0; i < maxCards; i++)
        {
            if(i < playerHands[currentHandIndex].Count)
            {
                CardInstance pCard = playerHands[currentHandIndex][i];

                pCard.displayComponent.transform.SetParent(currentParent.parent);

                int cardOrderIndex = playerHands[currentHandIndex].Count - 1 - i;
                float xOffset = cardOrderIndex * playerCardOffset.x;
                float yOffset = cardOrderIndex * playerCardOffset.y;
                float zOffset = cardOrderIndex * -zOverlap;

                Vector3 targetLocalPos = new Vector3(xOffset, yOffset, zOffset);

                StartCoroutine(CardAnimationCoroutine(
                    pCard.displayComponent.transform,
                    currentParent.TransformPoint(targetLocalPos),
                    currentParent.rotation,
                    cardScaleVector,
                    animDuration
                ));
            }

            if(i < dealerHand.Count)
            {
                CardInstance dCard = dealerHand[i];

                dCard.displayComponent.transform.SetParent(dealerCardPosition.parent);

                int cardOrderIndex = dealerHand.Count - 1 - i;
                float xOffset = cardOrderIndex * dealerCardHorizontalSpacing;
                float yOffset = 0f;
                float zOffset = cardOrderIndex * -zOverlap;

                Vector3 targetLocalPos = new Vector3(xOffset, yOffset, zOffset);

                StartCoroutine(CardAnimationCoroutine(
                    dCard.displayComponent.transform,
                    dealerCardPosition.TransformPoint(targetLocalPos),
                    dealerCardPosition.rotation,
                    cardScaleVector,
                    animDuration
                ));
            }
        }

        yield return new WaitForSeconds(animDuration);

        foreach(CardInstance card in playerHands[currentHandIndex])
        {
            card.displayComponent.transform.SetParent(currentParent);
        }

        foreach(CardInstance card in dealerHand)
        {
            card.displayComponent.transform.SetParent(dealerCardPosition);
        }

        UpdateHandVisuals(playerHands[currentHandIndex], currentParent, true);
        UpdateHandVisuals(dealerHand, dealerCardPosition, false);
        UpdateUI(true);

        smokeParticle.Stop();
        isActionLocked = false;

        EvaluateDoubleDownCondition();
    }

    public bool ActivateAlcohol()
    {
        if(!isRoundActive || isActionLocked || isAlcoholActive) return false;

        isAlcoholActive = true;

        StartCoroutine(AlcoholCoroutine());

        return true;
    }

    private IEnumerator AlcoholCoroutine()
    {
        isActionLocked = true;

        bottleAnimation.gameObject.SetActive(true);
        bottleAnimation.SetTrigger("Drink");

        AudioManager.instance.Play("Drink");

        yield return StartCoroutine(DrinkAlcoholCoroutine());
        yield return new WaitForSeconds(1.5f);

        AudioManager.instance.isMuffled = true;

        distortion.SetActive(true);
        bottleAnimation.gameObject.SetActive(false);

        StartCoroutine(AlcoholCameraSway(0f, 0.2f, 0f, 0.1f, 1f));

        foreach(var hand in playerHands)
        {
            foreach(CardInstance card in hand)
            {
                alcoholCards.Add(card);

                card.displayComponent.SetDoubledVisual(true);
            }
        }

        UpdateUI(true);

        List<CardInstance> activeHand = playerHands[currentHandIndex];

        int handValue = CalculateHandValue(activeHand, true);

        if(handValue > blackjackGoal || handValue < -blackjackGoal)
        {
            yield return StartCoroutine(BustCheckCoroutine(activeHand));
        }
        else
        {
            isActionLocked = false;
        }

        UpdateCardVFX();
    }

    private IEnumerator AlcoholCameraSway(float minAmp, float maxAmp, float minFreq, float maxFreq, float speed)
    {
        float elapsedTime = 0f;

        while(isAlcoholActive)
        {
            elapsedTime += Time.deltaTime * speed;
            float lerpValue = Mathf.PingPong(elapsedTime, 1f);

            lerpValue = lerpValue * lerpValue * (3f - 2f * lerpValue);

            noise.AmplitudeGain = Mathf.Lerp(minAmp, maxAmp, lerpValue);
            noise.FrequencyGain = Mathf.Lerp(minFreq, maxFreq, lerpValue);

            yield return null;
        }
    }

    //Tilt camera down and back up to simulate the player taking a drink.
    private IEnumerator DrinkAlcoholCoroutine()
    {
        float elapsedTime = 0f;

        Quaternion startRot = playingCamera.transform.rotation;
        Quaternion targetRot = startRot * Quaternion.Euler(-30f, 0f, 0f);

        float halfDuration = 1f / 2f;

        while(elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;

            float tLerp = elapsedTime / halfDuration;
            float smoothT = tLerp * tLerp * (3f - 2f * tLerp);

            playingCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);

            yield return null;
        }

        elapsedTime = 0f;

        while(elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;

            float tLerp = elapsedTime / halfDuration;
            float smoothT = tLerp * tLerp * (3f - 2f * tLerp);

            playingCamera.transform.rotation = Quaternion.Slerp(targetRot, startRot, smoothT);

            yield return null;
        }

        playingCamera.transform.rotation = startRot;
    }

    public bool ActivateFan()
    {
        if(!isRoundActive || isActionLocked) return false;

        StartCoroutine(FanCoroutine());

        return true;
    }

    private IEnumerator FanCoroutine()
    {
        isActionLocked = true;

        yield return StartCoroutine(AnimateCardsOffScreen());

        ClearTable();

        playerHands.Add(new List<CardInstance>());
        handBets.Add(currentBet);
        currentHandIndex = 0;

        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(DealCardToPlayerCoroutine());
        yield return StartCoroutine(DealCardToDealerCoroutine(true));
        yield return StartCoroutine(DealCardToPlayerCoroutine());
        yield return StartCoroutine(DealCardToDealerCoroutine(false));

        UpdateUI();

        if(IsBlackjack(CalculateHandValue(playerHands[0], true)))
        {
            canDoubleDown = false;
            statusText.text = "Blackjack!";
            dialogueSystem.ShowPlayerBlackjackTaunt();

            yield return new WaitWhile(() => dialogueSystem.IsPlaying);
            yield return StartCoroutine(CheckLotteryTicket());
            yield return StartCoroutine(CheckPowerballCurrentHand());

            StartCoroutine(DealerTurnCoroutine(true));
        }
        else
        {
            statusText.text = "";
            isActionLocked = false;

            EvaluateDoubleDownCondition();
        }
    }

    private IEnumerator AnimateCardsOffScreen()
    {
        float animDuration = 2f;

        List<Coroutine> moveCoroutines = new List<Coroutine>();

        foreach(GameObject card in activeCardObjects)
        {
            Vector3 randomWindDirection = new Vector3(Random.Range(-25f, -15f), Random.Range(5f, 15f), Random.Range(-10f, 10f));
            Vector3 offScreenPos = card.transform.position + randomWindDirection;
            Vector3 randomSpin = new Vector3(Random.Range(-500f, 500f), Random.Range(-500f, 500f), Random.Range(-500f, 500f));

            moveCoroutines.Add(StartCoroutine(BlowCardAwayCoroutine(card.transform, offScreenPos, randomSpin, animDuration)));
        }

        foreach(Coroutine c in moveCoroutines)
        {
            yield return c;
        }
    }

    //Helps with spinning cards away when the fan is used.
    private IEnumerator BlowCardAwayCoroutine(Transform cardTransform, Vector3 targetPosition, Vector3 spinSpeed, float duration)
    {
        Vector3 startPosition = cardTransform.position;

        float time = 0;

        while(time < duration)
        {
            time += Time.deltaTime;

            float t = time / duration;
            float moveT = t * t * (3f - 2f * t);

            cardTransform.position = Vector3.Lerp(startPosition, targetPosition, moveT);
            cardTransform.Rotate(spinSpeed * Time.deltaTime, Space.World);

            yield return null;
        }
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

    public bool ActivateLotteryTicket()
    {
        if(isLottoActive) return false;

        isLottoActive = true;
        lotteryNumbers.Clear();

        for(int i = 0; i < 4; i++)
        {
            lotteryNumbers.Add(Random.Range(2, 34)); //2 to 33
        }

        return true;
    }

    public void DeactivateLotteryTicket()
    {
        isLottoActive = false;
        lotteryNumbers.Clear();
    }

    public bool TearLotteryTicket()
    {
        if(!isRoundActive || isActionLocked) return false;

        DeactivateLotteryTicket();

        AudioManager.instance.Play("LottoTear");

        return true;
    }

    private IEnumerator CheckLotteryTicket()
    {
        if(!isLottoActive) yield break;

        int moneyGained = 0;
        int smallReward = 500;
        int bigReward = 5000;

        foreach(var hand in playerHands)
        {
            int handValue = Mathf.Abs(CalculateHandValue(hand, true));
            int matches = lotteryNumbers.RemoveAll(number => number == handValue);

            moneyGained += matches * smallReward;
        }

        if(lotteryNumbers.Count == 0)
        {
            moneyGained += bigReward;

            isLottoActive = false;
            lotteryNumbers.Clear();
            itemManager.RemoveItemOfType(ItemType.Lotto);
        }

        if(moneyGained > 0)
        {
            int targetBalance = playerMoney + moneyGained;

            AudioManager.instance.Play("MoneyGained");

            yield return StartCoroutine(AnimateBetChange(targetBalance, 3f));
        }
    }

    private IEnumerator CheckPowerballCurrentHand() => CheckPowerballAtIndex(currentHandIndex);
    private IEnumerator CheckPowerballAtIndex(int index)
    {
        if (powerballNumbers == null || powerballNumbers.Count == 0) yield break;

        var hand = playerHands[index];
        int handValue = Mathf.Abs(CalculateHandValue(hand, true));
        powerballNumbers.RemoveAll(number => number == handValue);


        if (powerballNumbers.Count == 0)
        {
            dialogueSystem.ShowPowerballTaunt();
            var targetBalance = playerMoney + 3 * currentBet;
            
            AudioManager.instance.Play("MoneyGained");

            yield return StartCoroutine(AnimateBetChange(targetBalance, 3f));

            powerballNumbers = PowerballEvent.GenerateNumbers();
            isNewPowerball = true;
        }
        
        UpdatePowerballGoal?.Invoke();
    }
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

                yield return StartCoroutine(WaitDelayOrInput(5.0f));

                introPlayed = true;
                eventTriggered = true;
            }

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

                yield return StartCoroutine(WaitDelayOrInput(5.0f));
            }
        }

        if(eventTriggered)
        {
            triggeredThresholdsCount++;
            currentMaxTurns = eventThresholds[triggeredThresholdsCount].maxTurns;
            currentTurns = 0;
            
            DisableCamera(eventCamera);
            ChangeProgressText?.Invoke();
            UpdatePowerballGoal?.Invoke();
            EnableCamera(sittingCamera);
            if (isPowerballTriggered)
            {
                dialogueSystem.PlayPowerballTutorial();
                isPowerballTriggered = false;
            }
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

        UpdateCardVFX();
    }

    public void SetDoubleLowActive(bool active)
    {
        isDoubleLowActive = active;

        UpdateCardVFX();
    }

    public void SetHalfHighActive(bool active)
    {
        isHalfHighActive = active;

        UpdateCardVFX();
    }

    public void SetRouletteBlackjackActive(bool active)
    {
        isRouletteBlackjackActive = active;
    }

    public void DisplayCardOptions(int minValue, int maxValue)
    {
        var copyCount = gameDeck.GetCopyCount(minValue, maxValue);
        OnAddCardsEvent?.Invoke();
        StopCoroutine(eventTriggerCoroutine);
        ClearTable();

        foreach(var text in handTotalTexts)
        {
            text.text = "";
        }
        dealerTotalText.text = "";
        
        UpdateBettingUI();
        dialogueSystem.ShowAddCardsText(copyCount);
    }

    public void AddClickableCardOptions() => cursorDetection.OnSelectCardOption(this, CardTrigger.AddCardsEvent);
    public void AddCardCopies(Card card) => gameDeck.AddCardCopies(card);

    public void SelectCardCopyEnd() => StartCoroutine(SelectCardCopyEndCoroutine());

    public void SelectCursorHand(bool isActive)
    {
        cursorFollowManager.SetCursorTypeActive(isActive, CursorType.Flip);
        standHandAnimator.gameObject.SetActive(!isActive);
    }

    public void SetPowerballEventActive(List<int> goal)
    {
        powerballNumbers = goal;
        isPowerballTriggered = true;
    }
    
    private IEnumerator SelectCardCopyEndCoroutine()
    {
        yield return new WaitForSeconds(0.7f);
        dialogueSystem.ShowCopyChoiceTaunt();
        
        yield return new WaitForSeconds(1.5f);
        DeleteCopyOptions?.Invoke();
        StartGame();
    }

    private void RandomizeBlackjackGoal()
    {
        blackjackGoal = Random.Range(21, 37); //from 21 to 36

        rouletteText.text = blackjackGoal.ToString();
    }

    private bool CheckIfDoubled(Card card)
    {
        if(card.rank == Card.Rank.Joker) return false;

        if(!isDoubleLowActive) return false;

        float cardValue = 0f;

        if(card.rank >= Card.Rank.Ten && card.rank <= Card.Rank.King) cardValue = 10f;
        else if(card.rank == Card.Rank.Ace) cardValue = currentAceRule == AceValueRule.Always1 ? 1f : 11f;
        else cardValue = (int)card.rank;

        return cardValue < 6f;
    }

    private bool CheckIfHalved(Card card)
    {
        if(card.rank == Card.Rank.Joker) return false;

        if(!isHalfHighActive) return false;

        float cardValue = 0f;

        if(card.rank >= Card.Rank.Ten && card.rank <= Card.Rank.King) cardValue = 10f;
        else if(card.rank == Card.Rank.Ace) cardValue = currentAceRule == AceValueRule.Always1 ? 1f : 11f;
        else cardValue = (int)card.rank;

        return cardValue > 5f;
    }

    private IEnumerator ChangePriceCoroutine()
    {
        if(!priceChanged && playerMoney >= percentagePriceThreshold)
        {
            priceChanged = true;

            DisableCamera(playingCamera);
            DisableCamera(sittingCamera);
            EnableCamera(eventCamera);

            AudioManager.instance.Play("Laugh");

            statusText.text = "Let's make it more interesting";

            yield return StartCoroutine(WaitDelayOrInput(5.0f));

            AudioManager.instance.Play("NewEvent");

            statusText.text = "Item prices are scaling";

            yield return StartCoroutine(WaitDelayOrInput(4.0f));

            DisableCamera(eventCamera);
            EnableCamera(sittingCamera);

            statusText.text = "";
        }
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

        foreach(var hand in playerHands)
        {
            hand.Clear();
        }

        playerHands.Clear();
        handBets.Clear();
        dealerHand.Clear();
        alcoholCards.Clear();
        scissoredCards.Clear();

        if(peekedCardObject != null)
        {
            Destroy(peekedCardObject);

            peekedCardObject = null;
            peekCardInstance = null;
        }
    }

    private void StartGame()
    {
        StartCoroutine(ButtonCoroutine());

        if(isAlcoholActive)
        {
            AudioManager.instance.isMuffled = false;

            distortion.SetActive(false);

            StartCoroutine(AlcoholCameraSway(0f, 0f, 0f, 0f, 1.0f));
        }

        ClearTable();
        DisableCamera(playingCamera);
        EnableCamera(sittingCamera);
        if (isNewPowerball)
        {
            dialogueSystem.ShowPowerballGenerateTaunt();
            isNewPowerball = false;
        }

        AudioManager.instance.Play("Shuffle");

        gameDeck.InitializeDeck();
        gameDeck.Shuffle();
        cursorDetection.OnRoundInactive();

        if(!isTutorialActive)
        {
            itemManager.PlaySuitcaseOpen();
            statusText.text = "";
        }
        
        isRoundActive = false;
        isActionLocked = false;
        canDoubleDown = false;
        isSplitting = false;
        isKnifeActive = false;
        isScissorsActive = false;
        isAcidActive = false;
        isCrucifixActive = false;
        isCigaretteActive = false;
        isAlcoholActive = false;

        foreach(var text in handTotalTexts)
        {
            text.text = "";
        }

        dealerTotalText.text = "";
        rouletteText.text = "";
        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 0f;

        //Set bet to the last valid bet
        if(PlayerMoney < minBet) currentBet = PlayerMoney;
        else if(currentBet > PlayerMoney) currentBet = PlayerMoney;
        else if(currentBet < minBet) currentBet = minBet;

        UpdateBettingUI();
    }

    //Locks the bet and starts the round
    public IEnumerator DealRoundCoroutine()
    {
        if(!tutorialCompleted)
        {
            tutorialCompleted = true;
            dialogueSystem.PlayTutorial();

            yield return new WaitWhile(() => dialogueSystem.IsPlaying);
        }

        if(isRoundActive || PlayerMoney < currentBet || isActionLocked) yield break;

        isActionLocked = true;
        isRoundActive = true;
        playerHands.Clear();
        playerHands.Add(new List<CardInstance>());
        handBets.Clear();
        handBets.Add(isTutorialActive ? 0 : currentBet);
        currentHandIndex = 0;
        buttonAnimator.SetBool("StartActive", false);

        AudioManager.instance.Play("Button");

        yield return new WaitForSeconds(0.5f);

        if(isRouletteBlackjackActive)
        {
            DisableCamera(sittingCamera);
            EnableCamera(eventCamera);
            RandomizeBlackjackGoal();

            statusText.text = $"New Blackjack goal: {blackjackGoal}";

            yield return StartCoroutine(WaitDelayOrInput(4f));

            DisableCamera(eventCamera);
            EnableCamera(playingCamera);
        }

        DisableCamera(sittingCamera);
        EnableCamera(playingCamera);

        statusText.text = "Dealing cards...";
        cursorDetection.OnRoundActive();
        itemManager.ChangeItemAction(true);
        itemManager.DespawnPowerUps();

        if(roundsCompleted < riggedRoundsLimit)
        {
            RigPlayerHand();
        }

        yield return StartCoroutine(DealCardToPlayerCoroutine());
        yield return StartCoroutine(DealCardToDealerCoroutine(true));
        yield return StartCoroutine(DealCardToPlayerCoroutine());
        yield return StartCoroutine(DealCardToDealerCoroutine(false));

        UpdateUI();

        if(IsBlackjack(CalculateHandValue(playerHands[0], true)))
        {
            canDoubleDown = false;
            statusText.text = "Blackjack!";
            dialogueSystem.ShowPlayerBlackjackTaunt();

            yield return new WaitWhile(() => dialogueSystem.IsPlaying);
            yield return StartCoroutine(CheckLotteryTicket());
            yield return StartCoroutine(CheckPowerballCurrentHand());

            StartCoroutine(DealerTurnCoroutine(true));
        }
        else
        {
            statusText.text = "";
            isActionLocked = false;

            EvaluateDoubleDownCondition();

            if(!hasSeenSplitTutorial && CanSplit() && roundsCompleted >= 2)
            {
                isActionLocked = true;
                hasSeenSplitTutorial = true;
                dialogueSystem.PlaySplitTutorial();

                yield return new WaitWhile(() => dialogueSystem.IsPlaying);

                isActionLocked = false;
            }

            if(!hasSeenDoubleDownTutorial && roundsCompleted >= 7 && canDoubleDown)
            {
                isActionLocked = true;
                hasSeenDoubleDownTutorial = true;
                dialogueSystem.PlayDoubleDownTutorial();

                yield return new WaitWhile(() => dialogueSystem.IsPlaying);

                isActionLocked = false;
            }
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

        bool isSuitNegative = negativeSuits.Contains(newCardData.suit);
        bool isDoubled = CheckIfDoubled(newCardData);
        bool isHalved = CheckIfHalved(newCardData);

        cardDisplay.SetNegativeVisual(isSuitNegative);
        cardDisplay.SetDoubledVisual(isDoubled);
        cardDisplay.SetCutVisual(isHalved);

        if(cardDisplay != null) cardDisplay.SetHidden(isHidden);

        CardInstance newCardInstance = new CardInstance(newCardData, cardDisplay, isHidden);

        if(newCardInstance.cardData.rank == Card.Rank.Joker)
        {
            newCardInstance.jokerValue = Random.Range(-10, 11); //Joker value between -10 and 10
        }

        hand?.Insert(0, newCardInstance);

        return newCardInstance;
    }
    
    public CardInstance DealCardInstanceOption(Card newCardData, bool isHidden)
    {
        if(!cardPrefabLookup.TryGetValue((newCardData.rank, newCardData.suit), out GameObject cardPrefabToUse)) return null;

        GameObject cardObject = Instantiate(cardPrefabToUse, deckPosition);

        cardObject.transform.localScale = cardScaleVector;

        CardDisplay cardDisplay = cardObject.GetComponent<CardDisplay>();

        bool isSuitNegative = negativeSuits.Contains(newCardData.suit);
        bool isDoubled = CheckIfDoubled(newCardData);
        bool isHalved = CheckIfHalved(newCardData);

        cardDisplay.SetNegativeVisual(isSuitNegative);
        cardDisplay.SetDoubledVisual(isDoubled);
        cardDisplay.SetCutVisual(isHalved);

        if(cardDisplay != null) cardDisplay.SetHidden(isHidden);

        CardInstance newCardInstance = new CardInstance(newCardData, cardDisplay, isHidden);

        if(newCardInstance.cardData.rank == Card.Rank.Joker)
        {
            newCardInstance.jokerValue = Random.Range(-10, 11); //Joker value between -10 and 10
        }

        return newCardInstance;
    }

    public Card DealCard()
    {
        return gameDeck.DealCard();
    }

    private IEnumerator DealCardToPlayerCoroutine()
    {
        var savedPosition = deckPosition.position;

        Card newCardData = new Card { rank = Card.Rank.None };

        bool cardFound = false;

        List<CardInstance> currentHand = playerHands[currentHandIndex];

        if(isCrucifixActive)
        {
            isCrucifixActive = false;

            int playerValue = CalculateHandValue(currentHand, true);
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
            // newCardData = gameDeck.DealCard();
            if (powerballNumbers == null || powerballNumbers.Count == 0)
                newCardData = gameDeck.DealCard();
            else
            {
                var ncd = gameDeck.DealSpecificCard(Card.Rank.Ten);
                if (ncd != null) newCardData = (Card)ncd;
                else newCardData = (Card) gameDeck.DealSpecificCard(Card.Rank.King);
            }
        }

        Transform currentParent = handPositions[currentHandIndex];
        
        CardInstance newCardInstance;
        if (peekCardInstance == null) 
            newCardInstance = DealCardInstance(newCardData, currentHand, currentParent, false);
        else
        {
            newCardInstance = peekCardInstance;
            currentHand.Insert(0, newCardInstance);
            peekCardInstance = null;
        }
        AudioManager.instance.Play("CardHit");

        if(newCardInstance != null)
        {
            int cardOrderIndex = currentHand.Count - 1;
            float xOffset = cardOrderIndex * playerCardOffset.x;
            float yOffset = cardOrderIndex * playerCardOffset.y;
            float zOffset = cardOrderIndex * -zOverlap;

            Vector3 targetLocalPos = new Vector3(xOffset, yOffset, zOffset);
            Quaternion targetRotation = Quaternion.identity;

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

            UpdateHandVisuals(currentHand, currentParent, true);
            UpdateUI(true);
            UpdateSplitOutlines();
        }

        deckPosition.position = savedPosition;
    }

    private IEnumerator DealCardToDealerCoroutine(bool isHidden)
    {
        Card newCardData = gameDeck.DealCard();

        CardInstance newCardInstance;
        if (peekCardInstance == null) 
            newCardInstance = DealCardInstance(newCardData, dealerHand, dealerCardPosition, isHidden);
        else
        {
            newCardInstance = peekCardInstance;
            dealerHand.Insert(0, newCardInstance);
            peekCardInstance = null;
        }
        AudioManager.instance.Play("CardHit");

        if(newCardInstance != null)
        {
            int cardOrderIndex = dealerHand.Count - 1;
            float xOffset = cardOrderIndex * dealerCardHorizontalSpacing;
            float yOffset = 0f;
            float zOffset = cardOrderIndex * -zOverlap;

            Vector3 targetLocalPos = new Vector3(xOffset, yOffset, zOffset);
            Quaternion targetRotation = Quaternion.identity;

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

            UpdateHandVisuals(dealerHand, dealerCardPosition, false);
            UpdateUI(true);
        }
    }
    
    public IEnumerator DealCardOption()
    {
        var card = gameDeck.DealCard();
        var cardInstance = DealCardInstanceOption(card, false);
        AudioManager.instance.Play("CardHit");

        if (cardInstance != null)
        {
            int cardOrderIndex = dealerHand.Count - 1;
            float xOffset = cardOrderIndex * dealerCardHorizontalSpacing;
            float yOffset = 0f;
            float zOffset = cardOrderIndex * -zOverlap;

            Vector3 targetLocalPos = new Vector3(xOffset, yOffset, zOffset);
            Quaternion targetRotation = Quaternion.identity;

            cardInstance.displayComponent.transform.SetParent(dealerCardPosition.parent);

            yield return StartCoroutine(CardAnimationCoroutine(
                cardInstance.displayComponent.transform,
                dealerCardPosition.TransformPoint(targetLocalPos),
                dealerCardPosition.rotation * targetRotation,
                cardScaleVector,
                cardAnimationDuration
            ));

            cardInstance.displayComponent.transform.SetParent(dealerCardPosition);
            cardInstance.displayComponent.transform.localPosition = targetLocalPos;
            cardInstance.displayComponent.transform.localRotation = targetRotation;
            cardInstance.displayComponent.transform.localScale = cardScaleVector;

            UpdateHandVisuals(dealerHand, dealerCardPosition, false);
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

        List<CardInstance> activeHand = playerHands[currentHandIndex];

        int handValue = CalculateHandValue(activeHand, true);

        if(activeHand.Count == 7 && handValue <= blackjackGoal)
        {
            statusText.text = "Hand full";

            yield return StartCoroutine(CheckLotteryTicket());
            yield return StartCoroutine(CheckPowerballCurrentHand());
            yield return new WaitForSeconds(1.5f);
            yield return StartCoroutine(AdvanceHandCoroutine());
        }
        else if(handValue > blackjackGoal || handValue < -blackjackGoal)
        {
            yield return StartCoroutine(BustCheckCoroutine(activeHand));
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
        statusText.text = "You stand";
        standHandAnimator.SetTrigger("standTrigger");

        if(isLottoActive)
        {
            yield return StartCoroutine(CheckLotteryTicket());
        }

        yield return StartCoroutine(CheckPowerballCurrentHand());
        yield return new WaitForSeconds(1.5f);
        yield return StartCoroutine(AdvanceHandCoroutine());
    }

    private IEnumerator DoubleDownCoroutine()
    {
        if(!isRoundActive || isActionLocked || !canDoubleDown) yield break;

        isActionLocked = true;
        canDoubleDown = false;

        handBets[currentHandIndex] *= 2;

        UpdateBettingUI();

        statusText.text = "You Double Down...";

        AudioManager.instance.Play("BetUp");

        hitHandAnimator.SetTrigger("doubleDownTrigger");

        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(DealCardToPlayerCoroutine());
        yield return StartCoroutine(CheckLotteryTicket());
        yield return StartCoroutine(CheckPowerballCurrentHand());
        yield return StartCoroutine(AdvanceHandCoroutine());
    }

    private IEnumerator SplitCoroutine()
    {
        isActionLocked = true;

        int betToAdd = handBets[currentHandIndex];

        AudioManager.instance.Play("BetUp");

        statusText.text = "Splitting Hand...";
        standHandAnimator.SetTrigger("splitTrigger");

        yield return new WaitForSeconds(2.0f);

        statusText.text = "";

        List<CardInstance> activeHand = playerHands[currentHandIndex];
        CardInstance cardToMove = activeHand[0];

        activeHand.RemoveAt(0);

        List<CardInstance> newHand = new List<CardInstance> { cardToMove };

        playerHands.Insert(currentHandIndex + 1, newHand);
        handBets.Insert(currentHandIndex + 1, betToAdd);

        UpdateBettingUI();

        for(int i = currentHandIndex + 2; i < playerHands.Count; i++)
        {
            Transform shiftTarget = handPositions[i];

            foreach(var card in playerHands[i])
            {
                card.displayComponent.transform.SetParent(shiftTarget);
            }

            UpdateHandVisuals(playerHands[i], shiftTarget, true);
        }

        AudioManager.instance.Play("CardHit");
        Transform targetPosition = handPositions[currentHandIndex + 1];

        yield return StartCoroutine(CardAnimationCoroutine(
            cardToMove.displayComponent.transform,
            targetPosition.position,
            targetPosition.rotation,
            cardScaleVector,
            cardAnimationDuration
        ));

        cardToMove.displayComponent.transform.SetParent(targetPosition);
        cardToMove.displayComponent.transform.localPosition = Vector3.zero;

        UpdateHandVisuals(activeHand, handPositions[currentHandIndex], true);
        UpdateHandVisuals(newHand, targetPosition, true);

        yield return new WaitForSeconds(0.5f);

        isActionLocked = false;

        EvaluateDoubleDownCondition();
        UpdateUI();
        UpdateSplitOutlines();
    }

    private IEnumerator AdvanceHandCoroutine()
    {
        currentHandIndex++;

        if(currentHandIndex >= playerHands.Count)
        {
            StartCoroutine(DealerTurnCoroutine());
        }
        else
        {
            statusText.text = "Playing next hand...";

            yield return new WaitForSeconds(1.5f);

            statusText.text = "";
            isActionLocked = false;

            EvaluateDoubleDownCondition();
            UpdateUI(true);
            UpdateSplitOutlines();
        }
    }

    private IEnumerator DealerTurnCoroutine(bool playerHasBlackjack = false)
    {
        foreach(var hand in playerHands)
        {
            foreach(var card in hand)
            {
                card.displayComponent.GetComponentInChildren<ClickableCard>()?.OnRemoveOutline();
            }
        }

        statusText.text = "Dealer turn...";

        yield return new WaitForSeconds(1.0f);

        bool allHandsBust = true;

        foreach(var hand in playerHands)
        {
            int val = CalculateHandValue(hand, true);

            if(val <= blackjackGoal && val >= -blackjackGoal)
            {
                allHandsBust = false;

                break;
            }
        }

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

            if(!playerHasBlackjack && IsBlackjack(dealerValueInit))
            {
                dialogueSystem.ShowDealerBlackjackTaunt();

                yield return new WaitWhile(() => dialogueSystem.IsPlaying);
            }

            if(playerHasBlackjack)
            {
                if(!IsBlackjack(dealerValueInit))
                {
                    StartCoroutine(EndGameCoroutine("Blackjack! You win"));

                    yield break;
                }
                else
                {
                    statusText.text = "Dealer also has Blackjack";

                    yield return new WaitForSeconds(1.5f);

                    StartCoroutine(EndGameCoroutine("Both have Blackjack. Its a tie"));

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
        int playerValue = CalculateHandValue(playerHands[0], true);
        bool playerBust = (playerValue > blackjackGoal || playerValue < -blackjackGoal);
        bool dealerBust = (finalDealerValue > blackjackGoal || finalDealerValue < -blackjackGoal);
        int playerDiff = Mathf.Abs(Mathf.Abs(playerValue) - blackjackGoal);
        int dealerDiff = Mathf.Abs(Mathf.Abs(finalDealerValue) - blackjackGoal);
        bool wonByOne = false;

        if(!playerBust && !dealerBust && playerDiff - dealerDiff == 1)
        {
            wonByOne = true;
        }

        if(wonByOne)
        {
            dialogueSystem.ShowDealerWinsByOneTaunt();

            yield return new WaitWhile(() => dialogueSystem.IsPlaying);
        }

        if(playerHands.Count > 1)
        {
            for(int i = 0; i < playerHands.Count; i++)
            {
                int finalPlayerValue = CalculateHandValue(playerHands[i], true);
                string resultMessage = DetermineWinner(finalPlayerValue, finalDealerValue);

                yield return StartCoroutine(ProcessPayout(resultMessage, handBets[i]));
                yield return new WaitForSeconds(1.5f);
            }

            yield return StartCoroutine(EndRoundSequence());
        }
        else
        {
            int finalPlayerValue = CalculateHandValue(playerHands[0], true);
            string resultMessage = DetermineWinner(finalPlayerValue, finalDealerValue);

            yield return StartCoroutine(EndGameCoroutine(resultMessage));
        }
    }

    private IEnumerator ProcessPayout(string message, int betAmount)
    {
        bool shouldPlayBetLostTaunt = false;

        if(message == "Its a tie")
        {
            dialogueSystem.ShowTieTaunt();

            yield return new WaitWhile(() => dialogueSystem.IsPlaying);
        }
        else
        {
            statusText.text = message;

            if((message.Contains("Dealer wins") || message.Contains("Bust")) && betAmount >= (playerMoney * 0.5f))
            {
                shouldPlayBetLostTaunt = true;
            }
        }

        if(isTutorialActive)
        {
            yield return new WaitForSeconds(1.5f);
            yield break;
        }

        if(message.Contains("You win"))
        {
            targetMoneyBalance = playerMoney + betAmount;

            AudioManager.instance.Play("MoneyGained");

            Instantiate(greenParticlePrefab, particleSpawnPoint.position, particleSpawnPoint.rotation);

            yield return StartCoroutine(AnimateBetChange(targetMoneyBalance, 3f));
        }
        else if(message.Contains("Dealer wins") || message.Contains("Bust"))
        {
            if(!isOrganActive)
            {
                targetMoneyBalance = playerMoney - betAmount;

                AudioManager.instance.Play("MoneyLost");

                Instantiate(redParticlePrefab, particleSpawnPoint.position, particleSpawnPoint.rotation);

                standHandAnimator.SetTrigger("flipperTrigger");

                yield return StartCoroutine(AnimateBetChange(targetMoneyBalance, 3f));
            }
            else
            {
                AudioManager.instance.Play("MoneyLost");

                standHandAnimator.SetTrigger("flipperTrigger");

                yield return new WaitForSeconds(0.5f);

                AudioManager.instance.Play("OrganExpire");

                itemManager.RemoveItemOfType(ItemType.Organ);
                isOrganActive = false;
                targetMoneyBalance = playerMoney;

                yield return new WaitForSeconds(2.0f);
            }
        }
        else
        {
            targetMoneyBalance = playerMoney;

            yield return new WaitForSeconds(1.5f);
        }

        if(shouldPlayBetLostTaunt)
        {
            dialogueSystem.ShowBetLostTaunt();

            yield return new WaitWhile(() => dialogueSystem.IsPlaying);
        }
    }

    private IEnumerator BustCheckCoroutine(List<CardInstance> activeHand)
    {
        if(isLottoActive)
        {
            yield return StartCoroutine(CheckLotteryTicket());
        }

        yield return StartCoroutine(CheckPowerballCurrentHand());
        yield return new WaitForSeconds(2f);

        var playerJokers = activeHand.Where(c => c.cardData.rank == Card.Rank.Joker).ToList();
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

            yield return new WaitForSeconds(2f);
        }

        currentBustCoroutine = null;

        if(playerHands.Count == 1)
        {
            yield return StartCoroutine(EndGameCoroutine("Bust... You lose"));
        }
        else
        {
            statusText.text = "Hand Bust";

            yield return new WaitForSeconds(1.5f);
            yield return StartCoroutine(AdvanceHandCoroutine());
        }
    }

    private string DetermineWinner(int playerValue, int dealerValue)
    {
        bool playerBust = (playerValue > blackjackGoal || playerValue < -blackjackGoal);
        bool dealerBust = (dealerValue > blackjackGoal || dealerValue < -blackjackGoal);
        int playerDiff = Mathf.Abs(Mathf.Abs(playerValue) - blackjackGoal);
        int dealerDiff = Mathf.Abs(Mathf.Abs(dealerValue) - blackjackGoal);

        if(playerBust) return "Bust... You lose";

        if(dealerBust) return "Dealer busts... You win";

        if(playerDiff < dealerDiff) return "You win";

        if(dealerDiff < playerDiff) return "Dealer wins";

        return "Its a tie";
    }

    private IEnumerator EndGameCoroutine(string message)
    {
        int activeBetAmount = (handBets != null && handBets.Count > 0) ? handBets[0] : currentBet;

        yield return StartCoroutine(ProcessPayout(message, activeBetAmount));
        yield return StartCoroutine(EndRoundSequence());
    }
    #endregion

    #region Card Visuals
    //The dealer hand is in a straight line, the player hand creates a staircase effect.
    private void UpdateHandVisuals(List<CardInstance> hand, Transform parentPos, bool isPlayerHand)
    {
        int cardCount = hand.Count;

        if(cardCount == 0) return;

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

    public void UpdateCardVFX()
    {
        foreach(var hand in playerHands)
        {
            foreach(CardInstance card in hand)
            {
                bool isNegative = negativeSuits.Contains(card.cardData.suit);
                bool isDoubled = CheckIfDoubled(card.cardData) || isAlcoholActive;
                bool isHalved = CheckIfHalved(card.cardData) || scissoredCards.ContainsKey(card);

                card.displayComponent.SetNegativeVisual(isNegative);
                card.displayComponent.SetDoubledVisual(isDoubled);
                card.displayComponent.SetCutVisual(isHalved);
            }
        }

        foreach(CardInstance card in dealerHand)
        {
            bool isNegative = negativeSuits.Contains(card.cardData.suit);
            bool isDoubled = CheckIfDoubled(card.cardData);
            bool isHalved = CheckIfHalved(card.cardData) || scissoredCards.ContainsKey(card);

            card.displayComponent.SetNegativeVisual(isNegative);
            card.displayComponent.SetDoubledVisual(isDoubled);
            card.displayComponent.SetCutVisual(isHalved);
        }

        if(peekedCardObject != null)
        {
            Card? topCard = gameDeck.PeekCard();

            if(topCard.HasValue)
            {
                CardDisplay display = peekedCardObject.GetComponent<CardDisplay>();

                if(display != null)
                {
                    Card cardData = topCard.Value;

                    bool isNegative = negativeSuits.Contains(cardData.suit);
                    bool isDoubled = CheckIfDoubled(cardData) || isAlcoholActive;
                    bool isHalved = CheckIfHalved(cardData);

                    display.SetNegativeVisual(isNegative);
                    display.SetDoubledVisual(isDoubled);
                    display.SetCutVisual(isHalved);
                }
            }
        }
    }

    //Animates a card moving from the deck to its position in the hand.
    public IEnumerator CardAnimationCoroutine(Transform cardTransform, Vector3 targetPosition, Quaternion targetRotation, Vector3 targetScale, float duration)
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

    //Flip animation for revealing the hidden card.
    private IEnumerator FlipCardCoroutine(CardDisplay cardDisplay, float duration)
    {
        Transform cardTransform = cardDisplay.transform;

        Quaternion startRotation = cardTransform.localRotation;
        Quaternion ninetyDegrees = Quaternion.Euler(0, 90f, startRotation.eulerAngles.z);

        float halfDuration = duration / 2.0f;
        float elapsedTime = 0;

        if(!isCigaretteActive)
        {
            AudioManager.instance.Play("Flip");
        }

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

    //Outlines every card in the active hand that is being played when splitting.
    private void UpdateSplitOutlines()
    {
        if(playerHands.Count <= 1) return;

        for(int i = 0; i < playerHands.Count; i++)
        {
            foreach(CardInstance card in playerHands[i])
            {
                ClickableCard clickable = card.displayComponent.GetComponentInChildren<ClickableCard>();

                if(clickable != null)
                {
                    if(i == currentHandIndex) clickable.ApplyOutline();
                    else clickable.OnRemoveOutline();
                }
            }
        }
    }

    private IEnumerator ButtonCoroutine()
    {
        buttonAnimator.SetBool("StartActive", true);

        yield return new WaitForSeconds(1f);

        AudioManager.instance.Play("ButtonOpen");
    }
    #endregion

    //Calculates the total value of a hand. Aces are 1 or 11.
    private int CalculateHandValue(List<CardInstance> hand, bool countJoker)
    {
        float value = 0f;

        List<float> aceReductions = new List<float>();

        for(int i = 0; i < hand.Count; i++)
        {
            CardInstance cardInstance = hand[i];

            Card card = cardInstance.cardData;

            float cardValue;
            float valueAsOne = 1f;

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

            if(isDoubleLowActive && card.rank != Card.Rank.Joker)
            {
                if(cardValue < 6)
                {
                    cardValue *= 2;
                }

                if(valueAsOne < 6)
                {
                    valueAsOne *= 2;
                }
            }

            if(isHalfHighActive && card.rank != Card.Rank.Joker)
            {
                if(cardValue > 5)
                {
                    cardValue = Mathf.CeilToInt(cardValue / 2f);
                }

                if(valueAsOne > 5)
                {
                    valueAsOne = Mathf.CeilToInt(valueAsOne / 2f);
                }
            }

            if(negativeSuits.Contains(card.suit))
            {
                cardValue = -cardValue;
                valueAsOne = -valueAsOne;
            }

            if(scissoredCards.TryGetValue(cardInstance, out int reduction))
            {
                if(card.rank == Card.Rank.Joker)
                {
                    cardValue = 0;
                    valueAsOne = 0;
                }
                else
                {
                    var half = Mathf.CeilToInt(Mathf.Abs(cardValue) / reduction);
                    if(cardValue > 0)
                    {
                        cardValue = half;
                    }
                    else if(cardValue < 0)
                    {
                        cardValue = -half;
                    }

                    var halfAce = Mathf.CeilToInt(Mathf.Abs(valueAsOne) / reduction);
                    if(valueAsOne > 0)
                    {
                        valueAsOne = halfAce;
                    }
                    else if(valueAsOne < 0)
                    {
                        valueAsOne = -halfAce;
                    }
                }
            }

            if(alcoholCards.Contains(cardInstance) && card.rank != Card.Rank.Joker)
            {
                cardValue *= 2;
                valueAsOne *= 2;
            }

            if(card.rank == Card.Rank.Ace && currentAceRule == AceValueRule.Flexible)
            {
                aceReductions.Add(Mathf.Abs(cardValue - valueAsOne));
            }

            value += cardValue;
        }

        if(currentAceRule == AceValueRule.Flexible)
        {
            aceReductions.Sort((a, b) => b.CompareTo(a));

            foreach(float reduction in aceReductions)
            {
                if(value > blackjackGoal || value < -blackjackGoal)
                {
                    value += (value > 0) ? -reduction : reduction;
                }
            }
        }

        return Mathf.RoundToInt(value);
    }

    private IEnumerator RevealJokers()
    {
        List<CardInstance> allPlayerJokers = new List<CardInstance>();

        foreach(var hand in playerHands)
        {
            allPlayerJokers.AddRange(hand.Where(c => c.cardData.rank == Card.Rank.Joker));
        }

        var dealerJokers = dealerHand.Where(c => c.cardData.rank == Card.Rank.Joker).ToList();
        string revealMessage = "";

        if(allPlayerJokers.Count > 0)
        {
            revealMessage += "Your Joker(s): ";
            revealMessage += string.Join(", ", allPlayerJokers.Select(j => j.jokerValue.ToString()));
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

            yield return StartCoroutine(WaitDelayOrInput(4f));
        }
        else
        {
            yield return StartCoroutine(WaitDelayOrInput(1.5f));
        }
    }

    //Updates the score, money, and checks for busts.
    public void UpdateUI(bool dealerHidden = true)
    {
        bool revealJokers = !dealerHidden;

        if(handTotalTexts != null && handTotalTexts.Count > 0)
        {
            for(int i = 0; i < handTotalTexts.Count; i++)
            {
                if(handTotalTexts[i] == null) continue;

                if(i < playerHands.Count)
                {
                    string prefix = playerHands.Count > 1 ? $"" : "";

                    handTotalTexts[i].text = FormatHandText(prefix, playerHands[i], revealJokers, false);
                }
                else
                {
                    handTotalTexts[i].text = "";
                }
            }
        }

        if(dealerTotalText != null)
        {
            if(dealerHand.Count > 0)
            {
                if(dealerHidden && dealerHand.Any(c => c.isHidden))
                {
                    List<CardInstance> visibleCards = dealerHand.Where(x => !x.isHidden).ToList();
                    dealerTotalText.text = FormatHandText("", visibleCards, revealJokers, true);
                }
                else
                {
                    dealerTotalText.text = FormatHandText("", dealerHand, revealJokers, false);
                }
            }
            else
            {
                dealerTotalText.text = "";
            }
        }

        UpdateBettingUI();
    }

    private string FormatHandText(string prefix, List<CardInstance> cards, bool revealJokers, bool dealerHasHiddenCard)
    {
        if(cards.Count == 0) return "";

        int totalValue = CalculateHandValue(cards, true);
        bool hasJoker = cards.Any(c => c.cardData.rank == Card.Rank.Joker);

        if(revealJokers || !hasJoker)
        {
            return prefix + (dealerHasHiddenCard ? $"{totalValue}" : totalValue.ToString());
        }

        int baseValue = CalculateHandValue(cards, false);

        return prefix + (dealerHasHiddenCard ? $"{baseValue}" : $"{baseValue}");
    }

    private IEnumerator EndRoundSequence()
    {
        if(isOrganActive)
        {
            itemManager.OnRoundEnded();
        }
        
        isRoundActive = false;
        cursorDetection.OnRoundInactive();
        itemManager.ChangeItemAction(false);

        if(roundsCompleted == tutorialRoundsLimit - 1)
        {
            DisableCamera(playingCamera);
            EnableCamera(eventCamera);

            AudioManager.instance.Play("Laugh");

            statusText.text = "Lets raise the stakes...";

            yield return StartCoroutine(WaitDelayOrInput(5.0f));

            AudioManager.instance.Play("NewEvent");

            statusText.text = "Betting enabled";

            yield return StartCoroutine(WaitDelayOrInput(5.0f));

            DisableCamera(eventCamera);
            EnableCamera(sittingCamera);

            betUpCollider.enabled = true;
            betDownCollider.enabled = true;
        }

        roundsCompleted++;
        if (useTurnLimit)
        {
            currentTurns++;
            ChangeProgressText.Invoke();
        }

        eventTriggerCoroutine = CheckForEventTriggerCoroutine();
        yield return StartCoroutine(eventTriggerCoroutine);

        if(!priceChanged)
        {
            yield return StartCoroutine(ChangePriceCoroutine());
        }

        if(playerMoney <= 100 && playerMoney > 0)
        {
            dialogueSystem.ShowLowMoneyTaunt();

            yield return new WaitWhile(() => dialogueSystem.IsPlaying);
        }

        if(!isTutorialActive && PlayerMoney <= 0)
        {
            SceneManager.LoadSceneAsync(3);

            yield break;
        }
        
        if (useTurnLimit && currentTurns >= currentMaxTurns)
        {
            dialogueSystem.ShowTurnLimitTaunt();

            yield return new WaitWhile(() => dialogueSystem.IsPlaying);
            
            SceneManager.LoadSceneAsync(3);

            yield break;
        }

        if(PlayerMoney >= 100000)
        {
            SceneManager.LoadSceneAsync(2);

            yield break;
        }

        isSplitting = false;

        StartGame();
    }

    private bool IsBlackjack(int handValue)
    {
        if(handValue == blackjackGoal || handValue == -blackjackGoal) return true;

        return false;
    }

    public bool CanSplit()
    {
        if(!isRoundActive || isActionLocked || playerHands.Count >= maxSplits + 1) return false;

        List<CardInstance> currentHand = playerHands[currentHandIndex];

        if(currentHand.Count != 2) return false;

        float val1 = GetCardValueForSplit(currentHand[0].cardData);
        float val2 = GetCardValueForSplit(currentHand[1].cardData);

        int totalBets = 0;

        foreach(int b in handBets) totalBets += b;

        return val1 == val2 && playerMoney >= (totalBets + handBets[currentHandIndex]);
    }

    private float GetCardValueForSplit(Card card)
    {
        float cardValue;

        if(card.rank >= Card.Rank.Ten && card.rank <= Card.Rank.King) cardValue = 10;
        else if(card.rank == Card.Rank.Ace) cardValue = 11;
        else cardValue = (int)card.rank;

        if(isDoubleLowActive && cardValue < 6 && card.rank != Card.Rank.Joker) cardValue *= 2;

        if(isHalfHighActive && cardValue > 5 && card.rank != Card.Rank.Joker) cardValue = Mathf.CeilToInt(cardValue / 2f);

        return cardValue;
    }

    private void EvaluateDoubleDownCondition()
    {
        if(currentHandIndex >= playerHands.Count)
        {
            canDoubleDown = false;

            return;
        }

        int totalBets = 0;

        foreach(int b in handBets) totalBets += b;

        canDoubleDown = playerMoney >= (totalBets + handBets[currentHandIndex]);
    }

    private void RigPlayerHand()
    {
        int maxAttempts = 50;
        int attempts = 0;

        while(attempts < maxAttempts)
        {
            Card? firstCard = gameDeck.PeekCardAt(0);
            Card? secondCard = gameDeck.PeekCardAt(2);

            if(!firstCard.HasValue || !secondCard.HasValue) break;

            int simulatedValue = SimulateInitialHandValue(firstCard.Value, secondCard.Value);

            if(simulatedValue >= 12 && simulatedValue <= 16)
            {
                gameDeck.Shuffle();

                attempts++;
            }
            else
            {
                break;
            }
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

    private IEnumerator WaitDelayOrInput(float duration)
    {
        float timer = 0f;

        yield return new WaitForSeconds(0.1f);

        timer += 0.1f;

        while(timer < duration)
        {
            if(Input.anyKeyDown) break;

            timer += Time.deltaTime;

            yield return null;
        }
    }
}
