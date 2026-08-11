using System;
using Managers;
using UnityEngine;

public class CardEffectActions
{
    private BlackjackGame blackjackGame;
    private CursorFollow cursorFollow;
    private CursorDetection cursorDetection;
    private CursorType cursorType;
    private CardTrigger cardTrigger;

    // Keep cursor objects as parameters for blackjack game removal
    public CardEffectActions(BlackjackGame blackjackGame, CursorType cursorType, CardTrigger cardTrigger)
    {
        this.blackjackGame = blackjackGame;
        this.cursorFollow = blackjackGame.CursorFollow;
        this.cursorDetection = blackjackGame.CursorDetection;
        this.cursorType = cursorType;
        this.cardTrigger = cardTrigger;
    }

    public void SelectCard()
    {
        blackjackGame.ShopManager.SetInventoryActive(false);
        cursorFollow.SetCursorTypeActive(true, cursorType);
    }
    
    public void OnCardSelected()
    {
        blackjackGame.ShopManager.SetInventoryActive(true);
        cursorFollow.SetCursorTypeActive(false, cursorType);
        blackjackGame.UpdateUI();
    }
    
    public void OnCancelSelect()
    {
        blackjackGame.ShopManager.SetInventoryActive(true);
        cursorFollow.SetCursorTypeActive(false, cursorType);
        cursorDetection.EndSelectCard();
    }

    public void AddItemCardEffectAction(Action<CardInstance> cardEffect)
    {
        cursorDetection.OnItemSelectCard(blackjackGame, cardTrigger);
        cursorDetection.AddActionToClickableCards(cardEffect);
    }

    public void AddEventCardEffectAction(Action<CardInstance> cardEffect, Transform cardsPosition)
    {
        cursorDetection.OnEventSelectCard(cardsPosition, blackjackGame, cardTrigger);
        cursorDetection.AddActionToClickableCards(cardEffect);
    }
}
