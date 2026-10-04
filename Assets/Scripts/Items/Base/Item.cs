using System;
using UnityEngine;

public abstract class Item : Clickable
{
    [Header("Outlines")]
    [SerializeField] private Color outlineBuy = Color.green;
    [SerializeField] private Color outlineUse = Color.blue;
    [SerializeField] private Color outlineCantUse = Color.red;
    [SerializeField] private Color outlineSell = Color.purple;

    [Header("Shop Stats")]
    [SerializeField] protected int basePrice;
    [Tooltip("Subtract percentage of current price on resale.")]
    [Range(0, 1)] [SerializeField] private float resaleLossPercentage = 0.5f;
    [SerializeField] public ItemType type;
    [Tooltip("Higher number means more common.")]
    [SerializeField] public int spawnWeight = 10;

    [Header("Hover Animation")]
    [SerializeField] private Vector3 hoverPositionOffset = new Vector3(0f, 0.03f, 0f);
    [SerializeField] private Vector3 hoverRotationOffset = new Vector3(-30f, 0f, 0f);
    [SerializeField] private float hoverAnimationSpeed = 10f;
    [SerializeField] private float hoverRadius = 100f;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Vector3 originalWorldPosition;

    [HideInInspector] public bool isPurchased;
    [HideInInspector] public bool delayDestroy;
    protected bool isCardSelecting;
    private bool isHovering;
    private bool positionInitialized;

    private float coinMultiplier = 1.0f;

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
        coinMultiplier = value;
    }

    #endregion

    protected virtual void Update()
    {
        if(!positionInitialized) return;

        if(isHovering)
        {
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(originalWorldPosition);

            float distance = Vector2.Distance(Input.mousePosition, new Vector2(screenPoint.x, screenPoint.y));

            if(distance > hoverRadius)
            {
                isHovering = false;
            }
        }

        Vector3 targetPosition = isHovering ? originalLocalPosition + hoverPositionOffset : originalLocalPosition;
        Quaternion targetRotation = isHovering ? originalLocalRotation * Quaternion.Euler(hoverRotationOffset) : originalLocalRotation;

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * hoverAnimationSpeed);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * hoverAnimationSpeed);
    }

    #region Override Methods

    protected override void OnMouseEnter()
    {
        base.OnMouseEnter();

        if(isPurchased && blackjackGame != null && blackjackGame.isRoundActive)
        {
            if(!positionInitialized)
            {
                originalLocalPosition = transform.localPosition;
                originalLocalRotation = transform.localRotation;
                originalWorldPosition = transform.position;
                positionInitialized = true;
            }

            isHovering = true;
        }
    }

    protected override void OnMouseExit()
    {
        base.OnMouseExit();
    }

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

    protected override Color GetOutlineColor()
    {
        if(!isPurchased)
        {
            bool hasEnoughMoney = blackjackGame.PlayerMoney >= GetPrice();

            if(hasEnoughMoney)
            {
                return outlineBuy;
            }

            return outlineCantUse;
        }

        if(!blackjackGame.isRoundActive)
        {
            return outlineSell;
        }

        if(blackjackGame.isActionLocked)
        {
            if(blackjackGame.UseAfterStand)
            {
                return outlineUse;
            }

            return outlineCantUse;
        }

        return outlineUse;
    }

    #endregion

    #region Activate

    public abstract bool Activate();

    protected virtual void OnCancelCardEffect()
    {
        if (cardEffect == null || !isCardSelecting) return;

        IsActive = true;
        cardEffect.OnCancelSelect();
        SetVisibility(true);
        blackjackGame.ItemManager.UndoItemToRemove(this);
        isCardSelecting = false;
    }
    
    #endregion

    #region Price

    public int GetPrice()
    {
        float discount = 0f;

        if(KeepsakeManager.instance != null)
        {
            discount = KeepsakeManager.instance.GetShopDiscount();
        }

        int finalPrice = Mathf.RoundToInt(basePrice * coinMultiplier * blackjackGame.ShopManager.priceMultiplier * (1f - discount));

        return Mathf.Max(1, finalPrice);
    }

    public int GetResalePrice()
    {
        if(!blackjackGame) return basePrice;

        return GetPrice() - Mathf.RoundToInt(GetPrice() * resaleLossPercentage);
    }

    #endregion

    #region Passive Effects

    public virtual void OnRoundEnd() {}
    
    public virtual void OnRoundStart() {}
    
    public virtual void ActivatePassive() {}
    
    public virtual void DeactivatePassive() {}
    
    #endregion
}
