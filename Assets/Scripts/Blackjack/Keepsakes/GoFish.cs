using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GoFish", menuName = "Keepsakes/Go Fish")]
public class GoFish : Keepsake
{
    private CardSelectorManager cardSelector;
    private BlackjackGame gameManager;
    private TableCards tableCards;

    private void OnEnable()
    {
        isActive = true;
    }

    public override void SetMembers(BlackjackGame blackjackGame)
    {
        gameManager = blackjackGame;
        cardSelector = FindFirstObjectByType<CardSelectorManager>(FindObjectsInactive.Include);
        tableCards = gameManager.TableCards;
    }

    public override bool ActivateTableEffect()
    {
        if(!gameManager.isRoundActive || gameManager.isActionLocked) return false;

        if(cardSelector)
        {
            cardSelector.OpenCardSelector(this);

            return true;
        }

        return false;
    }

    public override void OnRoundStart()
    {
        cardSelector.ResetPrinting();
    }

    public IEnumerator GoFishCoroutine(Card.Rank targetRank, Action<bool> onComplete)
    {
        var stolenCards = TryStealFromDealer(targetRank, onComplete);

        gameManager.isActionLocked = true;

        yield return AddStolenCardsToPlayer(stolenCards);
        
        var hiddenCard = tableCards.DealerHand.Find(c => c.isHidden);
        bool isHidden = hiddenCard != null;
        gameManager.UpdateUI(isHidden);
        gameManager.CalculateBust();
    }

    private List<CardInstance> TryStealFromDealer(Card.Rank targetRank, Action<bool> onComplete)
    {
        List<CardInstance> stolenCards = new List<CardInstance>();

        for(int i = tableCards.DealerHand.Count - 1; i >= 0; i--)
        {
            if(tableCards.DealerHand[i].cardData.rank == targetRank)
            {
                stolenCards.Add(tableCards.DealerHand[i]);
                tableCards.DealerHand.RemoveAt(i);
            }
        }

        onComplete?.Invoke(stolenCards.Count != 0);

        return stolenCards;
    }
    
    private IEnumerator AddStolenCardsToPlayer(List<CardInstance> stolenCards)
    {
        foreach(var card in stolenCards)
        {
            if(card.isHidden)
            {
                yield return tableCards.FlipCard(card.displayComponent, 0.4f);
                card.isHidden = false;
            }

            tableCards.CurrentHand.Insert(0, card);
            yield return tableCards.PlaceCardAtPlayerHandIndex(0, card);
        }

        tableCards.UpdateAllHandsVisuals();
    }
}