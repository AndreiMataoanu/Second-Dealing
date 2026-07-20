using System;
using System.Collections.Generic;
using UnityEngine;

public enum CardTrigger
{
    Acid,
    Scissors,
    AddCardsEvent,
    AntiMatter,
    Pyro,
    HatTrick,
}

public class CursorDetection : MonoBehaviour
{
    [SerializeField] private new Camera camera;
    [SerializeField] private List<Clickable> roundActiveClickables;
    [SerializeField] private List<Clickable> roundInactiveClickables;
    [SerializeField] private List<Transform> cardTransforms;
    [SerializeField] private Transform cardOptions;

    private List<Clickable> cardClickables;
    private List<Clickable> tarotClickables = new();

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
        clickables.ForEach(clickable => SetClickable(clickable, isActive));
    }

    private void SetClickable(Clickable clickable, bool isActive)
    {
        clickable.SetActive(isActive);
        clickable.OnRemoveOutline();
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
        SetClickables(tarotClickables, false);
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
            var clickableCard = card.GetComponentInChildren<ClickableCard>();

            if(clickableCard)
            {
                var cardInstance = cardDisplay.GetCardInstance();
                if (cardTrigger == CardTrigger.Scissors && CardEffects.IsCardCut(cardInstance)) continue;
                if (cardInstance.tarotData) continue;
                
                clickableCard.SetCardInstance(cardInstance);
                clickableCard.SetBlackjackGame(blackjackGame);
                clickableCard.AddCardAction(OnClickCard);
                AddCardAction(blackjackGame, clickableCard, cardTrigger);
                clickableCard.AddCardAction(ReactivateClickables);

                cardClickables.Add(clickableCard);
            }
        }
    }

    private void AddActionToClickableCards(Action<CardInstance> cardAction, List<Clickable> clickableList)
    {
        foreach (var clickable in clickableList)
        {
            var card = (ClickableCard)clickable;
            card.AddCardEffect(cardAction);
        }
    }
    
    public void AddActionToClickableCards(Action<CardInstance> cardAction)
    {
        AddActionToClickableCards(cardAction, cardClickables);
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
            roundActiveClickables.Add(clickable);
    }

    public void RemoveRoundActiveClickable(Clickable clickable)
    {
        roundActiveClickables.Remove(clickable);
    }

    #endregion

    #region Clickable Tarot Cards

    public void ResetTarotClickables()
    {
        SetClickables(tarotClickables, false);
        tarotClickables = new List<Clickable>();
    }

    public ClickableCard AddTarotClickable(BlackjackGame blackjackGame, CardInstance cardInstance)
    {
        ClickableCard clickableCard = cardInstance.CardObject.GetComponentInChildren<ClickableCard>();
        
        if (!clickableCard || !cardInstance.tarotData || !cardInstance.tarotData.rewardItemPrefab) return null;
                    
        clickableCard.SetCardInstance(cardInstance);
        clickableCard.SetBlackjackGame(blackjackGame);
        clickableCard.AddCardAction(() => tarotClickables.Remove(clickableCard));

        SetClickable(clickableCard, true);
        tarotClickables.Add(clickableCard);

        return clickableCard;
    }
    
    private void ReactivateClickables()
    {
        SetClickables(roundActiveClickables, true);
        SetClickables(tarotClickables, true);
    }

    public void SetCardActive(CardInstance cardInstance, bool isActive)
    {
        var clickableCard = cardInstance.CardObject.GetComponentInChildren<ClickableCard>();
        SetClickable(clickableCard, isActive);
    }
    
    #endregion
    
    #region Getters
    public Transform GetCardOptionsPosition() => cardOptions;
    #endregion
}