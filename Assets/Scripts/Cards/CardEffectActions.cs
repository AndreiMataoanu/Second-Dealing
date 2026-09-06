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

    #region Select card
    
    public void SelectCard()
    {
        blackjackGame.ShopManager.SetInventoryActive(false);
        cursorFollow.SetCursorTypeActive(true, cursorType);
    }

    public void SelectCard(Vector3 startMousePosition)
    {
        blackjackGame.ShopManager.SetInventoryActive(false);
        cursorFollow.UseCursorAtPosition(true, cursorType, startMousePosition);
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
    
    #endregion

    #region Add card effects

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

    #endregion
}
