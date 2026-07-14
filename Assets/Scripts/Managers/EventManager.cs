using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Utils;
using Random = UnityEngine.Random;

[System.Serializable]
public class Threshold
{
    public List<BlackjackEvent> events;

    public int moneyAmount;

    public int maxTurns;
}
public enum AceValueRule { Flexible, Always1, Always11 }

public class EventManager : MonoBehaviour
{
    [SerializeField] private bool useTurnLimit = false;
    [SerializeField] private List<EventThreshold> eventThresholds;
    [SerializeField] private List<BlackjackEvent> lowSeverityEvents;
    [SerializeField] private List<BlackjackEvent> mediumSeverityEvents;
    [SerializeField] private List<BlackjackEvent> highSeverityEvents;
    
    [Header("Update progress display")]
    public UnityEvent ChangeProgressText;
    public UnityEvent UpdatePowerballGoal;
    
    [Header("Add cards event")]
    public UnityEvent OnAddCardsEvent;
    public UnityEvent DeleteCopyOptions;
    
    private int triggeredThresholdsCount = 0;
    private int currentMaxTurns;
    private int currentTurns;
    
    private List<BlackjackEvent> availableLowEvents;
    private List<BlackjackEvent> availableMediumEvents;
    private List<BlackjackEvent> availableHighEvents;
    private List<EventThreshold> triggeredThresholds = new List<EventThreshold>();
    private List<int> powerballNumbers = new List<int>();

    private AceValueRule currentAceRule = AceValueRule.Flexible;
    
    private int targetMoneyBalance;
    
    public static bool isDoubleLowActive = false;
    public static bool isHalfHighActive = false;
    private bool isRouletteBlackjackActive = false;
    private bool isPowerballTriggered = false;
    private bool isNewPowerball = false;

    private IEnumerator eventTriggerCoroutine;
    
    private BlackjackGame blackjackGame;
    
    #region Getters
    
    public bool UseTurnLimit => useTurnLimit;
    public int TriggeredThresholdsCount => triggeredThresholdsCount;
    public int TurnsLeft => currentMaxTurns - currentTurns;
    public bool IsDoubleLowActive => isDoubleLowActive;
    public bool IsHalfHighActive => isHalfHighActive;
    public bool IsRouletteBlackjackActive => isRouletteBlackjackActive;
    public List<int> PowerballGoal => powerballNumbers;
    public List<EventThreshold> EventThresholds => eventThresholds;
    
    #endregion

    #region Setters

    public void SetBlackjackGame(BlackjackGame game) => blackjackGame = game;

    #endregion
    
    #region Monobehaviour Methods
    
    private void Awake()
    {
        currentMaxTurns = eventThresholds.First().maxTurns;
        currentTurns = 0;
    }

    private void Start()
    {
        availableLowEvents = new List<BlackjackEvent>(lowSeverityEvents);
        availableMediumEvents = new List<BlackjackEvent>(mediumSeverityEvents);
        availableHighEvents = new List<BlackjackEvent>(highSeverityEvents);
    }

    #endregion
    
    #region Check Event Rules

    public bool IsAceRule(AceValueRule aceRule) => currentAceRule == aceRule;

    #endregion
    
    #region Activate Events
    
    public void RemoveValueFromDeck(Card.Rank rank) => blackjackGame.GameDeck.AddRemovedValue(rank);
    
    public void RemoveSuitFromDeck(Card.Suit suit) => blackjackGame.GameDeck.AddRemovedSuit(suit);
    
    public void AddJokers() => blackjackGame.GameDeck.AddJokersToDeck();

    public void SetAceRule(AceValueRule newRule) => currentAceRule = newRule;
    
    public void SetRouletteBlackjackActive(bool active) => isRouletteBlackjackActive = active;

    public void SetNegativeSuit(Card.Suit suit)
    {
        CardEffects.AddNegativeSuit(suit);
        blackjackGame.UpdateCardVFX();
    }

    public void SetDoubleLowActive(bool active)
    {
        isDoubleLowActive = active;
        blackjackGame.UpdateCardVFX();
    }

    public void SetHalfHighActive(bool active)
    {
        isHalfHighActive = active;
        blackjackGame.UpdateCardVFX();
    }
    
    public void SetPowerballEventActive(List<int> goal)
    {
        powerballNumbers = goal;
        isPowerballTriggered = true;
    }
    
    #endregion

    #region Add Cards Event

    public void DisplayCardOptions(int minValue, int maxValue)
    {
        var copyCount = blackjackGame.GameDeck.GetCopyCount(minValue, maxValue);
        OnAddCardsEvent?.Invoke();
        
        StopEventFlow();
        
        blackjackGame.DialogueSystem.ShowAddCardsText(copyCount);
    }

    public void AddClickableCardOptions() => blackjackGame.CursorDetection.OnSelectCardOption(blackjackGame, CardTrigger.AddCardsEvent);
    
    public void AddCardCopies(Card card) => blackjackGame.GameDeck.AddCardCopies(card);
    
    public void SelectCardCopyEnd() => StartCoroutine(SelectCardCopyEndCoroutine());
    private IEnumerator SelectCardCopyEndCoroutine()
    {
        yield return new WaitForSeconds(0.7f);
        blackjackGame.DialogueSystem.ShowCopyChoiceTaunt();
        
        yield return new WaitForSeconds(1.5f);
        DeleteCopyOptions?.Invoke();
        blackjackGame.StartGame();
    }
    
    #endregion

    #region Powerball Event

