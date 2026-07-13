using System;
using UnityEngine;

public abstract class Item : Clickable
{
    [SerializeField] protected int basePrice;
    [Tooltip("Percentage cost. Example: 0.1 = 10%")]
    [SerializeField] private float percentagePrice = 0.1f;
    [Tooltip("Subtract percentage of current price on resale.")]
    [Range(0, 1)] [SerializeField] private float resaleLossPercentage = 0.5f;
    [SerializeField] public ItemType type;
    [Tooltip("Higher number means more common.")]
    [SerializeField] public int spawnWeight = 10;
    
    [HideInInspector] public bool isPurchased;
    [HideInInspector] public bool delayDestroy = false;
    private float multiplier = 1.0f;

    internal CardEffectActions cardEffect;

    private Action<Item> itemAction;
    protected BlackjackGame blackjackGame;

    #region Setters

    public void AddAction(Action<Item> action) => itemAction += action;

    public void RemoveAction(Action<Item> action) => itemAction -= action;
    
    public void SetBlackjackGame(BlackjackGame blackjack)
    {
        blackjackGame = blackjack;
    }

    public virtual void SetMembers() {}

    public void SetMultiplier(float value = 1.0f)
    {
        if (type == ItemType.Coin) return;
        multiplier = value;
    }

    #endregion

    #region Override Methods
    
    public override void OnClick(int mouseButton = 0)
    {
        if(!IsActive) return;
        
        base.OnClick();

        itemAction?.Invoke(this);
    }

    protected override string GetTooltipHeader()
    {
        if(!isPurchased)
        {
            return $"{base.GetTooltipHeader()} [${GetPrice()}]";
        }

        return base.GetTooltipHeader();
    }
    
    #endregion

    #region Activate
    
    public abstract bool Activate();
    
    #endregion

    #region Price

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

    #endregion

    #region Passive Effects

    public virtual void OnRoundEnd() {}
    
    public virtual void OnRoundStart() {}
    
    public virtual void ActivatePassive() {}
    
    public virtual void DeactivatePassive() {}
    
    #endregion
}
