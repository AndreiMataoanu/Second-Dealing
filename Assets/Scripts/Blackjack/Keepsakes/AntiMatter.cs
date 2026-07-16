using Managers;
using UnityEngine;

[CreateAssetMenu(fileName = "AntiMatter", menuName = "Keepsakes/Anti Matter")]
public class AntiMatter : Keepsake
{
    public static bool isAntiMatterActive;

    private CardEffectActions cardEffect;
    private BlackjackGame game;
    private int usesThisRound = 0;

    #region Setup

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
            game.CursorFollow,
            game.CursorDetection,
            CursorType.None,
            CardTrigger.AntiMatter
        );
    }

    #endregion

    #region Activate AntiMatter

    public override bool ActivateTableEffect(BlackjackGame game)
    {
        if(usesThisRound >= 1) return false;

        return ActivateAntiMatter();
    }
    
    private bool ActivateAntiMatter()
    {
        if(!game.isRoundActive || game.isActionLocked || isAntiMatterActive) return false;

        isAntiMatterActive = true;
        cardEffect.SelectCard();
        cardEffect.AddCardEffectAction(OnAntiMatterCard);
        usesThisRound++;

        return true;
    }
    
    private void OnAntiMatterCard(CardInstance cardInstance)
    {
        AudioManager.instance.Play("ItemBuy");

        cardEffect.OnCardSelected();
        ApplyAntiMatterToCard(cardInstance);
        isAntiMatterActive = false;
    }

    private void ApplyAntiMatterToCard(CardInstance cardInstance)
    {
        if(game.dealerHand.Contains(cardInstance))
            KeepsakeUnlockProgression.instance.AddStat(ChallengeType.AlterDealerHand);

        if(!CardEffects.AddAntiMatterCard(cardInstance))
            CardEffects.RemoveAntiMatterCard(cardInstance);
        
        bool isNowNegative = CardEffects.IsCardNegative(cardInstance.cardData);
        cardInstance.displayComponent.SetNegativeVisual(isNowNegative);
        
        game.UpdateUI();
    }

    #endregion
    
}