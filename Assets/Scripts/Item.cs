using System;
using UnityEngine;

public class Item : Clickable
{
    [SerializeField] public int price;
    [SerializeField] public PowerUpType type;

    private Action<Item> itemAction;
    private BlackjackGame blackjackGame;

    public void AddAction(Action<Item> action) => itemAction += action;
    public void RemoveAction(Action<Item> action) => itemAction -= action;

    public void Activate()
    {
        if(!blackjackGame)
        {
            Debug.Log("No blackjack game");
            return;
        }
        
        switch(type)
        {
            case PowerUpType.Knife:
                blackjackGame.ActivateKnife();
                break;
            case PowerUpType.Scissors:
                blackjackGame.ActivateScissors();
                break;
            case PowerUpType.Crucifix:
                blackjackGame.ActivatePrayerBeads();
                break;
            case PowerUpType.Sunglasses:
                blackjackGame.ActivateSunglasses();
                break;
        }
    }

    public void SetBlackjackGame(BlackjackGame blackjack)
    {
        blackjackGame = blackjack;
    }

    public override void OnClick()
    {
        base.OnClick();
        itemAction.Invoke(this);
    }
}
