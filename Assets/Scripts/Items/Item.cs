using System;
using UnityEngine;

public class Item : Clickable
{
    [SerializeField] public int price;
    [SerializeField] public ItemType type;
    [Tooltip("Higher number means more common.")]
    [SerializeField] public int spawnWeight = 10;

    [SerializeField] public int PassiveItemRounds = 2;
    [SerializeField] public bool passive = false;  

    private Action<Item> itemAction;
    private BlackjackGame blackjackGame;

    public void AddAction(Action<Item> action) => itemAction += action;

    public void RemoveAction(Action<Item> action) => itemAction -= action;

    public bool Activate()
    {
        if(!blackjackGame)
        {
            Debug.Log("No blackjack game");
            return false;
        }

        var result = type switch
        {
            ItemType.Knife => blackjackGame.ActivateKnife(),
            ItemType.Scissors => blackjackGame.ActivateScissors(),
            ItemType.Crucifix => blackjackGame.ActivateCrucifix(),
            ItemType.Sunglasses => blackjackGame.ActivateSunglasses(),
            ItemType.Organ => blackjackGame.ActivateOrgan(),
            ItemType.Cigarette => blackjackGame.ActivateCigarette(),
            ItemType.Alcohol => blackjackGame.ActivateAlcohol(),
            ItemType.Fan => blackjackGame.ActivateFan(),
            _ => false
        };

        return result;
    }

    public void SetBlackjackGame(BlackjackGame blackjack)
    {
        blackjackGame = blackjack;
    }

    public override void OnClick(int mouseButton = 0)
    {
        if (!IsActive) return;
        
        base.OnClick();
        itemAction?.Invoke(this);
    }
}
