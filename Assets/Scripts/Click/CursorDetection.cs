using System;
using System.Collections.Generic;
using UnityEngine;

public enum CardTrigger
{
    None,
    Acid,
    Scissors,
    AddCardsEvent,
    AntiMatter,
    Pyro,
    HatTrick
}

public class CursorDetection : MonoBehaviour
{
    [SerializeField] private new Camera camera;
    [SerializeField] private List<Clickable> roundActiveClickables;
    [SerializeField] private List<Clickable> roundInactiveClickables;
    [SerializeField] private List<Transform> cardTransforms;
    [SerializeField] private Transform cardOptions;

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
                    if(cardTrigger == CardTrigger.Scissors && CardEffects.IsCardCut(cardDisplay.GetCardInstance())) continue;

                    clickableCard.SetCardInstance(cardDisplay.GetCardInstance());
                    clickableCard.SetBlackjackGame(blackjackGame);
                    clickableCard.AddCardAction(OnClickCard);
                    AddCardAction(blackjackGame, clickableCard, cardTrigger);
                    clickableCard.AddCardAction(ReactivateClickables);

                    cardClickables.Add(clickableCard);
                }
            }
        }
    }

    public void AddActionToClickableCards(Action<CardInstance> cardAction)
    {
        foreach (var clickable in cardClickables)
        {
            var card = (ClickableCard)clickable;
            card.AddCardEffect(cardAction);
        }
    }

    // TODO: shouldn't need blackjack game
    private void AddCardAction(BlackjackGame blackjackGame, ClickableCard clickableCard, CardTrigger cardTrigger)
    {
        switch (cardTrigger)
        {
            case CardTrigger.AddCardsEvent:
                clickableCard.AddCardAction(clickableCard.OnAddCardsOption);
                clickableCard.AddCardAction(() => blackjackGame.SelectCursorHand(false));
                break;
        }
    }
    
    private void OnClickCard()
    {
        RemoveCardEffects();
        SetClickables(cardClickables, false);
        cardClickables.RemoveAll(_ => true);
    }

    private void RemoveCardEffects()
    {
        foreach(var clickable in cardClickables)
        {
            var cardClickable = clickable as ClickableCard;
            cardClickable?.RemoveCardEffect();
        }
    }

    public void AddRoundActiveClickable(Clickable clickable)
    {
        if(!roundActiveClickables.Contains(clickable))
        {
            roundActiveClickables.Add(clickable);
        }
    }

    // TODO: remove methods
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