using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
    [SerializeField] private TextMeshProUGUI lottoWorldText;
    private int lastLottoCount = -1;
    private Action<Item> itemAction;
    private BlackjackGame blackjackGame;

    public void AddAction(Action<Item> action) => itemAction += action;

    public void RemoveAction(Action<Item> action) => itemAction -= action;

    private void Update()
    {
        if(type == ItemType.Lotto && isPurchased && blackjackGame != null && blackjackGame.isLottoActive)
        {
            if(lottoWorldText != null)
            {
                List<int> numbers = blackjackGame.GetLotteryNumbers();

                if(numbers.Count != lastLottoCount)
                {
                    lottoWorldText.text = string.Join("  ", numbers);
                    lastLottoCount = numbers.Count;
                }
            }
        }
    }

    public bool Activate()
    {
        var result = type switch
        {
            ItemType.Knife => blackjackGame.ActivateKnife(),
            ItemType.Scissors => blackjackGame.ActivateScissors(),
            ItemType.Crucifix => blackjackGame.ActivateCrucifix(),
            ItemType.Sunglasses => blackjackGame.ActivateSunglasses(),
            ItemType.Organ => false,
            ItemType.Cigarette => blackjackGame.ActivateCigarette(),
            ItemType.Alcohol => blackjackGame.ActivateAlcohol(),
            ItemType.Fan => blackjackGame.ActivateFan(),
            ItemType.Lotto => blackjackGame.TearLotteryTicket(),
            ItemType.Acid => blackjackGame.ActivateAcid(),
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
            return Mathf.RoundToInt(money * percentagePrice);
        }

        return basePrice;
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
        if(type == ItemType.Lotto && blackjackGame.isLottoActive)
        {
            List<int> numbers = blackjackGame.GetLotteryNumbers();

            return $"{string.Join(" | ", numbers)}\nFinish your hand with these values to win\nClick to tear";
        }

        if(type == ItemType.Organ && blackjackGame.isOrganActive)
        {
            return $"Passive: Sacrifice instead of your life\nExpires in: {blackjackGame.GetOrganRoundsLeft()} rounds";
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
}
