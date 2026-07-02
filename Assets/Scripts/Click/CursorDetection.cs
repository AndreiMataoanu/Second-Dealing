using System.Collections.Generic;
using UnityEngine;

public enum CardTrigger
{
    Acid,
    Scissors,
    AddCardsEvent
}

public class CursorDetection : MonoBehaviour
{
    [SerializeField] private new Camera camera;
    [SerializeField] private List<Clickable> roundActiveClickables;
    [SerializeField] private List<Clickable> roundInactiveClickables;
    [SerializeField] private List<Transform> cardTransforms;
    [SerializeField] private Transform cardOptions;

    private List<Clickable> cardClickables;

    public enum CardTargetMode { None, Scissors, AntiMatter, Pyro, HatTrick }
    private CardTargetMode currentTargetMode = CardTargetMode.None;

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
    public void OnUseScissors(BlackjackGame blackjackGame)
    {
        currentTargetMode = CardTargetMode.Scissors;

        AddAllClickableCards(blackjackGame);
        SetClickables(cardClickables, true);
        SetClickables(roundActiveClickables, false);
    }
    
    public void OnSelectCardOption(BlackjackGame blackjackGame, CardTrigger cardTrigger)
    {
        cardClickables = new List<Clickable>();
        AddClickableCards(cardOptions, blackjackGame, cardTrigger);
        SetClickables(cardClickables, true);
        SetClickables(roundActiveClickables, false);
    }

    public void OnUseCardItem(BlackjackGame blackjackGame, CardTrigger cardTrigger)
    {
        AddAllClickableCards(blackjackGame, cardTrigger);
        SetClickables(cardClickables, true);
        SetClickables(roundActiveClickables, false);
    }

    public void OnUseAntiMatter(BlackjackGame blackjackGame)
    {
        currentTargetMode = CardTargetMode.AntiMatter;

        AddAllClickableCards(blackjackGame);
        SetClickables(cardClickables, true);
        SetClickables(roundActiveClickables, false);
    }

    public void OnUsePyro(BlackjackGame blackjackGame)
    {
        currentTargetMode = CardTargetMode.Pyro;

        AddAllClickableCards(blackjackGame);
        SetClickables(cardClickables, true);
        SetClickables(roundActiveClickables, false);
    }

    public void OnUseHatTrick(BlackjackGame blackjackGame)
    {
        currentTargetMode = CardTargetMode.HatTrick;

        AddAllClickableCards(blackjackGame);
        SetClickables(cardClickables, true);
        SetClickables(roundActiveClickables, false);
    }

    private void AddAllClickableCards(BlackjackGame blackjackGame, CardTrigger cardTrigger)
    {
        cardClickables = new List<Clickable>();
        foreach (var cardsTransform in cardTransforms)
            AddClickableCards(cardsTransform, blackjackGame, cardTrigger);
    }

    private void AddClickableCards(Transform cardsTransform, BlackjackGame blackjackGame, CardTrigger cardTrigger)
    {
        foreach(Transform card in cardsTransform)
        {
            var cardDisplay = card.GetComponent<CardDisplay>();
            var face = card.transform.GetChild(0);

            if(face)
            {
                var clickableCard = face.GetComponent<ClickableCard>();

                if(clickableCard)
                {
                    if(currentTargetMode == CardTargetMode.Scissors && blackjackGame.IsCardScissored(cardDisplay.GetCardInstance())) continue;

                    clickableCard.SetCardInstance(cardDisplay.GetCardInstance());
                    clickableCard.SetBlackjackGame(blackjackGame);
                    clickableCard.AddAction(OnClickCard);

                    if(currentTargetMode == CardTargetMode.Scissors)
                    {
                        clickableCard.AddAction(clickableCard.OnCutCard);
                    }
                    else if(currentTargetMode == CardTargetMode.AntiMatter)
                    {
                        clickableCard.AddAction(clickableCard.OnAntiMatterCard);
                    }
                    else if(currentTargetMode == CardTargetMode.Pyro)
                    {
                        clickableCard.AddAction(clickableCard.OnPyroCard);
                    }
                    else if(currentTargetMode == CardTargetMode.HatTrick)
                    {
                        clickableCard.AddAction(clickableCard.OnHatTrickCard);
                    }

                    AddCardAction(blackjackGame, clickableCard, cardTrigger);
                    clickableCard.AddAction(ReactivateClickables);

                    cardClickables.Add(clickableCard);
                }
            }
        }
    }

    private void AddCardAction(BlackjackGame blackjackGame, ClickableCard clickableCard, CardTrigger cardTrigger)
    {
        switch (cardTrigger)
        {
            case CardTrigger.Scissors:
                clickableCard.AddAction(clickableCard.OnCutCard);
                break;
            case CardTrigger.Acid:
                clickableCard.AddAction(clickableCard.OnDissolveCard);
                break;
            case CardTrigger.AddCardsEvent:
                clickableCard.AddAction(clickableCard.OnAddCardsOption);
                clickableCard.AddAction(() => blackjackGame.SelectCursorHand(false));
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
        foreach(var clickable in cardClickables)
        {
            var cardClickable = (ClickableCard)clickable;

            if(cardClickable)
            {
                cardClickable.RemoveAction(OnClickCard);
                cardClickable.RemoveAction(cardClickable.OnCutCard);
                cardClickable.RemoveAction(cardClickable.OnAntiMatterCard);
                cardClickable.RemoveAction(cardClickable.OnPyroCard);
                cardClickable.RemoveAction(cardClickable.OnHatTrickCard);
                cardClickable.RemoveAction(ReactivateClickables);
            }
        }

        currentTargetMode = CardTargetMode.None;
    }

    public void RemoveRoundActiveClickable(Clickable clickable)
    {
        if(roundActiveClickables.Contains(clickable))
        {
            roundActiveClickables.Remove(clickable);
        }
    }

    private void ReactivateClickables() => SetClickables(roundActiveClickables, true);
    #endregion

    #region Getters
    public Transform GetCardOptionsPosition() => cardOptions;
    #endregion
}