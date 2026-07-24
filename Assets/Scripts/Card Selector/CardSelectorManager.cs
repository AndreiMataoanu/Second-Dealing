using TMPro;
using UnityEngine;

public class CardSelectorManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI display;
    [SerializeField] private BlackjackGame blackjackManager;
    [SerializeField] private GameObject cardSelector;
    private CardSelectorButton[] selectorButtons;
    private Keepsake activeKeepsake;
    private Card.Suit? selectedSuit = null;
    private Card.Rank? selectedRank = null;
    private bool hasPrintedThisTurn = false;

    private void Awake()
    {
        selectorButtons = cardSelector.GetComponentsInChildren<CardSelectorButton>(true);
    }

    private void Start()
    {
        cardSelector.SetActive(false);
    }

    public void OpenCardSelector(Keepsake keepsake)
    {
        if(hasPrintedThisTurn) return;

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
        cardSelector.SetActive(false);
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

        if(activeKeepsake is SecondDealing secondDealing)
        {
            Card? createdCard = blackjackManager.GameDeck.DealSecondDealingCard(newCard.rank, newCard.suit);

            if(!createdCard.HasValue)
            {
                AudioManager.instance.Play("ItemDeny");

                return;
            }

            secondDealing.UseCharge();

            CardInstance instance = blackjackManager.DealCardInstanceOption(newCard, false);

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
        else if(activeKeepsake is Printer printer)
        {
            blackjackManager.GameDeck.AddPrintedCard(newCard);
            hasPrintedThisTurn = true;

            AudioManager.instance.Play("CardHit");

            CloseCardSelector();
        }
        else if(activeKeepsake is GoFish goFish)
        {
            hasPrintedThisTurn = true;

            blackjackManager.GoFishRank(newCard.rank, (success) =>
            {
                if(success)
                {
                    AudioManager.instance.Play("CardHit");
                }
                else
                {
                    AudioManager.instance.Play("ItemDeny");
                }
            });

            CloseCardSelector();
        }
    }

    private void UpdateScreen()
    {
        string suitText = selectedSuit.HasValue ? selectedSuit.Value.ToString() : " ";
        string rankText = selectedRank.HasValue ? selectedRank.Value.ToString() : " ";

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
    }
}