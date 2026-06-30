using System;
using UnityEngine;

public class ClickableCard : Clickable
{
    [Header("Card VFX")] 
    [SerializeField] private float dissolveTime = 1.3f;
    
    private int index;
    private CardInstance cardInstance;
    private BlackjackGame blackjackGame;
    
    private Action cardAction;
    public void AddAction(Action action) => cardAction += action;
    public void RemoveAction(Action action) => cardAction -= action;

    public void SetCardInstance(CardInstance instance) => cardInstance = instance;
    public void SetBlackjackGame(BlackjackGame blackjack) => blackjackGame = blackjack;

    public void OnCutCard()
    {
        AudioManager.instance.Play("Scissors(Clone)");
        cardInstance.displayComponent.SetCutVisual(true);
        
        int originalValue;
        
        if(cardInstance.cardData.rank == Card.Rank.Joker)
        {
            originalValue = 0;
        }
        else
        {
            originalValue = cardInstance.cardData.GetValue();
        
            if(blackjackGame.IsDoubleLowActive() && originalValue < 6)
            {
                originalValue = originalValue + originalValue;
            }
        
            if(blackjackGame.IsHalfHighActive() && originalValue > 5)
            {
                originalValue = Mathf.CeilToInt(originalValue / 2f);
            }
        
            if(blackjackGame.GetNegativeSuits().Contains(cardInstance.cardData.suit))
            {
                originalValue = -originalValue;
            }
        }

        blackjackGame.ApplyCutToCard(cardInstance, 2);
        blackjackGame.SetScissorsActive(false);
        blackjackGame.UpdateUI(true);
    }

    public void OnDissolveCard()
    {
        // TODO: AudioManager.instance.Play("AcidSound");
        // TODO: add shader effect
        
        blackjackGame.ApplyDissolveToCard(cardInstance, dissolveTime);
        
    }

    public override void OnClick(int mouseButton = 0)
    {
        if (!IsActive) return;
        
        base.OnClick();
        cardAction?.Invoke();
    }
}
