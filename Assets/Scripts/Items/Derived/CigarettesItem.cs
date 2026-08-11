using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CigarettesItem : Item
{
    private TableCards tableCards;
    public static bool isCigaretteActive;

    #region Override

    public override void SetMembers()
    {
        delayDestroy = true;
        tableCards = blackjackGame.TableCards;
    }

    public override bool Activate()
    {
        return ActivateCigarette();
    }

    #endregion

    #region Activate

    private bool ActivateCigarette()
    {
        if(!blackjackGame.isRoundActive || blackjackGame.CheckItemAfterStand() 
                                        || isCigaretteActive 
                                        || blackjackGame.isSplitting) return false;

        isCigaretteActive = true;

        StartCoroutine(CigaretteCoroutine());

        return true;
    }

    private IEnumerator CigaretteCoroutine()
    {
        blackjackGame.isActionLocked = true;

        yield return blackjackGame.StopDealerTurn();
        tableCards.ResetLastActiveHand();
        blackjackGame.SetPlayerStand(false);
    
        blackjackGame.CursorDetection.OnRoundActive();
    
        yield return PlaySmokeAnimation();
        
        yield return tableCards.FlipDealerHiddenCard(0);
        yield return SwitchHands();
    
        tableCards.UpdateSplitOutlines(true);
        blackjackGame.CalculateBust();
        
        StopSmokeAnimation();
    }

    #endregion
    
    #region Smoke Animation
    
    private IEnumerator PlaySmokeAnimation()
    {
        AudioManager.instance.Play("Smoking");
    
        yield return new WaitForSeconds(1f);
    
        blackjackGame.smokeParticle.Play();
    
        yield return new WaitForSeconds(1f);
    }

    private void StopSmokeAnimation() => blackjackGame.smokeParticle.Stop();
    
    #endregion
    
    #region Switch Hands
    
    private IEnumerator SwitchHands(float animDuration=0.5f)
    {
        List<CardInstance> tempHand = tableCards.CurrentHand;
        tableCards.CurrentHand = new List<CardInstance>(tableCards.DealerHand);
        tableCards.DealerHand = new List<CardInstance>(tempHand);
    
        int maxCards = Mathf.Max(tableCards.CurrentHand.Count, tableCards.DealerHand.Count);
    
        for(int i = 0; i < maxCards; i++)
        {
            MoveCard(i, true, animDuration);
            MoveCard(i, false, animDuration);
        }
    
        yield return new WaitForSeconds(animDuration);
        
        tableCards.UpdateAllHandsVisuals();
        blackjackGame.UpdateUI();
    }
    
    private void MoveCard(int index, bool isPlayer, float animDuration)
    {
        List<CardInstance> hand = null; Transform position = null; Vector3 offset = new();
        tableCards.ProcessCardPlacement(isPlayer, ref hand, ref position, ref offset);
        MoveCard(hand, index, position, offset, animDuration);
    }
    
    private void MoveCard(List<CardInstance> hand, int i, Transform position, Vector3 offset, float animDuration)
    {
        if (i >= hand.Count) return;

        CardInstance card = hand[i];
    
        card.displayComponent.transform.SetParent(position);
    
        int cardOrderIndex = hand.Count - 1 - i;
        Vector3 targetLocalPos = offset * cardOrderIndex;
    
        StartCoroutine(tableCards.CardAnimationCoroutine(
            card.displayComponent.transform,
            position.TransformPoint(targetLocalPos),
            position.rotation,
            TableCards.CardScaleVector,
            animDuration
        ));
    }
    
    #endregion
}
