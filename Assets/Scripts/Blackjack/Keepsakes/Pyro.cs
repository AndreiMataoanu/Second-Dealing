using Managers;
using UnityEngine;

[CreateAssetMenu(fileName = "Pyro", menuName = "Keepsakes/Pyro")]
public class Pyro : Keepsake
{
    public static bool isPyroActive;
    
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
            CardTrigger.Pyro
        );
    }

    #endregion

    #region Activate Pyro

    public override bool ActivateTableEffect()
    {
        if(usesThisRound >= 1) return false;
        
        return ActivatePyro();
    }
    
    private bool ActivatePyro()
    {
        if(!game.isRoundActive || game.isActionLocked || isPyroActive) return false;

        isPyroActive = true;
        cardEffect.SelectCard();
        cardEffect.AddCardEffectAction(OnBurnCard);
        usesThisRound++;

        return true;
    }
    
    private void OnBurnCard(CardInstance cardInstance)
    {
        AudioManager.instance.Play("ItemBuy");

        cardEffect.OnCardSelected();
        DestroyCard(cardInstance);
        isPyroActive = false;
    }
    
    // TODO: revise, same as acid code
    private void DestroyCard(CardInstance cardInstance)
    {
        game.GameDeck.AddRemovedCard(cardInstance.cardData.rank, cardInstance.cardData.suit);
        CardEffects.RemoveCutCard(cardInstance);
        CardEffects.RemoveAlcoholCard(cardInstance);
        
        var cardObject = cardInstance.displayComponent.gameObject;
        game.activeCardObjects.Remove(cardObject);
        game.GameDeck.AddRemovedCard(cardInstance.cardData.rank, cardInstance.cardData.suit); // TODO: move to card effects

        if (game.dealerHand.Remove(cardInstance))
        {
            KeepsakeUnlockProgression.instance.AddStat(ChallengeType.AlterDealerHand);
            game.UpdateHandVisuals(game.dealerHand, false);
        }
        
        game.playerHands.ForEach(hand =>
        {
            hand.Remove(cardInstance);
            game.UpdateHandVisuals(hand, true);
        });
        
        if (cardInstance == game.peekCardInstance)
            game.peekCardInstance = null;
        
        Destroy(cardObject);
        game.UpdateUI();
        game.EvaluateDoubleDownCondition();
    }

    #endregion
    
}