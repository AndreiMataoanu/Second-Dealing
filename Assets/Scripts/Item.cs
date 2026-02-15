using System;
using UnityEngine;

public class Item : Clickable
{
    [SerializeField] public int price;
    [SerializeField] public ItemType type;
    [Tooltip("Higher number means more common.")]
    [SerializeField] public int spawnWeight = 10;

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
            case ItemType.Knife:
                blackjackGame.ActivateKnife();
                break;
            case ItemType.Scissors:
                blackjackGame.ActivateScissors();
                break;
            case ItemType.Crucifix:
                blackjackGame.ActivateCrucifix();
                break;
            case ItemType.Sunglasses:
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
        if (!IsActive) return;
        
        base.OnClick();
        itemAction?.Invoke(this);
    }
}
