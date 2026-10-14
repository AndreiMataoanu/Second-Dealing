using System;
using UnityEngine;

public class ClickableCard : Clickable
{
    [SerializeField] private Color outlineUse = Color.blue;
    [SerializeField] private Color outlineCantUse = Color.red;
    private CardInstance cardInstance;
    private BlackjackGame blackjackGame;
    private Action cardAction;
    private Action<CardInstance> cardEffect;
    private Action mouseEnterAction;
    private Action mouseExitAction;
    
    public void AddCardAction(Action action) => cardAction += action;
    public void RemoveCardAction(Action action) => cardAction -= action;
    public void AddCardEffect(Action<CardInstance> action) => cardEffect += action;
    public void RemoveCardEffect(Action<CardInstance> action) => cardEffect -= action;
    public void AddMouseEnterAction(Action action) => mouseEnterAction += action;
    public void RemoveMouseEnterAction(Action action) => mouseEnterAction -= action;
    public void AddMouseExitAction(Action action) => mouseExitAction += action;
    public void RemoveMouseExitAction(Action action) => mouseExitAction -= action;

    public void SetCardInstance(CardInstance instance) => cardInstance = instance;
    public void SetBlackjackGame(BlackjackGame blackjack) => blackjackGame = blackjack;

    public void RemoveCardEffect() => cardEffect = null;
    public void RemoveMouseEnterAction() => mouseEnterAction = null;
    public void RemoveMouseExitAction() => mouseExitAction = null;

    protected override void OnMouseEnter()
    {
        if (!IsActive) return;
        
        base.OnMouseEnter();
        mouseEnterAction?.Invoke();
    }

    protected override void OnMouseExit()
    {
        if (!IsActive) return;
        
        base.OnMouseExit();
        mouseExitAction?.Invoke();
    }

    public override void OnClick(int mouseButton = 0)
    {
        if(!IsActive || mouseButton != 0) return;
        
        base.OnClick();

        cardEffect?.Invoke(cardInstance);
        cardAction?.Invoke();
    }

    protected override Color GetOutlineColor()
    {
        if(cardInstance != null && cardInstance.tarotData != null)
        {
            if(blackjackGame != null && blackjackGame.ShopManager.IsInventoryFull)
            {
                return outlineCantUse;
            }

            return outlineUse;
        }

        return base.GetOutlineColor();
    }
}