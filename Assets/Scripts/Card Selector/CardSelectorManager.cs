using TMPro;
using UnityEngine;

public class CardSelectorManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI display;
    [SerializeField] private BlackjackGame blackjackManager;
    [SerializeField] private GameObject machineContainer;
    private Card.Suit? selectedSuit = null;
    private Card.Rank? selectedRank = null;
    private bool hasPrintedThisTurn = false;

    public void OpenMachine()
    {
        if(hasPrintedThisTurn) return;

        machineContainer.SetActive(true);

        ResetInputs();
    }

    public void CloseMachine()
    {
        machineContainer.SetActive(false);
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

        blackjackManager.GameDeck.AddPrintedCard(newCard); //change later

        CardInstance instance = blackjackManager.DealCardInstanceOption(newCard, false);

        if(instance != null)
        {
            hasPrintedThisTurn = true;
            blackjackManager.HandleNewCardInPlayerHand(instance);

            AudioManager.instance.Play("CardHit");

            CloseMachine();
        }
        else
        {
            AudioManager.instance.Play("ItemDeny");
        }
    }

    private void UpdateScreen()
    {
        string suitText = selectedSuit.HasValue ? selectedSuit.Value.ToString() : "_";
        string rankText = selectedRank.HasValue ? selectedRank.Value.ToString() : "_";

        display.text = $"{suitText} {rankText}";
    }

    private void ResetInputs()
    {
        selectedSuit = null;
        selectedRank = null;
        display.text = "INPUT CARD";
    }

    public void OnRoundStart()
    {
        hasPrintedThisTurn = false;
    }
}