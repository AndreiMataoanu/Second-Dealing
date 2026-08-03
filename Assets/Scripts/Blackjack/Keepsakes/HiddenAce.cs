using Managers;
using UnityEngine;

[CreateAssetMenu(fileName = "HiddenAce", menuName = "Keepsakes/Hidden Ace")]
public class HiddenAce : Keepsake
{
    public GameObject tokenPrefab;
    private CardEffectActions cardEffect;
    private BlackjackGame blackjackGame;

    public override void SetMembers(BlackjackGame game)
    {
        blackjackGame = game;
        cardEffect = new CardEffectActions(
            game,
            game.CursorFollow,
            game.CursorDetection,
            CursorType.HiddenAce,
            CardTrigger.HiddenAce
        );
    }

    public override bool ActivateTableEffect()
    {
        if(!blackjackGame.isRoundActive || blackjackGame.isActionLocked) return false;

        cardEffect.SelectCard();
        cardEffect.AddCardEffectAction(OnApplyEffect);

        return true;
    }

    private void OnApplyEffect(CardInstance cardInstance)
    {
        AudioManager.instance.Play("Coin(Clone)");
        CardEffects.AddHiddenAce(cardInstance, 1);

        Instantiate(tokenPrefab, cardInstance.displayComponent.transform);

        cardEffect.OnCardSelected();
    }
}