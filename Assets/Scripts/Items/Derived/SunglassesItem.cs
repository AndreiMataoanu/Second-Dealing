using UnityEngine;

public class SunglassesItem : Item
{
    private TableCards tableCards;
    //TODO redo after class table cards finished
    
    private bool ActivateSunglasses()
    {
        if(!blackjackGame.isRoundActive || tableCards.PeekCardInstance != null || blackjackGame.CheckItemAfterStand()) return false;
    
        // Card nextCard = blackjackGame.tableCard.GameDeck.PeekCard();
        // if(nextCard == null) return false;
        //
        // Card newCardData = null;
        //
        // if(!blackjackGame.cardPrefabLookup.TryGetValue((newCardData.rank, newCardData.suit), out GameObject cardPrefabToUse)) return false;
        //
        // var peekInstance = tableCards.PeekCardInstance;
        // blackjackGame.peekedCardObject = Instantiate(cardPrefabToUse, blackjackGame.sunglassesCardPosition);
        // blackjackGame.peekedCardObject.transform.localScale = TableCards.CardScaleVector;
        //
        // StartCoroutine(blackjackGame.CardAnimationCoroutine(
        //     blackjackGame.peekedCardObject.transform,
        //     blackjackGame.sunglassesCardPosition.position,
        //     blackjackGame.sunglassesCardPosition.rotation,
        //     TableCards.CardScaleVector,
        //     TableCards.CardAnimationDuration
        // ));
        //
        // CardDisplay cardDisplay = peekInstance.displayComponent;
        //
        // if(cardDisplay)
        // {
        //     cardDisplay.SetHidden(false);
        //
        //     bool isSuitNegative = CardEffects.IsCardNegative(newCardData);
        //     bool isDoubled = CardEffects.IsCardDoubled(newCardData) || AlcoholItem.isAlcoholActive;
        //     bool isHalved = CardEffects.IsCardHalved(newCardData);
        //
        //     cardDisplay.SetNegativeVisual(isSuitNegative);
        //     cardDisplay.SetDoubledVisual(isDoubled);
        //     cardDisplay.SetCutVisual(isHalved);
        //     
        //     blackjackGame.peekCardInstance = new CardInstance(newCardData, cardDisplay);
        // }
        //
        // blackjackGame.activeCardObjects.Add(blackjackGame.peekedCardObject);
    
        return true;
    }

    public override bool Activate()
    {
        return ActivateSunglasses();
    }
}
