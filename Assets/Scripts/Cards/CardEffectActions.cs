using System;
using Managers;
using UnityEngine;

public class CardEffectActions
{
    private BlackjackGame blackjackGame; // TODO: possibly remove
    private CursorFollow cursorFollow;
    private CursorDetection cursorDetection;
    private CursorType cursorType;
    private CardTrigger cardTrigger;

    // Keep cursor objects as parameters for blackjack game removal
    public CardEffectActions(BlackjackGame blackjackGame, CursorFollow cursorFollow, CursorDetection cursorDetection,
        CursorType cursorType, CardTrigger cardTrigger)
    {
        this.blackjackGame = blackjackGame;
        this.cursorFollow = cursorFollow;
        this.cursorDetection = cursorDetection;
        this.cursorType = cursorType;
        this.cardTrigger = cardTrigger;
    }

    public void SelectCard()
    {
        cursorFollow.SetCursorTypeActive(true, cursorType);
    }
    
    public void OnCardSelected()
    {
        cursorFollow.SetCursorTypeActive(false, cursorType);
        blackjackGame.UpdateUI();
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
