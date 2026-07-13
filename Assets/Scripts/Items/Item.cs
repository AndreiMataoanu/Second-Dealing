using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Item : Clickable
{
    [SerializeField] private int basePrice;
    [Tooltip("Percentage cost. Example: 0.1 = 10%")]
    [SerializeField] private float percentagePrice = 0.1f;
    [Tooltip("Subtract percentage of current price on resale.")]
    [Range(0, 1)] [SerializeField] private float resaleLossPercentage = 0.5f;
    [SerializeField] public ItemType type;
    [Tooltip("Higher number means more common.")]
    [SerializeField] public int spawnWeight = 10;
    [HideInInspector] public bool isPurchased = false;

    private Action<Item> itemAction;
    private BlackjackGame blackjackGame;
    private int nftRoundsLeft;
    private float multiplier = 1.0f;

    public void AddAction(Action<Item> action) => itemAction += action;

    public void RemoveAction(Action<Item> action) => itemAction -= action;
    
    public void SetNftRoundsLeft() => nftRoundsLeft = Random.Range(2, 4);

    public void SetMultiplier(float value = 1.0f)
    {
        if (type == ItemType.Coin) return;
        multiplier = value;
    }

    public bool Activate()
    {
        var result = type switch
        {
            ItemType.Knife => blackjackGame.ActivateKnife(),
            ItemType.Scissors => blackjackGame.ActivateScissors(),
            ItemType.Crucifix => blackjackGame.ActivateCrucifix(),
            ItemType.Coin => blackjackGame.ActivateCoin(),
            ItemType.Sunglasses => blackjackGame.ActivateSunglasses(),
            ItemType.Organ => false,
            ItemType.Cigarette => blackjackGame.ActivateCigarette(),
            ItemType.Alcohol => blackjackGame.ActivateAlcohol(),
            ItemType.Fan => blackjackGame.ActivateFan(),
            ItemType.Acid => blackjackGame.ActivateAcid(),
            ItemType.Nft => blackjackGame.ActivateNft(basePrice),
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
        if(!IsActive) return;
        
        base.OnClick();

        itemAction?.Invoke(this);
    }

    public int GetPrice()
    {
        if(!blackjackGame) return basePrice;

        int money = blackjackGame.PlayerMoney;

        if(money >= blackjackGame.percentagePriceThreshold)
        {
            return Mathf.RoundToInt(money * percentagePrice * multiplier);
        }

        return Mathf.RoundToInt(basePrice * multiplier);
    }

    public int GetResalePrice()
    {
        if(!blackjackGame) return basePrice;

        int money = blackjackGame.PlayerMoney;

        if(money >= blackjackGame.percentagePriceThreshold)
        {
            var currentPrice = Mathf.RoundToInt(money * percentagePrice);
            currentPrice -= Mathf.RoundToInt(currentPrice * resaleLossPercentage);
            return currentPrice;
        }

        return basePrice - Mathf.RoundToInt(basePrice * resaleLossPercentage);
    }

    protected override string GetTooltipContent()
    {
        if(type == ItemType.Organ && blackjackGame.isOrganActive)
        {
            return $"Passive: Sacrifice instead of your life\nExpires in: {blackjackGame.GetOrganRoundsLeft()} rounds";
        }

        if(type == ItemType.Nft)
        {
            return base.GetTooltipContent() + "\n Current value: " + basePrice;
        }

        return base.GetTooltipContent();
    }

    protected override string GetTooltipHeader()
    {
        if(!isPurchased)
        {
            return $"{base.GetTooltipHeader()} [${GetPrice()}]";
        }

        return base.GetTooltipHeader();
    }

    public void OnRoundStart()
    {
        if(type == ItemType.Nft)
        {
            if(nftRoundsLeft == 0)
                basePrice = 0;
            else
            {
                nftRoundsLeft--;
                basePrice = Random.Range(0, blackjackGame.GetPlayerMoney() * 2);
            }
        }
    }
}
