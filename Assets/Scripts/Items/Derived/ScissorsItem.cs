using Managers;

public class ScissorsItem : Item
{
    public static bool isScissorsActive;

    private TableCards tableCards;

    public override bool Activate()
    {
        SetMembers();
        return ActivateScissors();
    }

    private bool ActivateScissors()
    {
        if(!blackjackGame.isRoundActive || isScissorsActive || blackjackGame.CheckItemAfterStand()) return false;

        isScissorsActive = true;
        cardEffect.SelectCard();
        cardEffect.AddCardEffectAction(OnCutCard);
        
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
        
        CardEffects.AddCutCard(cardInstance, 2);
        cardEffect.OnCardSelected();
    }

    public override void SetMembers()
    {
        tableCards = blackjackGame.TableCards;
        cardEffect = new CardEffectActions(
            blackjackGame,
            blackjackGame.CursorFollow,
            blackjackGame.CursorDetection,
            CursorType.Scissors,
            CardTrigger.Scissors
        );
    }
}
