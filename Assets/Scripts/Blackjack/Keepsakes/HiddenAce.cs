using Managers;
using UnityEngine;

[CreateAssetMenu(fileName = "HiddenAce", menuName = "Keepsakes/Hidden Ace")]
public class HiddenAce : Keepsake
{
    public GameObject tokenPrefab;
    private CardEffectActions cardEffect;
    private BlackjackGame game;
    public static bool isHiddenAceActive;
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
            CursorType.HiddenAce,
            CardTrigger.HiddenAce
        );
    }

    public override bool ActivateTableEffect()
    {
        if(usesThisRound >= 1) return false;

        return ActivateHiddenAce();
    }

    private bool ActivateHiddenAce()
    {
        if(!game.isRoundActive || game.isActionLocked || isHiddenAceActive) return false;
        
        isHiddenAceActive = true;
        isCardSelecting = true;
        cardEffect.SelectCard();
        cardEffect.AddItemCardEffectAction(OnApplyToken);
        usesThisRound++;
        
        return true;
    }

    private void OnApplyToken(CardInstance cardInstance)
    {
        AudioManager.instance.Play("Coin(Clone)");
        CardEffects.AddHiddenAce(cardInstance, 1);

        Instantiate(tokenPrefab, cardInstance.displayComponent.transform);

        cardEffect.OnCardSelected();
        isHiddenAceActive = false;
        isCardSelecting = false;
    }

    public override bool OnCancel()
    {
        if(cardEffect == null || !isCardSelecting) return false;

        usesThisRound--;
        cardEffect.OnCancelSelect();
        isCardSelecting = false;
        isHiddenAceActive = false;

        return true;
    }
}