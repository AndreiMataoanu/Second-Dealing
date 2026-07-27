using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;
using Random = UnityEngine.Random;

[System.Serializable]
public class EventThreshold
{
    public BlackjackEvent.EventSeverity severityToTrigger;

    public int moneyAmount;

    public int maxTurns;
}

public class BlackjackGame : MonoBehaviour
{
    #region Attributes
    [Header("Set-Up")]
    [SerializeField] private ItemManager itemManager;
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private CursorDetection cursorDetection;
    [SerializeField] private CursorFollow cursorFollow;
    [SerializeField] private DialogueSystem dialogueSystem;
    [SerializeField] private EventManager eventManager;
    [SerializeField] private GameCamera gameCamera;
    [SerializeField] private Collider betUpCollider;
    [SerializeField] private Collider betDownCollider;
    [SerializeField] private int riggedRoundsLimit = 5;
    private Coroutine currentBustCoroutine = null;
    private Coroutine dealToDealerCoroutine = null;
    private Deck gameDeck;
    private int blackjackGoal = 21;
    private int roundsCompleted = 0;
    private int maxSplits = 3;
    private int maxMoneyThisRun = 0;
    [HideInInspector] public bool isSplitting = false;
    [HideInInspector] public bool isActionLocked = false;
    private bool isMedicineActive = false;
    private bool useAfterStand = false;
    private bool tutorialCompleted = false;
    private bool hasSeenSplitTutorial = false;
    private bool hasSeenDoubleDownTutorial = false;
    private bool tieTauntPlayed = false;
    private bool isTutorialActive => roundsCompleted < tutorialRoundsLimit;
    [HideInInspector] public bool canDoubleDown = false;
    [HideInInspector] public bool isRoundActive = false;
    private bool stayed = false;

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
    private int targetMoneyBalance;

    private int TimesWon;
    private int TimesLost;

    [Header("UI")]
    [SerializeField] private TMPro.TextMeshProUGUI moneyText;
    [SerializeField] private TMPro.TextMeshProUGUI betText;
    [SerializeField] private TMPro.TextMeshProUGUI statusText;
    [SerializeField] private TMPro.TextMeshProUGUI dealerTotalText;
    [SerializeField] private TMPro.TextMeshProUGUI rouletteText;
    [SerializeField] private Button leavebutton;
    [SerializeField] private Button staybutton;

    [Header("VFX")]
    [SerializeField] private Animator standHandAnimator;
    [SerializeField] private Animator hitHandAnimator;
    [SerializeField] private Animator buttonAnimator;
    [SerializeField] private GameObject greenParticlePrefab;
    [SerializeField] private GameObject redParticlePrefab;
    [SerializeField] private Transform particleSpawnPoint;
    [SerializeField] private ParticleSystem smokeParticle;
    [SerializeField] public Animator bottleAnimation;
    [SerializeField] private Animator FadeInAnimator;
    public GameObject peekedCardObject = null;
    private const float cardAnimationDuration = 0.25f;

    [Header("Visual Setup")]
    [SerializeField] private List<CardVisuals> cardPrefabs = new List<CardVisuals>();
    [SerializeField] private List<Transform> handPositions = new List<Transform>();
    [SerializeField] private List<TMPro.TextMeshProUGUI> handTotalTexts;
    [SerializeField] private Transform dealerCardPosition;
    [SerializeField] public Transform sunglassesCardPosition;
    [SerializeField] private Transform deckPosition;
    [Tooltip("Offsets the player cards to create the staircase layout.")]
    [SerializeField] private Vector3 playerCardsOffset = new(0.03f, 0.034f, -0.001f);
    [Tooltip("Offsets the dealer cards to create a horizontal line.")]
    [SerializeField] private Vector3 dealerCardsOffset = new(0.13f, 0f, -0.001f);

    public Dictionary<(Card.Rank, Card.Suit), GameObject> cardPrefabLookup;
    private readonly Vector3 cardScaleVector = Vector3.one * 0.05f;
    public List<CardInstance> dealerHand = new List<CardInstance>();
    public List<GameObject> activeCardObjects = new List<GameObject>();
    public List<List<CardInstance>> playerHands = new List<List<CardInstance>>();
    public CardInstance peekCardInstance = null;
    private List<int> handBets = new List<int>();
    private int currentHandIndex = 0;
    private bool isPlayerStand = false;

    #endregion

    #region Getters & Setters
    public int GetPlayerMoney() => playerMoney;
    
    public Transform CardOptionPosition => cursorDetection.GetCardOptionsPosition();
    public DialogueSystem DialogueSystem => dialogueSystem;
    public EventManager EventManager => eventManager;
    public Deck GameDeck => gameDeck;
    public CursorDetection CursorDetection => cursorDetection;
    public CursorFollow CursorFollow => cursorFollow;
    public ShopManager ShopManager => shopManager;
    public ItemManager ItemManager => itemManager;
    public void SetStatusText(string text) => statusText.text = text;
    public void SetBlackjackGoal(int gameGoal)
    {
        blackjackGoal = gameGoal;
        rouletteText.text = blackjackGoal.ToString();
    }
    public List<List<CardInstance>> PlayerHands => playerHands;
    public int CurrentBet => currentBet;
    public int TargetMoneyBalance => targetMoneyBalance;
    public GameCamera GameCamera => gameCamera;
    public bool IsPlayerHandValid => currentHandIndex < playerHands.Count;
    public List<CardInstance> CurrentHand => playerHands[currentHandIndex];
    public bool UseAfterStand => useAfterStand;
    #endregion

    #region Monobehaviour Methods

    private void Start()
    {
        maxMoneyThisRun = playerMoney;
        gameDeck = new Deck();

        ManagerSetup();
        InitializeCardLookup();
        StartGame();
    }

    private void Update()
    {
        if(currentBustCoroutine != null || isActionLocked || isRoundActive) return;

        if(Input.mouseScrollDelta.y > 0f && Time.timeScale != 0f)
        {
            IncreaseBet();
        }
        else if(Input.mouseScrollDelta.y < 0f && Time.timeScale != 0f)
        {
            DecreaseBet();
        }
    }
    #endregion
    
