using Managers;
using UnityEngine;

public class ScissorsItem : Item
{
    public static bool isScissorsActive;

    private TableCards tableCards;

    public override bool Activate()
    {
        return ActivateScissors();
    }

    private bool ActivateScissors()
    {
        if(!blackjackGame.isRoundActive || isScissorsActive || blackjackGame.CheckItemAfterStand()) return false;

        isCardSelecting = true;
        isScissorsActive = true;
        cardEffect.SelectCard();
        cardEffect.AddItemCardEffectAction(OnCutCard);
        
        return true;
    }
    
    private void OnCutCard(CardInstance cardInstance)
    {
        if(tableCards.DealerHand.Contains(cardInstance))
        {
            KeepsakeUnlockProgression.instance.AddStat(ChallengeType.AlterDealerHand);
        }

        AudioManager.instance.Play("Scissors(Clone)");

        isScissorsActive = false;
        isCardSelecting = false;
        
        CardEffects.AddCutCard(cardInstance, 2);
        cardEffect.OnCardSelected();
    }

    public override void SetMembers()
    {
        delayDestroy = true;
        tableCards = blackjackGame.TableCards;
        cardEffect = new CardEffectActions(
            blackjackGame,
            CursorType.Scissors,
            CardTrigger.Scissors
        );
    }

    protected override void OnCancelCardEffect()
    {
        base.OnCancelCardEffect();
        isScissorsActive = false;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            OnCancelCardEffect();
        }
    }
}
