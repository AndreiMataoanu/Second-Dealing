using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tarot", menuName = "Keepsakes/Tarot")]
public class Tarot : Keepsake
{
    public static bool isTarotActive = false;
    
    private CardEffectActions cardEffect;
    private BlackjackGame game;

    #region Setup

    public override void SetMembers(BlackjackGame blackjackGame)
    {
        isTarotActive = true;
        game = blackjackGame;
    }
    
    public override void Deactivate()
    {
        isTarotActive = false;
    }

    public override void OnRoundStart()
    {
        game.CursorDetection.ResetTarotClickables();
    }
    
    public override void OnAdvanceHand()
    {
        game.CursorDetection.ResetTarotClickables(); //deactivate previous hand
        if (!game.IsPlayerHandValid) return;
        game.CurrentHand.ForEach(AddTarotCard);
    }

    #endregion

    #region Activate Tarot

    public override bool AddTarotCards()
    {
        isTarotActive = true;
        return true;
    }

    public override void OnDealPlayerCard(CardInstance cardInstance)
    {
        AddTarotCard(cardInstance);
    }

    private void AddTarotCard(CardInstance cardInstance)
    {
        var clickable = game.CursorDetection.AddTarotClickable(game, cardInstance);
        clickable?.AddCardEffect(SacrificeTarot);
    }

    private void SacrificeTarot(CardInstance cardInstance)
    {
        if(!game.isRoundActive || game.isActionLocked || !game.IsPlayerHandValid) return;
    
        TarotCard tarotData = cardInstance.tarotData;
    
        if(!tarotData || !tarotData.rewardItemPrefab)
        {
            AudioManager.instance.Play("ItemDeny");
    
            return;
        }

        if (!game.ItemManager.SpawnInventoryItem(tarotData.rewardItemPrefab)) return;
    
        CardEffects.RemoveCutCard(cardInstance);
        CardEffects.RemoveAlcoholCard(cardInstance);
    
        game.CurrentHand.Remove(cardInstance);
        game.activeCardObjects.Remove(cardInstance.CardObject);
    
        Destroy(cardInstance.CardObject);
    
        game.UpdateHandVisuals(game.CurrentHand, true);
        game.UpdateUI();
    }

    #endregion
}