    #region Player Actions
    public void OnStartGame()
    {
        isSplitting = false;
        isPlayerStand = false;
        
        shopManager.DespawnShopItems();
        itemManager.OnRoundStart();
        
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
    public IEnumerator AnimateBetChange(int targetAmount, float duration)
    {
        if(targetAmount > maxMoneyThisRun)
        {
            maxMoneyThisRun = targetAmount;
        }

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

    public void AnimateBetGain(int moneyGained)
    {
        int targetBalance = playerMoney + moneyGained;

        AudioManager.instance.Play("MoneyGained");

        StartCoroutine(AnimateBetChange(targetBalance, 3f / GameUtils.gameSpeedMultiplier));
    }

    public IEnumerator CigaretteCoroutine()
    {
        isActionLocked = true;

        if(dealToDealerCoroutine != null)
        {
            StopCoroutine(dealToDealerCoroutine);

            dealToDealerCoroutine = null;
        }

        int targetIndex = ChooseHandIndex();

        currentHandIndex = targetIndex;
        isPlayerStand = false;

        cursorDetection.OnRoundActive();

        List<CardInstance> tempHand = new List<CardInstance>(playerHands[currentHandIndex]);

        playerHands[currentHandIndex] = new List<CardInstance>(dealerHand);
        dealerHand = new List<CardInstance>(tempHand);

        AudioManager.instance.Play("Smoking");

        yield return new WaitForSeconds(1f);

        smokeParticle.Play();

        yield return new WaitForSeconds(1f);

        foreach(var card in playerHands[targetIndex])
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
                Vector3 targetLocalPos = playerCardsOffset * cardOrderIndex;

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
                Vector3 targetLocalPos = dealerCardsOffset * cardOrderIndex;

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

        UpdateHandVisuals(playerHands[currentHandIndex], true);
        UpdateHandVisuals(dealerHand, false);
        UpdateUI(true);

        smokeParticle.Stop();

        int handValue = CalculateHandValue(playerHands[targetIndex], true);

        if(handValue > blackjackGoal || handValue < -blackjackGoal)
        {
            yield return StartCoroutine(BustCheckCoroutine(playerHands[targetIndex], targetIndex));
        }
        else
        {
            isActionLocked = false;

            EvaluateDoubleDownCondition();
        }
    }

    private int ChooseHandIndex()
    {
        if(!isPlayerStand) return currentHandIndex;

        return Mathf.Max(0, currentHandIndex - 1);
    }

    public void UpdateAlcoholCards()
    {
        playerHands.ForEach(CardEffects.AddAlcoholCardList);
        UpdateUI();
        UpdateCardVFX();
    }

    public IEnumerator FanCoroutine()
    {
        isActionLocked = true;
        isRoundActive = false;
        
        if (dealToDealerCoroutine != null)
        {
            StopCoroutine(dealToDealerCoroutine);
            dealToDealerCoroutine = null;

            yield return null;
        }
        
        yield return StartCoroutine(AnimateCardsOffScreen());

        ClearTable();

        playerHands.Add(new List<CardInstance>());
        handBets.Add(currentBet);
        currentHandIndex = 0;

        OnStartGame();
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

    private IEnumerator CheckPowerballCurrentHand() => eventManager.CheckPowerballAtIndex(currentHandIndex);

    public void GainMoney(int moneyAmount)
    {
        var targetBalance = playerMoney + moneyAmount;
        AudioManager.instance.Play("MoneyGained");
        StartCoroutine(AnimateBetChange(targetBalance, 3f / GameUtils.gameSpeedMultiplier));
    }

    private void ManagerSetup()
    {
        itemManager.SetBlackjackGame(this);
        itemManager.SetShopManager(shopManager);
        
        shopManager.SetBlackjackGame(this);
        
        eventManager.SetBlackjackGame(this);
    }

    #endregion

    #region Keepsakes
    
    public bool ActivateBpMedicine()
    {
        if (!isRoundActive) return false;
        isMedicineActive = true;

        return true;
    }

    public void DeactivateBpMedicine() => isMedicineActive = false;

    private IEnumerator ActivateMedicineCoroutine()
    {
        useAfterStand = true;
        
        yield return new WaitForSeconds(5.0f);
        
        useAfterStand = false;
    }

    public void HandleNewCardInPlayerHand(CardInstance cardInstance)
    {
        StartCoroutine(HandleNewCardInPlayerHandCoroutine(cardInstance));
    }
    
    private IEnumerator HandleNewCardInPlayerHandCoroutine(CardInstance cardInstance)
    {
        yield return PlaceCardInPlayerHandCoroutine(cardInstance);
        
        List<CardInstance> activeHand = playerHands[currentHandIndex];
        int handValue = CalculateHandValue(activeHand, true);
        
        yield return EvaluatePlayerHandValue(activeHand, handValue);
    }

    private IEnumerator EvaluatePlayerHandValue(List<CardInstance> activeHand, int handValue)
    {
        if(activeHand.Count == 7 && handValue <= blackjackGoal)
        {
            statusText.text = "Hand full";
    
            yield return StartCoroutine(CheckPowerballCurrentHand());
            yield return GameUtils.WaitForSecondsScaled(1f);
            yield return StartCoroutine(AdvanceHandCoroutine());
        }
        else
            yield return CalculateBustCoroutine(activeHand, handValue);
    }
    
    public void CalculateBust()
    {
        List<CardInstance> activeHand = playerHands[currentHandIndex];
        int handValue = CalculateHandValue(activeHand, true);
        StartCoroutine(CalculateBustCoroutine(activeHand, handValue));
    }
    
    private IEnumerator CalculateBustCoroutine(List<CardInstance> activeHand, int handValue)
    {
        if(handValue > blackjackGoal || handValue < -blackjackGoal)
        {
            yield return StartCoroutine(BustCheckCoroutine(activeHand, currentHandIndex));
        }
        else
        {
            isActionLocked = false;
        }
    }

    private IEnumerator PlaceCardInPlayerHandCoroutine(CardInstance cardInstance)
    {
        List<CardInstance> currentHand = playerHands[currentHandIndex];
        Transform currentParent = handPositions[currentHandIndex];
        CardInstance newCardInstance = DealCardInstance(cardInstance.cardData, currentHand, false);

        yield return PlaceCardInHand(newCardInstance, currentHand, currentParent, playerCardsOffset);
        UpdateHandVisuals(currentHand, true);
        UpdateSplitOutlines();
    }
    
    private IEnumerator PlaceCardInHand(CardInstance newCardInstance, List<CardInstance> currentHand,
        Transform currentParent, Vector3 offset)
    {
        if(newCardInstance != null)
        {
            int cardOrderIndex = currentHand.Count - 1;
            Vector3 targetLocalPos = offset * cardOrderIndex;
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
            newCardInstance.displayComponent.transform.localScale = cardScaleVector;
    
            UpdateUI(true);
        }
    }
    

    //Keepsake unlock progression.
    public bool CheckItemAfterStand()
    {
        if(isActionLocked && !useAfterStand)
        {
            KeepsakeUnlockProgression.instance.AddStat(ChallengeType.ItemAfterStand);

            return true;
        }

        return false;
    }

    public void AddIneritanceMoney(int amount)
    {
        playerMoney += amount;

        if(playerMoney > maxMoneyThisRun)
        {
            maxMoneyThisRun = playerMoney;
        }

        UpdateBettingUI();
    }

    public void GoFishRank(Card.Rank targetRank, System.Action<bool> onComplete)
    {
        StartCoroutine(GoFishCoroutine(targetRank, onComplete));
    }

    private IEnumerator GoFishCoroutine(Card.Rank targetRank, System.Action<bool> onComplete)
    {
        bool found = false;

        List<CardInstance> stolenCards = new List<CardInstance>();

        for(int i = dealerHand.Count - 1; i >= 0; i--)
        {
            if(dealerHand[i].cardData.rank == targetRank)
            {
                stolenCards.Add(dealerHand[i]);

                dealerHand.RemoveAt(i);

                found = true;
            }
        }

        if(!found)
        {
            onComplete?.Invoke(false);

            yield break;
        }

        isActionLocked = true;

        onComplete?.Invoke(true);

        List<CardInstance> currentHand = playerHands[currentHandIndex];
        Transform currentParent = handPositions[currentHandIndex];

        foreach(CardInstance card in stolenCards)
        {
            if(card.isHidden)
            {
                yield return StartCoroutine(FlipCardCoroutine(card.displayComponent, 0.4f));

                card.isHidden = false;
            }

            currentHand.Insert(0, card);
            card.displayComponent.transform.SetParent(currentParent.parent);

            int cardOrderIndex = currentHand.Count - 1;

            Vector3 targetLocalPos = playerCardsOffset * cardOrderIndex;

            yield return StartCoroutine(CardAnimationCoroutine(
                card.displayComponent.transform,
                currentParent.TransformPoint(targetLocalPos),
                currentParent.rotation,
                cardScaleVector,
                cardAnimationDuration
            ));

            card.displayComponent.transform.SetParent(currentParent);
        }

        bool dealerHidden = false;

        foreach(var c in dealerHand)
        {
            if(c.isHidden) dealerHidden = true;
        }

        UpdateHandVisuals(dealerHand, false);
        UpdateHandVisuals(currentHand, true);
        UpdateUI(dealerHidden);
        CalculateBust();
    }
    #endregion

    #region Event Methods
    public void SelectCursorHand(bool isActive)
    {
        cursorFollow.SetCursorTypeActive(isActive, CursorType.Flip);
        standHandAnimator.gameObject.SetActive(!isActive);
    }

    private IEnumerator ChangePriceCoroutine()
    {
        if(!priceChanged && playerMoney >= percentagePriceThreshold)
        {
            priceChanged = true;

            gameCamera.ChangeToCamera(CameraType.Event);

            AudioManager.instance.Play("Laugh");

            statusText.text = "Let's make it more interesting";

            yield return StartCoroutine(GameUtils.WaitDelayOrInput(5.0f));

            AudioManager.instance.Play("NewEvent");

            statusText.text = "Item prices are scaling";

            yield return StartCoroutine(GameUtils.WaitDelayOrInput(4.0f));

            gameCamera.ChangeToCamera(CameraType.Sitting);

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

    public void ClearTable()
    {
        foreach(GameObject cardObject in activeCardObjects)
            if(cardObject) Destroy(cardObject);

        activeCardObjects.Clear();

        foreach(var hand in playerHands)
        {
            hand.Clear();
        }

        playerHands.Clear();
        handBets.Clear();
        dealerHand.Clear();
        CardEffects.ClearAlcoholCards();
        CardEffects.ClearCutCards();

        if(peekedCardObject != null)
        {
            Destroy(peekedCardObject);

            peekedCardObject = null;
            peekCardInstance = null;
        }
    }

    public void StartGame()
    {
        KeepsakeManager.instance.ResetKeepsake();

        StartCoroutine(ButtonCoroutine());

        ClearTable();
        gameCamera.ChangeToCamera(CameraType.Sitting);
        eventManager.ShowNewPowerballTaunt();

        AudioManager.instance.Play("Shuffle");

        gameDeck.InitializeDeck();
        gameDeck.Shuffle();
        cursorDetection.OnRoundInactive();

        if(!isTutorialActive)
        {
            shopManager.PlaySuitcaseOpen();
            statusText.text = "";
        }
        
        isRoundActive = false;
        isActionLocked = false;
        canDoubleDown = false;
        isSplitting = false;
        tieTauntPlayed = false;
        KnifeItem.isKnifeActive = false;
        ScissorsItem.isScissorsActive = false;
        AcidItem.isAcidActive = false;
        CrucifixItem.isCrucifixActive = false;
        CigarettesItem.isCigaretteActive = false;
        AlcoholItem.isAlcoholActive = false;
        AntiMatter.isAntiMatterActive = false;
        Pyro.isPyroActive = false;
        HatTrick.isHatTrickActive = false;

        ResetTexts();

        //Set bet to the last valid bet
        if(PlayerMoney < minBet) currentBet = PlayerMoney;
        else if(currentBet > PlayerMoney) currentBet = PlayerMoney;
        else if(currentBet < minBet) currentBet = minBet;

        UpdateBettingUI();
    }

    public void ResetTexts()
    {
        foreach(var text in handTotalTexts) text.text = "";
        dealerTotalText.text = "";
        rouletteText.text = "";
    }

    //Locks the bet and starts the round
    private IEnumerator DealRoundCoroutine()
    {
        if(!tutorialCompleted)
        {
            tutorialCompleted = true;
            dialogueSystem.PlayTutorial();

            yield return new WaitWhile(() => dialogueSystem.IsPlaying);
        }

        if(isRoundActive || PlayerMoney < currentBet) yield break;


        isActionLocked = true;
        isRoundActive = true;
        playerHands.Clear();
        playerHands.Add(new List<CardInstance>());
        handBets.Clear();
        handBets.Add(isTutorialActive ? 0 : currentBet);
        currentHandIndex = 0;
        buttonAnimator.SetBool("StartActive", false);

        AudioManager.instance.Play("Button");

        yield return GameUtils.WaitForSecondsScaled(0.5f);
        yield return eventManager.ChangeBlackjackGoal();

        gameCamera.ChangeToCamera(CameraType.Playing);

        cursorDetection.OnRoundActive();
        itemManager.ChangeItemAction(true);

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
            dialogueSystem.ShowPlayerBlackjackTaunt();

            yield return new WaitWhile(() => dialogueSystem.IsPlaying);
            yield return StartCoroutine(CheckPowerballCurrentHand());

            StartCoroutine(DealerTurnCoroutine(true));
        }
        else
        {
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
    private CardInstance DealCardInstance(Card newCardData, List<CardInstance> hand, bool isHidden)
    {
        if(!cardPrefabLookup.TryGetValue((newCardData.rank, newCardData.suit), out GameObject cardPrefabToUse)) return null;

        GameObject cardObject = Instantiate(cardPrefabToUse, deckPosition);

        cardObject.transform.localScale = cardScaleVector;

        activeCardObjects.Add(cardObject);

        CardDisplay cardDisplay = cardObject.GetComponent<CardDisplay>();

        bool isNegative = CardEffects.IsCardNegative(newCardData);
        bool isDoubled = eventManager.CheckIfDoubled(newCardData);
        bool isHalved = eventManager.CheckIfHalved(newCardData);

        cardDisplay?.SetNegativeVisual(isNegative);
        cardDisplay?.SetDoubledVisual(isDoubled);
        cardDisplay?.SetCutVisual(isHalved);
        cardDisplay?.SetHidden(isHidden);

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

        bool isSuitNegative = CardEffects.IsSuitNegative(newCardData.suit);
        bool isDoubled = eventManager.CheckIfDoubled(newCardData);
        bool isHalved = eventManager.CheckIfHalved(newCardData);

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

        if(CrucifixItem.isCrucifixActive)
        {
            int playerValue = CalculateHandValue(currentHand, true);
            int idealValue = blackjackGoal - playerValue;

            Card.Rank targetRank = Card.GetRankForValue(idealValue);
            Card? dealtCard = gameDeck.DealSpecificCard(targetRank);
            
            CrucifixItem.isCrucifixActive = false;

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

        Transform currentParent = handPositions[currentHandIndex];
        
        CardInstance newCardInstance;
        if (peekCardInstance == null)
            newCardInstance = DealCardInstance(newCardData, currentHand, false);
        else
        {
            newCardInstance = peekCardInstance;
            currentHand.Insert(0, newCardInstance);
            peekCardInstance = null;
        }
        
        AudioManager.instance.Play("CardHit");

        yield return PlaceCardInHand(newCardInstance, currentHand, currentParent, playerCardsOffset);
        
        KeepsakeManager.instance.OnDealPlayerCard(newCardInstance);
        
        UpdateHandVisuals(currentHand, true);
        UpdateSplitOutlines();
        deckPosition.position = savedPosition;
    }

    private IEnumerator DealCardToDealerCoroutine(bool isHidden)
    {
        Card newCardData = new Card { rank = Card.Rank.None };

        bool cardFound = false;
        
        if(CrucifixItem.isCrucifixActive)
        {
            int dealerValue = CalculateHandValue(dealerHand, true);
            int idealValue;

            if (dealerValue >= 12) idealValue = 10;
            else if (dealerValue >= 6) idealValue = 16 - dealerValue;
            else idealValue = 12 - dealerValue; 

            Card.Rank targetRank = Card.GetRankForValue(idealValue);
            Card? dealtCard = gameDeck.DealSpecificCard(targetRank);
            
            CrucifixItem.isCrucifixActive = false;

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
        
        CardInstance newCardInstance;
        if (peekCardInstance == null)
            newCardInstance = DealCardInstance(newCardData, dealerHand, isHidden);
        else
        {
            newCardInstance = peekCardInstance;
            dealerHand.Insert(0, newCardInstance);
            peekCardInstance = null;
        }
        AudioManager.instance.Play("CardHit");
        yield return PlaceCardInHand(newCardInstance, dealerHand, dealerCardPosition, dealerCardsOffset);
        UpdateHandVisuals(dealerHand, false);
        UpdateUI();
    }

    private IEnumerator HitCoroutine()
    {
        if(!isRoundActive || isActionLocked) yield break;

        isActionLocked = true;

        bool endlessDouble = KeepsakeManager.instance.AllowEndlessDoubleDown();

        if(!endlessDouble)
        {
            canDoubleDown = false;
        }

        hitHandAnimator.SetTrigger("hitTrigger");

        yield return GameUtils.WaitForSecondsScaled(1f);
        yield return StartCoroutine(DealCardToPlayerCoroutine());

        UpdateUI(true);

        List<CardInstance> activeHand = playerHands[currentHandIndex];
        int handValue = CalculateHandValue(activeHand, true);

        yield return EvaluatePlayerHandValue(activeHand, handValue);
        if (handValue <= blackjackGoal && handValue >= -blackjackGoal && activeHand.Count < 7 && endlessDouble)
            EvaluateDoubleDownCondition();
    }

    private IEnumerator StandCoroutine()
    {
        isPlayerStand = true;
        
        if(!isRoundActive || isActionLocked) yield break;

        isActionLocked = true;
        
        KeepsakeManager.instance.AllowPostStandItem(this);
        if(isMedicineActive) StartCoroutine(ActivateMedicineCoroutine());
        
        standHandAnimator.SetTrigger("standTrigger");

        yield return StartCoroutine(CheckPowerballCurrentHand());

        float standTimer = 0f;

        while(standTimer < 1f)
        {
            if(!isPlayerStand || !isRoundActive) yield break;

            standTimer += Time.deltaTime;

            yield return null;
        }

        if(!isPlayerStand || !isRoundActive) yield break;

        yield return StartCoroutine(AdvanceHandCoroutine());
    }

    private IEnumerator DoubleDownCoroutine()
    {
        if(!isRoundActive || isActionLocked || !canDoubleDown) yield break;

        isActionLocked = true;

        KeepsakeUnlockProgression.instance.AddStat(ChallengeType.DoubleDown);

        bool endlessDouble = KeepsakeManager.instance.AllowEndlessDoubleDown();

        if(!endlessDouble)
        {
            canDoubleDown = false;
        }

        handBets[currentHandIndex] *= 2;

        UpdateBettingUI();

        AudioManager.instance.Play("BetUp");

        hitHandAnimator.SetTrigger("doubleDownTrigger");

        yield return GameUtils.WaitForSecondsScaled(1f);
        yield return StartCoroutine(DealCardToPlayerCoroutine());

        if(!endlessDouble)
        {
            yield return StartCoroutine(AdvanceHandCoroutine());
            yield return StartCoroutine(CheckPowerballCurrentHand());
        }
        else
        {
            UpdateUI(true);

            List<CardInstance> activeHand = playerHands[currentHandIndex];
            int handValue = CalculateHandValue(activeHand, true);
            
            yield return EvaluatePlayerHandValue(activeHand, handValue);
            if (handValue <= blackjackGoal && handValue >= -blackjackGoal && activeHand.Count < 7 && endlessDouble)
                EvaluateDoubleDownCondition();
        }
    }

    private IEnumerator SplitCoroutine()
    {
        isActionLocked = true;

        KeepsakeUnlockProgression.instance.AddStat(ChallengeType.Split);

        int betToAdd = handBets[currentHandIndex];

        AudioManager.instance.Play("BetUp");

        standHandAnimator.SetTrigger("splitTrigger");

        yield return GameUtils.WaitForSecondsScaled(1f);

        List<CardInstance> activeHand = playerHands[currentHandIndex];
        CardInstance cardToMove = activeHand[0];
        cursorDetection.SetCardActive(cardToMove, false);

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

            UpdateHandVisuals(playerHands[i], true);
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

        UpdateHandVisuals(activeHand, true);
        UpdateHandVisuals(newHand, true);

        yield return GameUtils.WaitForSecondsScaled(0.5f);

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
             yield return dealToDealerCoroutine = StartCoroutine(DealerTurnCoroutine());
        }
        else
        {
            yield return GameUtils.WaitForSecondsScaled(1f);

            isActionLocked = false;

            EvaluateDoubleDownCondition();
            UpdateUI();
            UpdateSplitOutlines();
        }
        
        KeepsakeManager.instance.OnAdvanceHand();
    }

    private IEnumerator DealerTurnCoroutine(bool playerHasBlackjack = false)
    {
        cursorDetection.OnDealerTurn();

        foreach(var hand in playerHands)
        {
            foreach(var card in hand)
            {
                card.displayComponent.GetComponentInChildren<ClickableCard>()?.OnRemoveOutline();
            }
        }

        yield return GameUtils.WaitForSecondsScaled(1f);

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

        if(allHandsBust && !KnifeItem.isKnifeActive)
        {
            yield return GameUtils.WaitForSecondsScaled(1f);
        }
        else
        {
            CardInstance hiddenCard = dealerHand.FirstOrDefault(x => x.isHidden);

            if(hiddenCard != null)
            {
                yield return StartCoroutine(FlipCardCoroutine(hiddenCard.displayComponent, 0.4f));

                hiddenCard.isHidden = false;

                UpdateUI(true);

                yield return GameUtils.WaitForSecondsScaled(1f);
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
                    yield return GameUtils.WaitForSecondsScaled(1f);

                    StartCoroutine(EndGameCoroutine("Both have Blackjack. Its a tie"));

                    yield break;
                }
            }

            int dealerAIValue = CalculateHandValue(dealerHand, false);
            IEnumerator DealerHit()
            {
                yield return StartCoroutine(DealCardToDealerCoroutine(false));

                UpdateUI(true);
                dealerAIValue = CalculateHandValue(dealerHand, false);

                yield return GameUtils.WaitForSecondsScaled(1f);
            }
            
            if(Mathf.Abs(dealerAIValue) < (blackjackGoal - 4) && dealerHand.Count < 7)
                yield return DealerHit();

            if(!KnifeItem.isKnifeActive)
                while (Mathf.Abs(dealerAIValue) < (blackjackGoal - 4) && dealerHand.Count < 7)
                    yield return DealerHit();
            
            if(dealerHand.Count == 7)
            {
                statusText.text = "Dealer hand full";
                yield return GameUtils.WaitForSecondsScaled(1f);
            }
        }

        UpdateUI(false);

        yield return StartCoroutine(RevealJokers());

        int finalDealerValue = CalculateHandValue(dealerHand, true);
        int playerValue = CalculateHandValue(playerHands[0], true);
        bool playerBust = playerValue > blackjackGoal || playerValue < -blackjackGoal;
        bool dealerBust = finalDealerValue > GetDealerBustThreshold() || finalDealerValue < -GetDealerBustThreshold();
        int playerDiff = Mathf.Abs(Mathf.Abs(playerValue) - blackjackGoal);
        int dealerDiff = Mathf.Abs(Mathf.Abs(finalDealerValue) - blackjackGoal);
        bool wonByOne = false;

        if(!playerBust && !dealerBust && playerDiff - dealerDiff == 1)
        {
            wonByOne = true;
        }

        if(wonByOne)
        {
            KeepsakeUnlockProgression.instance.AddStat(ChallengeType.LoseByOne);

            dialogueSystem.ShowDealerWinsByOneTaunt();

            yield return new WaitWhile(() => dialogueSystem.IsPlaying);
        }

        if(playerHands.Count > 1)
        {
            for(int i = 0; i < playerHands.Count; i++)
            {
                int finalPlayerValue = CalculateHandValue(playerHands[i], true);
                string resultMessage = DetermineWinner(finalPlayerValue, finalDealerValue);

                yield return StartCoroutine(ProcessPayout(resultMessage, handBets[i], playerHands));
                yield return GameUtils.WaitForSecondsScaled(1f);
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

    private IEnumerator ProcessPayout(string message, int betAmount, List<List<CardInstance>> allHands = null)
    {
        bool shouldPlayBetLostTaunt = false;

        if(message.Contains("tie"))
        {
            if(!tieTauntPlayed)
            {
                dialogueSystem.ShowTieTaunt();

                shouldPlayBetLostTaunt = true;
            }

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
            yield return GameUtils.WaitForSecondsScaled(1f);
            yield break;
        }

        if(message.Contains("You win"))
        {
            TimesWon++;
            CheckSuitWinCondition(allHands);
            CheckThreeOfAKind(allHands);

            targetMoneyBalance = playerMoney + KeepsakeManager.instance.ApplyPayoutModifiers(betAmount, allHands);

            AudioManager.instance.Play("MoneyGained");

            Instantiate(greenParticlePrefab, particleSpawnPoint.position, particleSpawnPoint.rotation);

            yield return StartCoroutine(AnimateBetChange(targetMoneyBalance, 3f / GameUtils.gameSpeedMultiplier));
        }
        else if(message.Contains("Dealer wins") || message.Contains("Bust"))
        {
            TimesLost ++;
            if(!OrganBagItem.isOrganActive)
            {
                targetMoneyBalance = playerMoney - betAmount;

                AudioManager.instance.Play("MoneyLost");

                Instantiate(redParticlePrefab, particleSpawnPoint.position, particleSpawnPoint.rotation);

                standHandAnimator.SetTrigger("flipperTrigger");

                yield return StartCoroutine(AnimateBetChange(targetMoneyBalance, 3f / GameUtils.gameSpeedMultiplier));
            }
            else
            {
                AudioManager.instance.Play("MoneyLost");

                standHandAnimator.SetTrigger("flipperTrigger");

                yield return GameUtils.WaitForSecondsScaled(0.5f);

                // TODO: move to organ item class
                AudioManager.instance.Play("OrganExpire");
                shopManager.RemoveFromInventory(ItemType.Organ);
                OrganBagItem.isOrganActive = false;
                
                targetMoneyBalance = playerMoney;

                yield return GameUtils.WaitForSecondsScaled(1f);
            }
        }
        else
        {
            targetMoneyBalance = playerMoney;

            yield return GameUtils.WaitForSecondsScaled(1f);
        }

        if(shouldPlayBetLostTaunt)
        {
            dialogueSystem.ShowBetLostTaunt();

            yield return new WaitWhile(() => dialogueSystem.IsPlaying);
        }
    }

    private void CheckSuitWinCondition(List<List<CardInstance>> allHands)
    {
        bool allRed = true;
        bool allBlack = true;

        foreach(var hand in allHands)
        {
            foreach(var card in hand)
            {
                if(card.cardData.suit == Card.Suit.Hearts || card.cardData.suit == Card.Suit.Diamonds)
                {
                    allBlack = false;
                }
                else if(card.cardData.suit == Card.Suit.Spades || card.cardData.suit == Card.Suit.Clubs)
                {
                    allRed = false;
                }
            }
        }

        if(allRed)
        {
            KeepsakeUnlockProgression.instance.AddStat(ChallengeType.WinRedSuits);
        }

        if(allBlack)
        {
            KeepsakeUnlockProgression.instance.AddStat(ChallengeType.WinBlackSuits);
        }
    }

    private void CheckThreeOfAKind(List<List<CardInstance>> allHands)
    {
        foreach(var hand in allHands)
        {
            Dictionary<int, int> valueCounts = new Dictionary<int, int>();

            foreach(var card in hand)
            {
                int val = card.cardData.GetValue();

                if(!valueCounts.ContainsKey(val)) valueCounts[val] = 0;

                valueCounts[val]++;

                if(valueCounts[val] >= 3)
                {
                    KeepsakeUnlockProgression.instance.AddStat(ChallengeType.ThreeOfAKind);

                    return;
                }
            }
        }
    }

    private IEnumerator BustCheckCoroutine(List<CardInstance> activeHand, int handIndex)
    {
        yield return StartCoroutine(CheckPowerballCurrentHand());
        yield return GameUtils.WaitForSecondsScaled(1f);
        List<Coroutine> dissolveCoroutines = new List<Coroutine>();

        var playerJokers = activeHand.Where(c => c.cardData.rank == Card.Rank.Joker).ToList();
        foreach(CardInstance card in activeHand)
            if(card.cardData.rank == Card.Rank.Joker)
                dissolveCoroutines.Add(CreateRealJokerCard(card, handPositions[handIndex]));
        
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

            yield return GameUtils.WaitForSecondsScaled(1f);
        }

        currentBustCoroutine = null;

        if(playerHands.Count == 1)
        {
            yield return StartCoroutine(EndGameCoroutine("Bust... You lose"));
        }
        else
        {
            yield return GameUtils.WaitForSecondsScaled(1f);
            yield return StartCoroutine(AdvanceHandCoroutine());
        }
        foreach(Coroutine coroutine in dissolveCoroutines)
        {
            yield return coroutine;
        }
    }

    private string DetermineWinner(int playerValue, int dealerValue)
    {
        bool playerBust = (playerValue > blackjackGoal || playerValue < -blackjackGoal);
        bool dealerBust = (dealerValue > GetDealerBustThreshold() || dealerValue < -GetDealerBustThreshold());
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
        cursorDetection.OnDealerTurn();

        int activeBetAmount = (handBets != null && handBets.Count > 0) ? handBets[0] : currentBet;

        yield return StartCoroutine(ProcessPayout(message, activeBetAmount, playerHands));
        yield return StartCoroutine(EndRoundSequence());
    }
    #endregion

    #region Card Visuals
    
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
        foreach(var hand in playerHands)
        {
            foreach(CardInstance card in hand)
            {
                bool isNegative = CardEffects.IsCardNegative(card.cardData);
                bool isDoubled = eventManager.CheckIfDoubled(card.cardData) || AlcoholItem.isAlcoholActive;
                bool isHalved = eventManager.CheckIfHalved(card.cardData) || CardEffects.IsCardCut(card);

                card.displayComponent.SetNegativeVisual(isNegative);
                card.displayComponent.SetDoubledVisual(isDoubled);
                card.displayComponent.SetCutVisual(isHalved);
            }
        }

        foreach(CardInstance card in dealerHand)
        {
            bool isNegative = CardEffects.IsCardNegative(card.cardData);
            bool isDoubled = eventManager.CheckIfDoubled(card.cardData);
            bool isHalved = eventManager.CheckIfHalved(card.cardData) || CardEffects.IsCardCut(card);

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

                    bool isNegative = CardEffects.IsCardNegative(cardData);
                    bool isDoubled = eventManager.CheckIfDoubled(cardData) || AlcoholItem.isAlcoholActive;
                    bool isHalved = eventManager.CheckIfHalved(cardData);

                    display.SetNegativeVisual(isNegative);
                    display.SetDoubledVisual(isDoubled);
                    display.SetCutVisual(isHalved);
                }
            }
        }
    }

    //Animates a card moving from the deck to its position in the hand.
    public void DrawCardAnimation(Transform cardTransform, Vector3 targetPosition, Quaternion targetRotation,
        Vector3 targetScale, float duration)
    {
        StartCoroutine(CardAnimationCoroutine(cardTransform, targetPosition, targetRotation, targetScale, duration));
    }
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

        if(!CigarettesItem.isCigaretteActive)
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
                    else clickable.OnRemoveOutline(false);
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
    public int CalculateHandValue(List<CardInstance> hand, bool countJoker)
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
                cardValue = eventManager.IsAceRule(AceValueRule.Always1) ? 1 : 11;
            }
            else if(card.rank >= Card.Rank.Ten && card.rank <= Card.Rank.King)
            {
                cardValue = 10;
            }
            else
            {
                cardValue = (int)card.rank;
            }

            if(eventManager.IsDoubleLowActive && card.rank != Card.Rank.Joker)
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

            if(eventManager.IsHalfHighActive && card.rank != Card.Rank.Joker)
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

            if(CardEffects.IsCardNegative(card))
            {
                cardValue = -cardValue;
                valueAsOne = -valueAsOne;
            }

            if(CardEffects.cutCards.TryGetValue(cardInstance, out int reduction))
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

            if(CardEffects.IsCardDrunk(cardInstance) && card.rank != Card.Rank.Joker)
            {
                cardValue *= 2;
                valueAsOne *= 2;
            }

            if(card.rank == Card.Rank.Ace && eventManager.IsAceRule(AceValueRule.Flexible))
            {
                aceReductions.Add(Mathf.Abs(cardValue - valueAsOne));
            }

            value += cardValue;
        }

        if(eventManager.IsAceRule(AceValueRule.Flexible))
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
        int handIndex = 0;
        
        foreach(var hand in playerHands)
        {
            allPlayerJokers.AddRange(hand.Where(c => c.cardData.rank == Card.Rank.Joker));
            foreach(CardInstance card in hand)
                if(card.cardData.rank == Card.Rank.Joker)
                    CreateRealJokerCard(card,handPositions[handIndex]);
            
            handIndex++;
        }
        
        var dealerJokers = dealerHand.Where(c => c.cardData.rank == Card.Rank.Joker).ToList();
        foreach(CardInstance card in dealerHand)
            if(card.cardData.rank == Card.Rank.Joker)
                CreateRealJokerCard(card,dealerCardPosition);      
        
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

            yield return StartCoroutine(GameUtils.WaitDelayOrInput(4f));
        }
        else
        {
            yield return StartCoroutine(GameUtils.WaitDelayOrInput(1.5f));
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
        itemManager.OnRoundEnd();

        if(playerMoney <= 0)
        {
            bool isConsumed = KeepsakeManager.instance.ConsumeKeepsake();

            if(isConsumed)
            {
                dialogueSystem.ShowTrustFundTaunt();
                targetMoneyBalance = 500;

                yield return StartCoroutine(AnimateBetChange(500, 3f / GameUtils.gameSpeedMultiplier));
                yield return GameUtils.WaitForSecondsScaled(1f);
            }
        }
        else
        {
            int passiveIncome = KeepsakeManager.instance.GetPassiveIncome();

            if(passiveIncome > 0)
            {
                targetMoneyBalance = playerMoney + passiveIncome;

                yield return StartCoroutine(AnimateBetChange(targetMoneyBalance, 3f / GameUtils.gameSpeedMultiplier));
            }
        }

        isRoundActive = false;
        cursorDetection.OnRoundInactive();
        itemManager.ChangeItemAction(false);

        if(roundsCompleted == tutorialRoundsLimit - 1)
        {
            gameCamera.ChangeToCamera(CameraType.Event);

            AudioManager.instance.Play("Laugh");

            statusText.text = "Lets raise the stakes...";

            yield return StartCoroutine(GameUtils.WaitDelayOrInput(5.0f));

            AudioManager.instance.Play("NewEvent");

            statusText.text = "Betting enabled";

            yield return StartCoroutine(GameUtils.WaitDelayOrInput(5.0f));

            gameCamera.ChangeToCamera(CameraType.Sitting);

            betUpCollider.enabled = true;
            betDownCollider.enabled = true;
        }

        roundsCompleted++;
        eventManager.UpdateTurnsLeft();

        yield return eventManager.CheckForEventTrigger();

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
            CardEffects.Reset();
            PlayerPrefs.SetInt("PreviousRunMoney", maxMoneyThisRun);
            PlayerPrefs.SetInt("PreviousRunWins", TimesWon);
            PlayerPrefs.SetInt("PreviousRunLoss", TimesLost);
            PlayerPrefs.Save();
            KeepsakeUnlockProgression.instance.EndRun();
            
            FadeInAnimator.SetTrigger("fadeInTrig");
            yield return StartCoroutine(GameUtils.WaitDelayOrInput(3.0f));
            SceneManager.LoadSceneAsync(2);
            yield break;
        }

        yield return eventManager.CheckTurnLimit();

        if(PlayerMoney >= 100000 && stayed == false)
        {

            StartCoroutine(LeaveOrStay());

        }

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

        bool hasEnoughMoney = playerMoney >= (totalBets + handBets[currentHandIndex]);
        bool allowsOverdraft = KeepsakeManager.instance.AllowOverdraft();
        bool keepsakeAllowsSplit = KeepsakeManager.instance.AllowAnySplit();
        bool validSplit = val1 == val2 || keepsakeAllowsSplit;
        bool validFunds = hasEnoughMoney || allowsOverdraft;

        return validSplit && validFunds;
    }

    private float GetCardValueForSplit(Card card)
    {
        float cardValue;

        if(card.rank >= Card.Rank.Ten && card.rank <= Card.Rank.King) cardValue = 10;
        else if(card.rank == Card.Rank.Ace) cardValue = 11;
        else cardValue = (int)card.rank;

        if(eventManager.IsDoubleLowActive && cardValue < 6 && card.rank != Card.Rank.Joker) cardValue *= 2;

        if(eventManager.IsHalfHighActive && cardValue > 5 && card.rank != Card.Rank.Joker) cardValue = Mathf.CeilToInt(cardValue / 2f);

        return cardValue;
    }

    public void EvaluateDoubleDownCondition()
    {
        if(currentHandIndex >= playerHands.Count)
        {
            canDoubleDown = false;

            return;
        }

        int totalBets = 0;

        foreach(int b in handBets) totalBets += b;

        bool hasEnoughMoney = playerMoney >= (totalBets + handBets[currentHandIndex]);
        bool allowsOverdraft = KeepsakeManager.instance.AllowOverdraft();

        canDoubleDown = hasEnoughMoney || allowsOverdraft;
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

    private int GetDealerBustThreshold()
    {
        return blackjackGoal - KeepsakeManager.instance.GetDealerBustModifier();
    }

    private IEnumerator LeaveOrStay()
    {
        dialogueSystem.playCashOutText();
        yield return new WaitWhile(() => dialogueSystem.IsPlaying);
        cursorDetection.OnDealerTurn();
        leavebutton.gameObject.SetActive(true);
        staybutton.gameObject.SetActive(true);
    }
    
    public void Leave()
    {
        PlayerPrefs.SetInt("PreviousRunMoney", maxMoneyThisRun);
        PlayerPrefs.SetInt("PreviousRunWins",TimesWon);
        PlayerPrefs.SetInt("PreviousRunLoss",TimesLost);
        PlayerPrefs.Save();
        KeepsakeUnlockProgression.instance.AddStat(ChallengeType.CashOut);
        KeepsakeUnlockProgression.instance.EndRun();
        FadeInAnimator.SetTrigger("fadeInTrig");
        if(playerMoney >= 1000000)
        {
            KeepsakeUnlockProgression.instance.AddStat(ChallengeType.Millionaire);
        }
        CardEffects.Reset();
        SceneManager.LoadSceneAsync(4);
    }
    
    public void Stay()
    {
        leavebutton.gameObject.SetActive(false);
        staybutton.gameObject.SetActive(false);
        cursorDetection.OnRoundInactive();
        stayed = true;
    }
    
    private Coroutine CreateRealJokerCard(CardInstance card, Transform parent)
    {
        int realValue = card.jokerValue;
        if(card.jokerValue > 11 || card.jokerValue < -11)
            realValue = realValue / 2;
        
        if(card.jokerValue != 0)
        {
            cardPrefabLookup.TryGetValue((Card.GetRankForValue(Mathf.Abs(realValue)),card.cardData.suit), out GameObject realCard);
            GameObject realCardObject = Instantiate(realCard,card.CardObject.transform.position,card.CardObject.transform.rotation,parent);
            
            if(card.jokerValue < 0)
                realCardObject.GetComponent<CardDisplay>().SetNegativeVisual(true);

            if(card.jokerValue > 11 || card.jokerValue < -11)
                realCardObject.GetComponent<CardDisplay>().SetDoubledVisual(true);

            activeCardObjects.Add(realCardObject);      
        }
        
        return CardEffects.SetDissolvedVisual(card.displayComponent, 2.0f, Color.aliceBlue,1.2f);                
    }

}
