using System;
using UnityEngine;

public class ClickableCard : Clickable
{
    [SerializeField] private Material outlineUse;
    [SerializeField] private Material outlineCantUse;
    private CardInstance cardInstance;
    private BlackjackGame blackjackGame;
    private Action cardAction;
    private Action<CardInstance> cardEffect;
    
    public void AddCardAction(Action action) => cardAction += action;
    public void RemoveCardAction(Action action) => cardAction -= action;
    public void AddCardEffect(Action<CardInstance> action) => cardEffect += action;
    public void RemoveCardEffect(Action<CardInstance> action) => cardEffect -= action;

    public void SetCardInstance(CardInstance instance) => cardInstance = instance;
    public void SetBlackjackGame(BlackjackGame blackjack) => blackjackGame = blackjack;

    public void RemoveCardEffect() => cardEffect = null;

    public override void OnClick(int mouseButton = 0)
    {
        if(!IsActive || mouseButton != 0) return;
        
        base.OnClick();

        cardEffect?.Invoke(cardInstance);
        cardAction?.Invoke();
    }

    protected override Material GetOutlineMaterial()
    {
        if(cardInstance != null && cardInstance.tarotData != null)
        {
            if(blackjackGame != null && blackjackGame.ShopManager.IsInventoryFull)
            {
                return outlineCantUse;
            }

            return outlineUse;
        }

        return base.GetOutlineMaterial();
    }
}