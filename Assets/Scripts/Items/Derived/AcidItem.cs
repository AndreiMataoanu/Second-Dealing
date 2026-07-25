using System.Collections;
using Managers;
using UnityEngine;
using Utils;

public class AcidItem : Item
{
    [SerializeField] private float dissolveTime = 1.3f;
    [SerializeField] private Color color = Color.green;
    [SerializeField] private float dissolveBorder = 1.1f;
    public static bool isAcidActive;

    private TableCards tableCards;

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
        
        cardEffect.OnCardSelected();
        StartCoroutine(DissolveCard(cardInstance));
    }
    
    // TODO: revise after finishing table cards class
    private IEnumerator DissolveCard(CardInstance cardInstance)
    {
        CardEffects.SetDissolvedVisual(cardInstance.displayComponent, dissolveTime, color,dissolveBorder);
        
        yield return new WaitForSeconds(dissolveTime);
        
        tableCards.DestroyCard(cardInstance);
        blackjackGame.EvaluateDoubleDownCondition();
        
        yield return null;
    }
}
