using System;
using UnityEngine;

public class ClickableCard : Clickable
{
    private int index;
    private CardInstance cardInstance;
    private BlackjackGame blackjackGame;
    
    private Action cardAction;
    public void AddAction(Action action) => cardAction += action;
    public void RemoveAction(Action action) => cardAction -= action;

    public void SetCardInstance(CardInstance instance) => cardInstance = instance;
    public void SetBlackjackGame(BlackjackGame blackjack) => blackjackGame = blackjack;

    public void OnCutCard()
    {
        AudioManager.instance.Play("Scissors(Clone)");
        cardInstance.displayComponent.SetCutVisual(true);
        
        int originalValue;
        
        if(cardInstance.cardData.rank == Card.Rank.Joker)
        {
            originalValue = 0;
        }
        else
        {
            originalValue = cardInstance.cardData.GetValue();
        
            if(blackjackGame.IsDoubleLowActive() && originalValue < 6)
            {
                originalValue = originalValue + originalValue;
            }
        
            if(blackjackGame.IsHalfHighActive() && originalValue > 5)
            {
                originalValue = Mathf.CeilToInt(originalValue / 2f);
            }
        
            if(blackjackGame.GetNegativeSuits().Contains(cardInstance.cardData.suit))
            {
                originalValue = -originalValue;
            }
        }
        
        int halvedValue = Mathf.CeilToInt((float)Mathf.Abs(originalValue) / 2f);

        blackjackGame.ApplyCutToCard(cardInstance, Mathf.Abs(originalValue) - halvedValue);
        blackjackGame.SetScissorsActive(false);
        blackjackGame.UpdateUI(true);
    }

    public override void OnClick(int mouseButton = 0)
    {
        if (!IsActive) return;
        
        base.OnClick();
        cardAction?.Invoke();
    }
}
