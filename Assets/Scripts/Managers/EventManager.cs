using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum AceValueRule { Flexible, Always1, Always11 }

public class EventManager : MonoBehaviour
{
    [Header("Display Event")] 
    [SerializeField] private GameCamera gameCamera;
    [SerializeField] private TMPro.TextMeshProUGUI statusText;
    [SerializeField] private DialogueSystem dialogueSystem;
    [SerializeField] private ProgressDisplay progressDisplay;

    [Header("Cards Event Actions")]
    [SerializeField] private AddCardChoiceEvent addCardChoiceEvent;
    
    private List<int> powerballNumbers = new();
    
    public static AceValueRule currentAceRule = AceValueRule.Flexible;
    public static bool isDoubleLowActive = false;
    public static bool isHalfHighActive = false;
    private bool isRouletteBlackjackActive = false;
    private bool isNewPowerball = false;

    public Image dealerRagebar;
    private int dealerRageNumber;
    private float currentValue = 0;
    public float maxValue = 4f;

    private bool isDealerRageTriggered = false;
    private bool isDealerRageActive = false;
    
    private BlackjackGame blackjackGame;
    private TableCards tableCards;

    #region Getters
    
    public List<int> PowerballGoal => powerballNumbers;

    public bool IsDealerRageActive => isDealerRageActive;
    public int GetDealerRageNumber => dealerRageNumber;
    
    #endregion

    #region Setters

    public void SetBlackjackGame(BlackjackGame game)
    {
        blackjackGame = game;
        tableCards = game.TableCards;
    }

    #endregion
    
    #region Monobehaviour Methods
    
    private void Awake()
    {
        currentAceRule = AceValueRule.Flexible;
        isDoubleLowActive = false;
        isHalfHighActive = false;
    }
    
    #endregion
    
    #region Activate Events
    
    public void RemoveValueFromDeck(Card.Rank rank) => tableCards.GameDeck.AddRemovedValue(rank);
    
    public void RemoveSuitFromDeck(Card.Suit suit) => tableCards.GameDeck.AddRemovedSuit(suit);
    
    public void AddJokers() => tableCards.GameDeck.AddJokersToDeck();

    public void SetAceRule(AceValueRule newRule) => currentAceRule = newRule;
    
    public void SetRouletteBlackjackActive(bool active) => isRouletteBlackjackActive = active;

    public void SetNegativeSuit(Card.Suit suit)
    {
        CardEffects.AddNegativeSuit(suit);
        tableCards.UpdateCardVFX();
    }

    public void SetDoubleLowActive(bool active)
    {
        isDoubleLowActive = active;
        tableCards.UpdateCardVFX();
    }

    public void SetHalfHighActive(bool active)
    {
        isHalfHighActive = active;
        tableCards.UpdateCardVFX();
    }
    
    public void SetPowerballEventActive(List<int> goal)
    {
        powerballNumbers = goal;
        progressDisplay.UpdatePowerballGoal(goal);
    }
    public void SetDealerRageActive(bool active)
    {
        isDealerRageActive = active;
        isDealerRageTriggered = true;   
    }
    #endregion

    #region Powerball Event

    public IEnumerator CheckPowerballCompletion()
    {
        if (powerballNumbers == null || powerballNumbers.Count == 0) yield break;

        var hand = tableCards.CurrentHand;
        int handValue = Mathf.Abs(tableCards.CalculateHandValue(hand, true));
        powerballNumbers.RemoveAll(number => number == handValue);

        if (powerballNumbers.Count == 0)
        {
            dialogueSystem.ShowPowerballTaunt();
            blackjackGame.GainMoney(3 * blackjackGame.CurrentBet);
            OnPowerballComplete();
        }
        
        progressDisplay.UpdatePowerballGoal(PowerballGoal);
    }

    private void OnPowerballComplete()
    {
        powerballNumbers = PowerballEvent.GenerateNumbers();
        progressDisplay.UpdatePowerballGoal(powerballNumbers);
        isNewPowerball = true;
    }

    public void ShowNewPowerballTaunt()
    {
        if (!isNewPowerball) return;
        
        dialogueSystem.ShowPowerballGenerateTaunt();
        isNewPowerball = false;
    }

    #endregion

    #region Roulette Event

    public IEnumerator ChangeBlackjackGoal()
    {
        if (!isRouletteBlackjackActive) yield break;
        
        gameCamera.ChangeToCamera(CameraType.Event);

        var goal = Random.Range(21, 37);
        statusText.text = $"New Blackjack goal: {goal}";
        blackjackGame.SetBlackjackGoal(goal);//TODO change

        yield return StartCoroutine(GameUtils.WaitDelayOrInput(4f));

        gameCamera.ChangeToCamera(CameraType.Playing);
    }

    #endregion
    
    #region Event Flow
    
    public IEnumerator TriggerEvent(BlackjackEvent gameEvent)
    {
        if (!gameEvent) yield break;
        
        ClearTable();

        yield return gameEvent.StartDisplay(gameCamera, statusText);

        yield return PresentPlayerChoice(gameEvent);
        
        gameEvent.Apply(this);
        
        yield return gameEvent.ExplainEventDialogue(dialogueSystem);
    }
    
    #endregion

    #region Card Choice Events

    private IEnumerator PresentPlayerChoice(BlackjackEvent gameEvent)
    {
        if (!gameEvent) yield break;
        
        gameEvent.ExplainChoiceDialogue(dialogueSystem);
        
        var cardChoiceEvent = GetCardChoiceEvent(gameEvent);
        yield return gameEvent.GiveChoiceToPlayer(gameCamera, cardChoiceEvent);

        if (cardChoiceEvent)
            yield return new WaitUntil(() => cardChoiceEvent.isChoosing == false);
    }

    private CardChoiceEvent GetCardChoiceEvent(BlackjackEvent blackjackEvent)
    {
        var addCards = blackjackEvent as AddCardsEvent;
        if (addCards) return addCardChoiceEvent;

        return null;
    }
    
    private void ClearTable()
    {
        tableCards.ClearTable();
        blackjackGame.UpdateBettingUI();
        blackjackGame.ResetTexts();
    }

    #endregion

    public void ChangeRageNumber(int value)
    {
        dealerRageNumber += value;
        if(dealerRageNumber <0)
        {
            dealerRageNumber = 0;
        }
        currentValue = Mathf.Clamp(currentValue+value, 0, maxValue);
        UpdateBar();
    }
    void UpdateBar()
    {
        dealerRagebar.fillAmount = currentValue / maxValue;
    }
}