    public IEnumerator CheckPowerballAtIndex(int index)
    {
        if (powerballNumbers == null || powerballNumbers.Count == 0) yield break;

        var hand = blackjackGame.PlayerHands[index];
        int handValue = Mathf.Abs(blackjackGame.CalculateHandValue(hand, true));
        powerballNumbers.RemoveAll(number => number == handValue);

        if (powerballNumbers.Count == 0)
        {
            blackjackGame.DialogueSystem.ShowPowerballTaunt();
            blackjackGame.GainMoney(3 * blackjackGame.CurrentBet);
            OnPowerballComplete();
        }
        
        UpdatePowerballGoal?.Invoke();
    }

    private void OnPowerballComplete()
    {
        powerballNumbers = PowerballEvent.GenerateNumbers();
        isNewPowerball = true;
    }

    public void ShowNewPowerballTaunt()
    {
        if (!isNewPowerball) return;
        
        blackjackGame.DialogueSystem.ShowPowerballGenerateTaunt();
        isNewPowerball = false;
    }

    #endregion

    #region Roulette Event

    public IEnumerator ChangeBlackjackGoal()
    {
        if (!isRouletteBlackjackActive) yield break;
        
        blackjackGame.GameCamera.ChangeToCamera(CameraType.Event);

        var goal = Random.Range(21, 37);
        blackjackGame.SetStatusText($"New Blackjack goal: {goal}");
        blackjackGame.SetBlackjackGoal(goal);

        yield return StartCoroutine(GameUtils.WaitDelayOrInput(4f));

        blackjackGame.GameCamera.ChangeToCamera(CameraType.Playing);
    }

    #endregion
    
    // TODO: move method to other script
    #region Check Cards
    
    public bool CheckIfDoubled(Card card)
    {
        if(card.rank == Card.Rank.Joker) return false;

        if(!isDoubleLowActive) return false;

        float cardValue = 0f;

        if(card.rank >= Card.Rank.Ten && card.rank <= Card.Rank.King) cardValue = 10f;
        else if(card.rank == Card.Rank.Ace) cardValue = currentAceRule == AceValueRule.Always1 ? 1f : 11f;
        else cardValue = (int)card.rank;

        return cardValue < 6f;
    }

    public bool CheckIfHalved(Card card)
    {
        if(card.rank == Card.Rank.Joker) return false;

        if(!isHalfHighActive) return false;

        float cardValue = 0f;

        if(card.rank >= Card.Rank.Ten && card.rank <= Card.Rank.King) cardValue = 10f;
        else if(card.rank == Card.Rank.Ace) cardValue = currentAceRule == AceValueRule.Always1 ? 1f : 11f;
        else cardValue = (int)card.rank;

        return cardValue > 5f;
    }
    
    #endregion
    
    #region End Blackjack Turn

    public void UpdateTurnsLeft()
    {
        if (!useTurnLimit) return;
        
        currentTurns++;
        ChangeProgressText.Invoke();
    }

    public IEnumerator CheckTurnLimit()
    {
        if (!useTurnLimit || currentTurns < currentMaxTurns) yield break;
        
        blackjackGame.DialogueSystem.ShowTurnLimitTaunt();

        yield return new WaitWhile(() => blackjackGame.DialogueSystem.IsPlaying);
        
        SceneManager.LoadSceneAsync(3);
    }

    public IEnumerator CheckForEventTrigger()
    {
        eventTriggerCoroutine = CheckForEventTriggerCoroutine();
        
        yield return StartCoroutine(eventTriggerCoroutine);
    }
    
    // TODO: refactor when adding canon events
    private IEnumerator CheckForEventTriggerCoroutine()
    {
        bool eventTriggered = false;
        bool introPlayed = false;

        while(true)
        {
            EventThreshold thresholdToTrigger = null;

            foreach(var threshold in eventThresholds)
            {
                if(blackjackGame.TargetMoneyBalance >= threshold.moneyAmount && !triggeredThresholds.Contains(threshold))
                {
                    thresholdToTrigger = threshold;

                    break;
                }
            }

            if(thresholdToTrigger == null) break;

            if(!introPlayed)
            {
                blackjackGame.GameCamera.ChangeToCamera(CameraType.Event);

                blackjackGame.SetStatusText("Lets make it more interesting");

                AudioManager.instance.Play("Laugh");

                yield return StartCoroutine(GameUtils.WaitDelayOrInput(5.0f));

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

                var text = $"New Event: {chosenEvent.eventName}";
                blackjackGame.SetStatusText(text);

                yield return StartCoroutine(GameUtils.WaitDelayOrInput(5.0f));
            }
        }

        if(eventTriggered)
        {
            triggeredThresholdsCount++;
            currentMaxTurns = eventThresholds[triggeredThresholdsCount].maxTurns;
            currentTurns = 0;
            
            ChangeProgressText?.Invoke();
            UpdatePowerballGoal?.Invoke();
            blackjackGame.GameCamera.ChangeToCamera(CameraType.Sitting);
            
            if (isPowerballTriggered)
            {
                blackjackGame.DialogueSystem.PlayPowerballTutorial();
                isPowerballTriggered = false;
            }
        }
    }

    #endregion
    
    #region Event Flow
    
    private void StopEventFlow()
    {
        StopCoroutine(eventTriggerCoroutine);
        blackjackGame.ClearTable();
        blackjackGame.UpdateBettingUI();
        blackjackGame.ResetTexts();
    }
    
    #endregion
}
