using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

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
    [SerializeField] private TableCards tableCards;
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
    private Coroutine dealerTurnCoroutine = null;
    public static int blackjackGoal = 21;
    private int roundsCompleted = 0;
    private int maxMoneyThisRun = 0;
    [HideInInspector] public bool isSplitting = false;
    [HideInInspector] public bool isActionLocked = false;
    private bool isMedicineActive = false;
    private bool useAfterStand = false;
    private bool tutorialCompleted = false;
    private bool hasSeenSplitTutorial = false;
    private bool hasSeenDoubleDownTutorial = false;
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
    [SerializeField] public ParticleSystem smokeParticle;
    [SerializeField] public Animator bottleAnimation;

    [Header("Visual Setup")]
    [SerializeField] private List<TMPro.TextMeshProUGUI> handTotalTexts;

    private List<int> handBets = new();
    private bool isPlayerStand;

    #endregion

    #region Getters & Setters
    public int GetPlayerMoney() => playerMoney;
    
    public Transform CardOptionPosition => cursorDetection.GetCardOptionsPosition();
    public DialogueSystem DialogueSystem => dialogueSystem;
    public EventManager EventManager => eventManager;
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
    public int CurrentBet => currentBet;
    public int TargetMoneyBalance => targetMoneyBalance;
    public GameCamera GameCamera => gameCamera;
    public TableCards TableCards => tableCards;
    public void SetPlayerStand(bool isStand) => isPlayerStand = isStand;
    #endregion

    #region Monobehaviour Methods

    private void Start()
    {
        maxMoneyThisRun = playerMoney;
        blackjackGoal = 21;

        ManagerSetup();
        ResetGame();

        AudioManager.instance.Play("MainTheme");
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

    public void GainMoney(int moneyAmount)
    {
        var targetBalance = playerMoney + moneyAmount;
        AudioManager.instance.Play("MoneyGained");
        StartCoroutine(AnimateBetChange(targetBalance, 3f / GameUtils.gameSpeedMultiplier));
    }

    private void ManagerSetup()
    {
        itemManager.SetBlackjackGame(this);
        shopManager.SetBlackjackGame(this);
        eventManager.SetBlackjackGame(this);
        tableCards.SetBlackjackGame(this);
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
        yield return tableCards.PlaceCardInPlayerHandCoroutine(cardInstance);
        
        int handValue = tableCards.CalculateHandValue(tableCards.CurrentHand, true);
        
        yield return EvaluatePlayerHandValue(tableCards.CurrentHand, handValue);
    }

    private IEnumerator EvaluatePlayerHandValue(List<CardInstance> activeHand, int handValue)
    {
        if(activeHand.Count == 7 && handValue <= blackjackGoal)
        {
            statusText.text = "Hand full";
    
            yield return StartCoroutine(eventManager.CheckPowerballCompletion());
            yield return GameUtils.WaitForSecondsScaled(1f);
            yield return StartCoroutine(AdvanceHandCoroutine());
        }
        else
            yield return CalculateBustCoroutine(activeHand, handValue);
    }
    
    public void CalculateBust()
    {
        int handValue = tableCards.CalculateHandValue(tableCards.CurrentHand, true);
        StartCoroutine(CalculateBustCoroutine(tableCards.CurrentHand, handValue));
    }
    
    private IEnumerator CalculateBustCoroutine(List<CardInstance> activeHand, int handValue)
    {
        if(handValue > blackjackGoal || handValue < -blackjackGoal)
        {
            yield return StartCoroutine(BustCheckCoroutine(activeHand));
        }
        else
        {
            isActionLocked = false;
            EvaluateDoubleDownCondition();
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

    public void AddInheritanceMoney(int amount)
    {
        playerMoney += amount;

        if(playerMoney > maxMoneyThisRun)
        {
            maxMoneyThisRun = playerMoney;
        }

        UpdateBettingUI();
    }
    #endregion

    #region Event Methods
    
    // TODO: move to other class
    private IEnumerator RevealJokers()
    {
        var (allPlayerJokers, _) = tableCards.CreatePlayerJokers();
        var (dealerJokers, _) = tableCards.CreateDealerJokers();
        
        string revealMessage = "";
        revealMessage += GetJokersText(allPlayerJokers, true);
        revealMessage += GetJokersText(dealerJokers, false);

        if(!string.IsNullOrEmpty(revealMessage))
        {
            statusText.text = revealMessage;
            yield return StartCoroutine(GameUtils.WaitDelayOrInput(4f));
        }
        else
            yield return StartCoroutine(GameUtils.WaitDelayOrInput(1.5f));
    }

    private string GetJokersText(List<CardInstance> jokers, bool isPlayer)
    {
        if (jokers == null || jokers.Count == 0) return "";
        
        string revealMessage = isPlayer ? "Your" : "Dealer's";
        revealMessage += jokers.Count > 1 ? " Jokers: " : " Joker: ";
        revealMessage += string.Join(", ", jokers.Select(j => j.cardData.jokerValue.ToString()));
        revealMessage += ". ";

        return revealMessage;
    }
    
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

    // used for fan item
    public void ResetToSingleBet()
    {
        handBets.Clear();
        handBets.Add(currentBet);
    }

    public void ResetGame()
    {
        KeepsakeManager.instance.ResetKeepsake();

        StartCoroutine(ButtonCoroutine());

        handBets.Clear();
        tableCards.ClearTable();
        gameCamera.ChangeToCamera(CameraType.Sitting);
        eventManager.ShowNewPowerballTaunt();

        tableCards.ShuffleCards();
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
        
        itemManager.DeactivateItems();
        KeepsakeManager.instance.DeactivateKeepsakes();

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

    public IEnumerator StopDealerTurn()
    {
        if (dealerTurnCoroutine == null) yield break;
        
        StopCoroutine(dealerTurnCoroutine);
        dealerTurnCoroutine = null;
        isPlayerStand = false;
    
        yield return null;
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
        handBets.Clear();
        handBets.Add(isTutorialActive ? 0 : currentBet);
        buttonAnimator.SetBool("StartActive", false);

        AudioManager.instance.Play("Button");

        yield return GameUtils.WaitForSecondsScaled(0.5f);
        yield return eventManager.ChangeBlackjackGoal();

        gameCamera.ChangeToCamera(CameraType.Playing);

        cursorDetection.OnRoundActive();
        itemManager.ChangeItemAction(true);

        bool isRiggedHand = roundsCompleted < riggedRoundsLimit;
        yield return tableCards.DealRoundCoroutine(isRiggedHand);

        if(IsBlackjack(tableCards.CalculateHandValue(tableCards.PlayerHands[0], true)))
        {
            canDoubleDown = false;
            dialogueSystem.ShowPlayerBlackjackTaunt();

            yield return new WaitWhile(() => dialogueSystem.IsPlaying);
            yield return StartCoroutine(eventManager.CheckPowerballCompletion());

            dealerTurnCoroutine = StartCoroutine(DealerTurnCoroutine(true));
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
        yield return StartCoroutine(tableCards.DealCardToPlayerCoroutine());

        UpdateUI();

        // TODO: change stuff here
        int handValue = tableCards.CalculateHandValue(tableCards.CurrentHand, true);

        yield return EvaluatePlayerHandValue(tableCards.CurrentHand, handValue);
        if (handValue <= blackjackGoal && handValue >= -blackjackGoal && 
            !tableCards.IsPlayerHandFull && endlessDouble)
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

        yield return StartCoroutine(eventManager.CheckPowerballCompletion());

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

        handBets[tableCards.CurrentHandIndex] *= 2;

        UpdateBettingUI();

        AudioManager.instance.Play("BetUp");

        hitHandAnimator.SetTrigger("doubleDownTrigger");

        yield return GameUtils.WaitForSecondsScaled(1f);
        yield return StartCoroutine(tableCards.DealCardToPlayerCoroutine());

        if(!endlessDouble)
        {
            yield return StartCoroutine(AdvanceHandCoroutine());
            yield return StartCoroutine(eventManager.CheckPowerballCompletion());
        }
        else
        {
            UpdateUI(true);

            int handValue = tableCards.CalculateHandValue(tableCards.CurrentHand, true);
            
            yield return EvaluatePlayerHandValue(tableCards.CurrentHand, handValue);
            if (handValue <= blackjackGoal && handValue >= -blackjackGoal && !tableCards.IsPlayerHandFull && endlessDouble)
                EvaluateDoubleDownCondition();
        }
    }

    private IEnumerator SplitCoroutine()
    {
        isActionLocked = true;

        KeepsakeUnlockProgression.instance.AddStat(ChallengeType.Split);

        AudioManager.instance.Play("BetUp");

        standHandAnimator.SetTrigger("splitTrigger");

        yield return GameUtils.WaitForSecondsScaled(1f);

        int betToAdd = handBets[tableCards.CurrentHandIndex];
        handBets.Insert(tableCards.CurrentHandIndex + 1, betToAdd);
        UpdateBettingUI();

        yield return tableCards.SplitCardsCoroutine();

        isActionLocked = false;

        EvaluateDoubleDownCondition();
        UpdateUI();
    }

    private IEnumerator AdvanceHandCoroutine()
    {
        tableCards.GoNextHand();

        if(!tableCards.IsPlayerTurn)
        {
             yield return dealerTurnCoroutine = StartCoroutine(DealerTurnCoroutine());
        }
        else
        {
            yield return GameUtils.WaitForSecondsScaled(1f);

            isActionLocked = false;
            isPlayerStand = false; // maybe here

            EvaluateDoubleDownCondition();
            UpdateUI();
            tableCards.UpdateSplitOutlines();
        }
        
        KeepsakeManager.instance.OnAdvanceHand();
    }

    // TODO: change stuff here too
    private IEnumerator DealerTurnCoroutine(bool playerHasBlackjack = false)
    {
        cursorDetection.OnDealerTurn();

        // TODO: maybe move to cursor
        foreach(var hand in tableCards.PlayerHands)
        {
            foreach(var card in hand)
            {
                card.displayComponent.GetComponentInChildren<ClickableCard>()?.OnRemoveOutline();
            }
        }

        yield return GameUtils.WaitForSecondsScaled(1f);

        bool allHandsBust = true;

        foreach(var hand in tableCards.PlayerHands)
        {
            int val = tableCards.CalculateHandValue(hand, true);

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
            yield return tableCards.FlipDealerHiddenCard();

            int dealerValueInit = tableCards.CalculateDealerHandValue(false);

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

                yield return GameUtils.WaitForSecondsScaled(1f);

                StartCoroutine(EndGameCoroutine("Both have Blackjack. Its a tie"));

                yield break;
            }

            int dealerAIValue = tableCards.CalculateDealerHandValue(false);
            IEnumerator DealerHit()
            {
                yield return StartCoroutine(tableCards.DealCardToDealerCoroutine(false));

                dealerAIValue = tableCards.CalculateDealerHandValue(false);
                UpdateUI();

                yield return GameUtils.WaitForSecondsScaled(1f);
            }
            
            if(Mathf.Abs(dealerAIValue) < (blackjackGoal - 4) && !tableCards.IsDealerHandFull)
                yield return DealerHit();

            if(!KnifeItem.isKnifeActive)
                while (Mathf.Abs(dealerAIValue) < (blackjackGoal - 4) && !tableCards.IsDealerHandFull)
                    yield return DealerHit();
            
            if(!tableCards.IsDealerHandFull)
            {
                statusText.text = "Dealer hand full";
                yield return GameUtils.WaitForSecondsScaled(1f);
            }
        }

        UpdateUI(false);

        yield return StartCoroutine(RevealJokers());

        int finalDealerValue = tableCards.CalculateDealerHandValue(true);
        int playerValue = tableCards.CalculateHandValue(tableCards.PlayerHands[0], true);
        bool playerBust = playerValue > blackjackGoal || playerValue < -blackjackGoal;
        bool dealerBust = finalDealerValue > GetDealerBustThreshold() || finalDealerValue < -GetDealerBustThreshold();
        int playerDiff = Mathf.Abs(Mathf.Abs(playerValue) - blackjackGoal);
        int dealerDiff = Mathf.Abs(Mathf.Abs(finalDealerValue) - blackjackGoal);
        bool wonByOne = !playerBust && !dealerBust && playerDiff - dealerDiff == 1;

        if(wonByOne)
        {
            KeepsakeUnlockProgression.instance.AddStat(ChallengeType.LoseByOne);

            dialogueSystem.ShowDealerWinsByOneTaunt();

            yield return new WaitWhile(() => dialogueSystem.IsPlaying);
        }

        if(tableCards.PlayerHandsCount > 1)
        {
            for(int i = 0; i < tableCards.PlayerHandsCount; i++)
            {
                int finalPlayerValue = tableCards.CalculateHandValue(tableCards.PlayerHands[i], true);
                string resultMessage = DetermineWinner(finalPlayerValue, finalDealerValue);

                yield return StartCoroutine(ProcessPayout(resultMessage, handBets[i], tableCards.PlayerHands));
                yield return GameUtils.WaitForSecondsScaled(1f);
            }

            yield return StartCoroutine(EndRoundSequence());
        }
        else
        {
            int finalPlayerValue = tableCards.CalculateHandValue(tableCards.PlayerHands[0], true);
            string resultMessage = DetermineWinner(finalPlayerValue, finalDealerValue);

            yield return StartCoroutine(EndGameCoroutine(resultMessage));
        }
    }

    private IEnumerator ProcessPayout(string message, int betAmount, List<List<CardInstance>> allHands = null)
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
            yield return GameUtils.WaitForSecondsScaled(1f);
            yield break;
        }

        if(message.Contains("You win"))
        {
            KeepsakeUnlockProgression.instance.CheckSuitWinCondition(allHands);
            KeepsakeUnlockProgression.instance.CheckThreeOfAKind(allHands);

            targetMoneyBalance = playerMoney + KeepsakeManager.instance.ApplyPayoutModifiers(betAmount, allHands);

            AudioManager.instance.Play("MoneyGained");

            Instantiate(greenParticlePrefab, particleSpawnPoint.position, particleSpawnPoint.rotation);

            yield return StartCoroutine(AnimateBetChange(targetMoneyBalance, 3f / GameUtils.gameSpeedMultiplier));
        }
        else if(message.Contains("Dealer wins") || message.Contains("Bust"))
        {
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

                OrganBagItem.Expire(shopManager);
                
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

    private IEnumerator BustCheckCoroutine(List<CardInstance> activeHand)
    {
        yield return StartCoroutine(eventManager.CheckPowerballCompletion());
        yield return GameUtils.WaitForSecondsScaled(1f);

        var (playerJokers, coroutines) = 
            tableCards.CreateJokers(activeHand, tableCards.CurrentHandPosition);
        string revealMessage = GetJokersText(playerJokers, true);

        if(!string.IsNullOrEmpty(revealMessage))
        {
            statusText.text = revealMessage;

            yield return GameUtils.WaitForSecondsScaled(1f);
        }

        currentBustCoroutine = null;

        if(tableCards.PlayerHands.Count == 1)
        {
            yield return StartCoroutine(EndGameCoroutine("Bust... You lose"));
        }
        else
        {
            yield return GameUtils.WaitForSecondsScaled(1f);
            yield return StartCoroutine(AdvanceHandCoroutine());
        }
        
        foreach(var coroutine in coroutines) yield return coroutine;
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

        yield return StartCoroutine(ProcessPayout(message, activeBetAmount, tableCards.PlayerHands));
        yield return StartCoroutine(EndRoundSequence());
    }
    #endregion

    #region Card Visuals

    private IEnumerator ButtonCoroutine()
    {
        buttonAnimator.SetBool("StartActive", true);

        yield return new WaitForSeconds(1f);

        AudioManager.instance.Play("ButtonOpen");
    }
    #endregion

    //Updates the score, money, and checks for busts.
    public void UpdateUI(bool dealerHidden = true)
    {
        bool revealJokers = !dealerHidden;

        if(handTotalTexts != null && handTotalTexts.Count > 0)
        {
            for(int i = 0; i < handTotalTexts.Count; i++)
            {
                if(!handTotalTexts[i]) continue;

                if(i < tableCards.PlayerHands.Count)
                {
                    string prefix = tableCards.PlayerHands.Count > 1 ? $"" : "";

                    handTotalTexts[i].text = FormatHandText(prefix, tableCards.PlayerHands[i], revealJokers, false);
                }
                else
                {
                    handTotalTexts[i].text = "";
                }
            }
        }

        if(dealerTotalText)
        {
            if(tableCards.DealerHand.Count > 0)
            {
                if(dealerHidden && tableCards.DealerHand.Any(c => c.isHidden))
                {
                    List<CardInstance> visibleCards = tableCards.DealerHand.Where(x => !x.isHidden).ToList();
                    dealerTotalText.text = FormatHandText("", visibleCards, revealJokers, true);
                }
                else
                {
                    dealerTotalText.text = FormatHandText("", tableCards.DealerHand, revealJokers, false);
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

        int totalValue = tableCards.CalculateHandValue(cards, true);
        bool hasJoker = cards.Any(c => c.cardData.rank == Card.Rank.Joker);

        if(revealJokers || !hasJoker)
        {
            return prefix + (dealerHasHiddenCard ? $"{totalValue}" : totalValue.ToString());
        }

        int baseValue = tableCards.CalculateHandValue(cards, false);

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
            PlayerPrefs.SetInt("PreviousRunMoney", maxMoneyThisRun);
            PlayerPrefs.Save();
            KeepsakeUnlockProgression.instance.EndRun();
            SceneManager.LoadSceneAsync(3);

            yield break;
        }

        yield return eventManager.CheckTurnLimit();

        if(PlayerMoney >= 100000 && stayed == false)
            StartCoroutine(LeaveOrStay());

        ResetGame();
    }

    private bool IsBlackjack(int handValue)
    {
        if(handValue == blackjackGoal || handValue == -blackjackGoal) return true;

        return false;
    }

    public int CalculateIdealNextValue(bool isForPlayer, int currentValue)
    {
        if (isForPlayer) return blackjackGoal - currentValue;
        
        var idealValue = 10;
        if (currentValue >= blackjackGoal - 15 && currentValue < blackjackGoal - 9)
            idealValue = blackjackGoal - 5 - currentValue;

        return idealValue;
    }

    public bool CanSplit()
    {
        if(!isRoundActive || isActionLocked || tableCards.AreSplitHandsFull) return false;

        List<CardInstance> currentHand = tableCards.CurrentHand;

        if(currentHand.Count != 2) return false;

        int totalBets = 0;

        foreach(int b in handBets) totalBets += b;

        bool hasEnoughMoney = playerMoney >= (totalBets + handBets[tableCards.CurrentHandIndex]);
        bool allowsOverdraft = KeepsakeManager.instance.AllowOverdraft();
        bool keepsakeAllowsSplit = KeepsakeManager.instance.AllowAnySplit();
        bool validSplit = tableCards.IsPlayerHandEqual() || keepsakeAllowsSplit;
        bool validFunds = hasEnoughMoney || allowsOverdraft;

        return validSplit && validFunds;
    }

    public void EvaluateDoubleDownCondition()
    {
        if(!tableCards.IsPlayerTurn)
        {
            canDoubleDown = false;
            return;
        }

        int totalBets = 0;

        foreach(int b in handBets) totalBets += b;

        bool hasEnoughMoney = playerMoney >= (totalBets + handBets[tableCards.CurrentHandIndex]);
        bool allowsOverdraft = KeepsakeManager.instance.AllowOverdraft();

        canDoubleDown = hasEnoughMoney || allowsOverdraft;
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
        PlayerPrefs.Save();
        KeepsakeUnlockProgression.instance.AddStat(ChallengeType.CashOut);
        KeepsakeUnlockProgression.instance.EndRun();

        if(playerMoney >= 1000000)
        {
            KeepsakeUnlockProgression.instance.AddStat(ChallengeType.Millionaire);
        }

        SceneManager.LoadSceneAsync(2);
    }
    
    public void Stay()
    {
        leavebutton.gameObject.SetActive(false);
        staybutton.gameObject.SetActive(false);
        cursorDetection.OnRoundInactive();
        stayed = true;
    }
}
