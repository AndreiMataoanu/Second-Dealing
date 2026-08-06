using System;

public class ClickableCard : Clickable
{
    private int index;
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
}