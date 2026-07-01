using System.Collections.Generic;
using UnityEngine;

public class CursorDetection : MonoBehaviour
{
    [SerializeField] private new Camera camera;
    [SerializeField] private List<Clickable> roundActiveClickables;
    [SerializeField] private List<Clickable> roundInactiveClickables;
    [SerializeField] private List<Transform> cardTransforms;

    private List<Clickable> cardClickables;
    
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        int mouseButton = -1;

        if(Input.GetMouseButtonDown(0)) mouseButton = 0;
        else if(Input.GetMouseButtonDown(1)) mouseButton = 1;

        if (mouseButton != -1)
        {
            var ray = camera.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                var clickable = hit.transform.GetComponent<Clickable>();

                if(clickable)
                {
                    clickable.OnClick(mouseButton);
                }
            }
        }
    }

    public void OnRoundActive()
    {
        SetClickables(roundActiveClickables, true);
        SetClickables(roundInactiveClickables, false);
    }

    public void OnRoundInactive()
    {
        SetClickables(roundActiveClickables, false);
        SetClickables(roundInactiveClickables, true);
    }

    public void OnDealerTurn()
    {
        SetClickables(roundActiveClickables, false);
        SetClickables(roundInactiveClickables, false);
    }

    private void SetClickables(List<Clickable> clickables, bool isActive)
    {
        foreach (var clickable in clickables)
        {
            clickable.SetActive(isActive);
            clickable.OnRemoveOutline();
        }
    }

    #region Clickable Cards

    public void OnUseCardItem(BlackjackGame blackjackGame, ItemType itemType)
    {
        AddAllClickableCards(blackjackGame, itemType);
        SetClickables(cardClickables, true);
        SetClickables(roundActiveClickables, false);
    }

    private void AddAllClickableCards(BlackjackGame blackjackGame, ItemType itemType)
    {
        cardClickables = new List<Clickable>();
        foreach (var cardsTransform in cardTransforms)
            AddClickableCards(cardsTransform, blackjackGame, itemType);
    }

    private void AddClickableCards(Transform cardsTransform, BlackjackGame blackjackGame, ItemType itemType)
    {
        foreach (Transform card in cardsTransform)
        {
            var cardDisplay = card.GetComponent<CardDisplay>();
            var face = card.transform.GetChild(0);
            if (face)
            {
                var clickableCard = face.GetComponent<ClickableCard>();
                if (clickableCard)
                {
                    if(blackjackGame.IsCardScissored(cardDisplay.GetCardInstance())) continue;

                    clickableCard.SetCardInstance(cardDisplay.GetCardInstance());
                    clickableCard.SetBlackjackGame(blackjackGame);
                    clickableCard.AddAction(OnClickCard);
                    AddCardAction(clickableCard, itemType);
                    clickableCard.AddAction(ReactivateClickables);
                    cardClickables.Add(clickableCard);
                }
            }
        }
    }

    private void AddCardAction(ClickableCard clickableCard, ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Scissors:
                clickableCard.AddAction(clickableCard.OnCutCard);
                break;
            case ItemType.Acid:
                clickableCard.AddAction(clickableCard.OnDissolveCard);
                break;
        }
    }
    
    private void OnClickCard()
    {
        RemoveCardActions();
        SetClickables(cardClickables, false);
        cardClickables.RemoveAll(_ => true);
    }

    private void RemoveCardActions()
    {
        foreach (var clickable in cardClickables)
        {
            var cardClickable = (ClickableCard)clickable;

            if (cardClickable)
            {
                cardClickable.RemoveAction(OnClickCard);
                cardClickable.RemoveAction(cardClickable.OnCutCard);
                cardClickable.RemoveAction(ReactivateClickables);
            }
        }
    }

    private void ReactivateClickables() => SetClickables(roundActiveClickables, true);

    #endregion

}
