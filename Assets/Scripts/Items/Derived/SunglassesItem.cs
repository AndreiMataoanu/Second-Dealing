using UnityEngine;

public class SunglassesItem : Item
{
    //TODO redo after class table cards finished
    
    private bool ActivateSunglasses()
    {
        if(!blackjackGame.isRoundActive || blackjackGame.peekedCardObject || blackjackGame.CheckItemAfterStand()) return false;
    
        Card? nextCard = blackjackGame.GameDeck.PeekCard();
    
        if(!nextCard.HasValue) return false;
    
        Card newCardData = nextCard.Value;
    
        if(!blackjackGame.cardPrefabLookup.TryGetValue((newCardData.rank, newCardData.suit), out GameObject cardPrefabToUse)) return false;
    
        blackjackGame.peekedCardObject = Instantiate(cardPrefabToUse, blackjackGame.sunglassesCardPosition);
        blackjackGame.peekedCardObject.transform.localScale = TableCards.CardScaleVector;
    
        StartCoroutine(blackjackGame.CardAnimationCoroutine(
            blackjackGame.peekedCardObject.transform,
            blackjackGame.sunglassesCardPosition.position,
            blackjackGame.sunglassesCardPosition.rotation,
            TableCards.CardScaleVector,
            TableCards.CardAnimationDuration
        ));
        
        CardDisplay cardDisplay = blackjackGame.peekedCardObject.GetComponent<CardDisplay>();
        
        if(cardDisplay != null)
        {
            cardDisplay.SetHidden(false);
    
            bool isSuitNegative = blackjackGame.IsCardNegative(newCardData);
            bool isDoubled = blackjackGame.EventManager.CheckIfDoubled(newCardData) || Alcoholtem.isAlcoholActive;
            bool isHalved = blackjackGame.EventManager.CheckIfHalved(newCardData);
    
            cardDisplay.SetNegativeVisual(isSuitNegative);
            cardDisplay.SetDoubledVisual(isDoubled);
            cardDisplay.SetCutVisual(isHalved);
            
            blackjackGame.peekCardInstance = new CardInstance(newCardData, cardDisplay);
        }
    
        blackjackGame.activeCardObjects.Add(blackjackGame.peekedCardObject);
    
        return true;
    }

    public override bool Activate()
    {
        return ActivateSunglasses();
    }
}
