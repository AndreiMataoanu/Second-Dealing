using System.Linq;
using Managers;
using UnityEngine;

[CreateAssetMenu(fileName = "HatTrick", menuName = "Keepsakes/Hat Trick")]
public class HatTrick : Keepsake
{
    public static bool isHatTrickActive = false;
    
    private CardEffectActions cardEffect;
    private BlackjackGame game;
    private TableCards tableCards;
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
        tableCards = blackjackGame.TableCards;
        cardEffect = new CardEffectActions(
            game,
            CursorType.None,
            CardTrigger.HatTrick
        );
    }

    #endregion

    #region Activate Hat Trick

    public override bool ActivateTableEffect()
    {
        if(usesThisRound >= 1) return false;

        return ActivateHatTrick();
    }
    
    private bool ActivateHatTrick()
    {
        if(!game.isRoundActive || game.isActionLocked || isHatTrickActive) return false;

        isHatTrickActive = true;
        cardEffect.SelectCard();
        cardEffect.AddItemCardEffectAction(TryHatTrickCard);
        usesThisRound++;
        
        return true;
    }

    private void TryHatTrickCard(CardInstance cardInstance)
    {
        if (!CheckHatTrickValid(cardInstance)) return;

        AddHatTrickCard(cardInstance);        
    }

    private bool CheckHatTrickValid(CardInstance cardInstance)
    {
        bool isValidTarget = tableCards.PlayerHands.Any(hand => hand.Contains(cardInstance));

        if(!isValidTarget && tableCards.DealerHand.Contains(cardInstance))
            isValidTarget = true;

        if (isValidTarget && !cardInstance.isHidden) return true;
        
        AudioManager.instance.Play("ItemDeny");
        return false;
    }

    private void AddHatTrickCard(CardInstance cardInstance)
    {
        AudioManager.instance.Play("ItemBuy");
        
        game.isActionLocked = true;
        game.canDoubleDown = false;
        
        tableCards.GameDeck.AddCardCopies(cardInstance.cardData, 1);
        game.HandleNewCardInPlayerHand(cardInstance);
        cardEffect.OnCardSelected();
    }

    #endregion
}