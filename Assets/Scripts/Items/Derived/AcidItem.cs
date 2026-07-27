using System.Collections;
using Managers;
using UnityEngine;

public class AcidItem : Item
{
    [SerializeField] private float dissolveTime = 1.5f;
    [SerializeField] private Color color = Color.green;
    [SerializeField] private float dissolveBorder = 1.1f;
    public static bool isAcidActive;

    public override void SetMembers()
    {
        delayDestroy = true;
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
        SetMembers();
         
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
        if(blackjackGame.dealerHand.Contains(cardInstance))
        {
            KeepsakeUnlockProgression.instance.AddStat(ChallengeType.AlterDealerHand);
        }

        AudioManager.instance.Play("Acid(Clone)");

        isAcidActive = false;

        CardEffects.SetDissolvedVisual(cardInstance.displayComponent, dissolveTime, color,dissolveBorder);
        // CardEffects.AddAcidCard(cardInstance);
        cardEffect.OnCardSelected();
        StartCoroutine(DissolveCard(cardInstance));
    }
    
    // TODO: revise after finishing table cards class
    private IEnumerator DissolveCard(CardInstance cardInstance)
    {
        yield return new WaitForSeconds(dissolveTime);
        
        var cardObject = cardInstance.displayComponent.gameObject;
        CardEffects.RemoveCutCard(cardInstance);
        CardEffects.RemoveAlcoholCard(cardInstance);
        blackjackGame.activeCardObjects.Remove(cardObject);
        blackjackGame.GameDeck.AddRemovedCard(cardInstance.cardData.rank, cardInstance.cardData.suit); // TODO: move to card effects

        if (blackjackGame.dealerHand.Remove(cardInstance))
        {
            KeepsakeUnlockProgression.instance.AddStat(ChallengeType.AlterDealerHand);
            blackjackGame.UpdateHandVisuals(blackjackGame.dealerHand, false);
        }
        
        blackjackGame.playerHands.ForEach(hand =>
        {
            hand.Remove(cardInstance);
            blackjackGame.UpdateHandVisuals(hand, true);
        });

        if (cardInstance == blackjackGame.peekCardInstance)
            blackjackGame.peekCardInstance = null;
        
        Destroy(cardObject);
        blackjackGame.UpdateUI();
        blackjackGame.EvaluateDoubleDownCondition();
        
        yield return null;
    }
}
