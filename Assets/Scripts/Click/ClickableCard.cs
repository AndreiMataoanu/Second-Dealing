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

    public void OnAddCardsOption()
    {
        AudioManager.instance.Play("CardHit");
        blackjackGame.EventManager.AddCardCopies(cardInstance.cardData);

        Destroy(gameObject);
        
        blackjackGame.EventManager.SelectCardCopyEnd();
    }

    public override void OnClick(int mouseButton = 0)
    {
        if(!IsActive) return;
        
        base.OnClick();

        cardEffect?.Invoke(cardInstance);
        cardAction?.Invoke();
        
        if(cardInstance != null && cardInstance.cardData.suit == Card.Suit.Tarot)
            blackjackGame.SacrificeTarot(cardInstance);
    }

    public void OnAntiMatterCard()
    {
        AudioManager.instance.Play("ItemBuy");

        blackjackGame.ApplyAntiMatterToCard(cardInstance);

        bool isNowNegative = blackjackGame.IsCardNegative(cardInstance.cardData);

        cardInstance.displayComponent.SetNegativeVisual(isNowNegative);
        blackjackGame.isAntiMatterTargeting = false;
        blackjackGame.UpdateUI(true);
    }

    public void OnHatTrickCard()
    {
        blackjackGame.TryHatTrickCard(cardInstance);
    }
}