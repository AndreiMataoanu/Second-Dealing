using System.Collections;
using Managers;
using UnityEngine;

public class AcidItem : Item
{
    [SerializeField] private float dissolveTime = 1.5f;
    [SerializeField] private Color color = Color.green;
    [SerializeField] private float dissolveBorder = 1.1f;
    public static bool isAcidActive;

    private TableCards tableCards;

    public override void SetMembers()
    {
        delayDestroy = true;
        tableCards = blackjackGame.TableCards;
        cardEffect = new CardEffectActions(
            blackjackGame,
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
        cardEffect.AddItemCardEffectAction(OnDissolveCard);

        return true;
    }
    
    private void OnDissolveCard(CardInstance cardInstance)
    {
        if(tableCards.DealerHand.Contains(cardInstance))
            KeepsakeUnlockProgression.instance.AddStat(ChallengeType.AlterDealerHand);

        AudioManager.instance.Play("Acid(Clone)");

        isAcidActive = false;

        cardEffect.OnCardSelected();
        StartCoroutine(DissolveCard(cardInstance));
    }
    
    private IEnumerator DissolveCard(CardInstance cardInstance)
    {
        CardEffects.SetDissolvedVisual(cardInstance.displayComponent, dissolveTime, color,dissolveBorder);
        
        yield return new WaitForSeconds(dissolveTime);
        
        tableCards.DestroyCard(cardInstance);
        blackjackGame.EvaluateDoubleDownCondition();
        
        yield return null;
    }
}
