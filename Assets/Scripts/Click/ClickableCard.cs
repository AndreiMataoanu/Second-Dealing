using System;

public class ClickableCard : Clickable
{
    private int index;
    private CardInstance cardInstance;
    
    private Action<Item> cardAction;
    private BlackjackGame blackjackGame;
    public void AddAction(Action<Item> action) => cardAction += action;
    public void RemoveAction(Action<Item> action) => cardAction -= action;

    public void SetCardInstance(CardInstance instance) => cardInstance = instance;
}
