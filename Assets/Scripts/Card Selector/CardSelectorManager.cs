using TMPro;
using UnityEngine;

public class CardSelectorManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI display;
    [SerializeField] private BlackjackGame blackjackManager;
    [SerializeField] private GameObject cardSelector;
    [SerializeField] private Animator animator;
    private CardSelectorButton[] selectorButtons;
    private Keepsake activeKeepsake;
    private Card.Suit? selectedSuit = null;
    private Card.Rank? selectedRank = null;
    private bool hasPrintedThisTurn = false;
    private TableCards tableCards;

    private void Awake()
    {
        selectorButtons = cardSelector.GetComponentsInChildren<CardSelectorButton>(true);
        tableCards = blackjackManager.TableCards;
    }

    private void Start()
    {
        cardSelector.SetActive(false);
    }

    public void OpenCardSelector(Keepsake keepsake)
    {
        hasPrintedThisTurn = false;
        activeKeepsake = keepsake;
        blackjackManager.CursorDetection.OnDealerTurn();
        cardSelector.SetActive(true);

        foreach(var button in selectorButtons)
        {
            button.SetActive(true);
        }

        ResetInputs();
    }

    public void CloseCardSelector()
    {
        animator.SetTrigger("hideTrigger");
        blackjackManager.CursorDetection.OnRoundActive();

        if(!hasPrintedThisTurn)
        {
            TableKeepsakeInteractable[] tableObjects = FindObjectsByType<TableKeepsakeInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach(var tableObject in tableObjects)
            {
                if(tableObject.keepsake is SecondDealing || tableObject.keepsake is Printer || tableObject.keepsake is GoFish)
                {
                    tableObject.ResetUse();
                }
            }
        }

        activeKeepsake = null;
    }

    public void DisableCardSelector()
    {
        cardSelector.SetActive(false);
    }

    public void SetSuit(int suitIndex)
    {
        selectedSuit = (Card.Suit)suitIndex;

        UpdateScreen();
    }

    public void SetRank(int rankIndex)
    {
        selectedRank = (Card.Rank)rankIndex;

        UpdateScreen();
    }

    public void PrintCard()
    {
        if(selectedSuit == null || selectedRank == null)
        {
            AudioManager.instance.Play("ItemDeny");

            return;
        }

        CreateCard();
    }

    private void CreateCard()
    {
        Card newCard = new Card { rank = selectedRank.Value, suit = selectedSuit.Value };

        switch (activeKeepsake)
        {
            case SecondDealing secondDealing:
                UseSecondDealing(newCard, secondDealing);
                break;
            case Printer printer:
                UsePrinter(newCard);
                break;
            case GoFish goFish:
                UseGoFish(newCard, goFish);
                break;
        }
    }

    private void UseGoFish(Card newCard, GoFish goFish)
    {
        hasPrintedThisTurn = true;

        GoFishRank(goFish, newCard.rank, (success) =>
        {
            AudioManager.instance.Play(success ? "CardHit" : "ItemDeny");
        });

        CloseCardSelector();
    }
    
    private void GoFishRank(GoFish goFish, Card.Rank targetRank, System.Action<bool> onComplete)
    {
        StartCoroutine(goFish.GoFishCoroutine(targetRank, onComplete));
    }

    private void UsePrinter(Card newCard)
    {
        tableCards.GameDeck.AddCardCopy(newCard);
        hasPrintedThisTurn = true;

        AudioManager.instance.Play("CardHit");

        CloseCardSelector();
    }

    private void UseSecondDealing(Card newCard, SecondDealing secondDealing)
    {
        Card dealtCard = tableCards.GameDeck.DealSecondDealingCard(newCard.rank, newCard.suit);
        CardInstance instance = tableCards.DealCardInstance(dealtCard, false);

        secondDealing.UseCharge();

        if(instance != null)
        {
            hasPrintedThisTurn = true;
            blackjackManager.HandleNewCardInPlayerHand(instance);

            AudioManager.instance.Play("CardHit");

            CloseCardSelector();
        }
        else
        {
            AudioManager.instance.Play("ItemDeny");
        }
    }

    private void UpdateScreen()
    {
        string suitText = selectedSuit.HasValue ? Card.GetSuitString(selectedSuit.Value) : " ";
        string rankText = selectedRank.HasValue ? Card.GetRankString(selectedRank.Value) : " ";

        display.text = $"{rankText} {suitText}";
    }

    private void ResetInputs()
    {
        selectedSuit = null;
        selectedRank = null;
        display.text = "";
    }

    public void ResetPrinting()
    {
        hasPrintedThisTurn = false;
        activeKeepsake = null;
    }
}