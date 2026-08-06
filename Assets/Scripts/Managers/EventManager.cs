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
    [SerializeField] private ItemManager itemManager;
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private Collider betUpCollider;
    [SerializeField] private Collider betDownCollider;

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
    [SerializeField] private int bossRound = 4; 
    private float currentValue = 0;
    private bool letItRide = false;
    private bool isDealerRageActive = false;
    private bool isBossRound = false;
    private DealerRageAbility ability;
    private BlackjackGame blackjackGame;
    private TableCards tableCards;

    #region Getters
    
    public List<int> PowerballGoal => powerballNumbers;

    public bool IsDealerRageActive => isDealerRageActive;
    public int GetDealerRageNumber => dealerRageNumber;
    public bool GetIsBossRound => isBossRound;
    public bool GetLetItRide => letItRide;

    #endregion

    #region Setters

    public void SetBlackjackGame(BlackjackGame game)
    {
        blackjackGame = game;
        tableCards = game.TableCards;
    }

    #endregion
    #region enum for dealer rage
    public enum DealerRageAbility
    {
        ForceAllIn,
        RemoveItem,
        HalvePlayerCards,
        ShufflePlayerHand
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
        dealerRagebar.gameObject.SetActive(true);
        dealerRageNumber = 0;
        UpdateBar();
         
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

    #region dealer rage event
    public void ChangeRageNumber(int value)
    {
        dealerRageNumber += value;
        if(dealerRageNumber <0)
        {
            dealerRageNumber = 0;
        }
        currentValue = Mathf.Clamp(currentValue+value, 0, bossRound);
        UpdateBar();
    }
    void UpdateBar()
    {
        dealerRagebar.fillAmount = currentValue / bossRound;
    }
    public void ActivateOrDeactivateDealerAbility()
    {
        if(GetDealerRageNumber >= bossRound && isBossRound == false)
        {
            isBossRound = true;
            ability = (DealerRageAbility)Random.Range(0,3);
            if(ability != DealerRageAbility.ShufflePlayerHand)
            {
                DealerRagePickAbility(ability);
            }
                
        }
        if(GetDealerRageNumber < bossRound)
        {
            NoBossRound();
        }
    }
    private void ForcePlayerAllInn()
    {
        letItRide = true;
        blackjackGame.ChangeBetTo(blackjackGame.GetPlayerMoney());
        dialogueSystem.ShowForcedAllInnTaunts();
        betDownCollider.gameObject.SetActive(false);
        betUpCollider.gameObject.SetActive(false);
    }

    public void CheckForShuffleAbility()
    {
        int handValue = tableCards.CalculateHandValue(tableCards.CurrentHand, true);
        if(handValue >= 20 && isBossRound && ability == DealerRageAbility.ShufflePlayerHand)
        {
            DealerRagePickAbility(ability); 
        }
    }

    private void ThrowAwayOneItem()
    {
        dialogueSystem.ShowDealerRemovesItemTaunts();
        int item = Random.Range(0,1);
        if(shopManager.InventoryItems.Count == 1)
        {
            itemManager.AddItemToRemove(shopManager.InventoryItems[0]);
        }
        else
        {
            itemManager.AddItemToRemove(shopManager.InventoryItems[item]);
        }
        
    }
    private void HalvePlayerCards()
    {
        dialogueSystem.ShowDealerHalvesCardsTaunts();
        tableCards.halvePlayerCards = true;
    }

    private void NoBossRound()
    {
        isBossRound = false;
        tableCards.halvePlayerCards = false;
        letItRide = false;
        betDownCollider.gameObject.SetActive(true);
        betUpCollider.gameObject.SetActive(true);
    }
    private void ShufflePlayerHand()
    {
        dialogueSystem.ShowDealerShuffleTaunts();
        tableCards.destroyPlayerCards();
    }

    private void DealerRagePickAbility(DealerRageAbility ability)
    {
        switch(ability)
        {
            case DealerRageAbility.ForceAllIn:
            ForcePlayerAllInn();
            break;

            case DealerRageAbility.RemoveItem:
            if(shopManager.InventoryItems.Count == 0)
                {
                    ability = (DealerRageAbility)Random.Range(0,3);
                    DealerRagePickAbility(ability);
                    break;
                }
            ThrowAwayOneItem();
            break;

            case DealerRageAbility.HalvePlayerCards:
            HalvePlayerCards();
            break;

            case DealerRageAbility.ShufflePlayerHand:
            ShufflePlayerHand();
            break;
        }

        
    }
    #endregion
}
