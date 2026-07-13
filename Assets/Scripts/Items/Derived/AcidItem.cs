using System.Collections;
using Managers;
using UnityEngine;
using Utils;

public class AcidItem : Item
{
    [SerializeField] private float dissolveTime = 1.3f;
    public static bool isAcidActive;

    public override void SetMembers()
    {
        cardEffect = new CardEffectActions(
            blackjackGame,
            blackjackGame.CursorFollow,
            blackjackGame.CursorDetection,
            CursorType.Acid,
            CardTrigger.Acid
        );
    }
    
    public override bool Activate()
    {
        return ActivateAcid();
    }
    
    private bool ActivateAcid()
    {
        if(!blackjackGame.isRoundActive || isAcidActive || blackjackGame.CheckItemAfterStand()) return false;

        isAcidActive = true;
        cardEffect.SelectCard();
        cardEffect.AddCardEffectAction(OnDissolveCard);
 
        return true;
    }
    

    private void OnDissolveCard(CardInstance cardInstance)
    {
        isAcidActive = false;
        
        // CardEffects.UseDissolveShaderOrSmth(); 
        // CardEffects.AddAcidCard(cardInstance);
        cardEffect.OnCardSelected();
        StartCoroutine(DissolveCard(cardInstance));
    }
    
    // TODO: revise after finishing table cards class
    private IEnumerator DissolveCard(CardInstance cardInstance)
    {
        yield return new WaitForSeconds(dissolveTime);
        
        var cardObject = cardInstance.displayComponent.gameObject;
        blackjackGame.activeCardObjects.Remove(cardObject);
        blackjackGame.GameDeck.AddRemovedCard(cardInstance.cardData.rank, cardInstance.cardData.suit); // TODO: move to card effects

        blackjackGame.dealerHand.Remove(cardInstance);
        blackjackGame.playerHands.ForEach(hand => hand.Remove(cardInstance));
        if (cardInstance == blackjackGame.peekCardInstance)
            blackjackGame.peekCardInstance = null;
        
        Destroy(cardObject);
        blackjackGame.UpdateUI();
        
        yield return null;
    }
}
