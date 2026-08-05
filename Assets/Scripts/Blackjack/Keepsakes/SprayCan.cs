using Managers;
using UnityEngine;

[CreateAssetMenu(fileName = "SprayCan", menuName = "Keepsakes/Spray Can")]
public class SprayCan : Keepsake
{
    private CardEffectActions cardEffect;
    private BlackjackGame blackjackGame;

    public override void SetMembers(BlackjackGame game)
    {
        blackjackGame = game;
        cardEffect = new CardEffectActions(
            game,
            game.CursorFollow,
            game.CursorDetection,
            CursorType.None,
            CardTrigger.SprayCan
        );
    }

    public override bool ActivateTableEffect()
    {
        if(!blackjackGame.isRoundActive || blackjackGame.isActionLocked) return false;

        cardEffect.SelectCard();
        cardEffect.AddItemCardEffectAction(OnApplyEffect);

        return true;
    }

    private void OnApplyEffect(CardInstance cardInstance)
    {
        AudioManager.instance.Play("ItemBuy");
        CardEffects.ToggleColorSwap(cardInstance.cardData);
        CardEffects.SetVisualEffects(cardInstance, cardInstance.isHidden, true, true);

        cardEffect.OnCardSelected();
    }
}