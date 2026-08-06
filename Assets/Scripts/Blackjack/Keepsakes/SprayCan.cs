using Managers;
using UnityEngine;

[CreateAssetMenu(fileName = "SprayCan", menuName = "Keepsakes/Spray Can")]
public class SprayCan : Keepsake
{
    private CardEffectActions cardEffect;
    private BlackjackGame game;
    public static bool isSprayCanActive;
    private bool isCardSelecting;
    private int usesThisRound = 0;

    private void OnEnable()
    {
        usesThisRound = 0;
    }

    public override void OnRoundStart()
    {
        usesThisRound = 0;
    }

    public override void SetMembers(BlackjackGame blackjackGame)
    {
        game = blackjackGame;
        cardEffect = new CardEffectActions(
            game,
            CursorType.SprayCan,
            CardTrigger.SprayCan
        );
    }

    public override bool ActivateTableEffect()
    {
        if(usesThisRound >= 1) return false;

        return ActivateSprayCan();
    }

    private bool ActivateSprayCan()
    {
        if(!game.isRoundActive || game.isActionLocked || isSprayCanActive) return false;
        
        isSprayCanActive = true;
        isCardSelecting = true;
        cardEffect.SelectCard();
        cardEffect.AddItemCardEffectAction(OnSprayCard);
        usesThisRound++;
        
        return true;
    }

    private void OnSprayCard(CardInstance cardInstance)
    {
        AudioManager.instance.Play("ItemBuy");
        CardEffects.ToggleColorSwap(cardInstance.cardData);
        CardEffects.SetVisualEffects(cardInstance, cardInstance.isHidden, true, true);

        cardEffect.OnCardSelected();
        isSprayCanActive = false;
        isCardSelecting = false;
    }

    public override bool OnCancel()
    {
        if(cardEffect == null || !isCardSelecting) return false;

        usesThisRound--;
        cardEffect.OnCancelSelect();
        isCardSelecting = false;
        isSprayCanActive = false;

        return true;
    }
}