using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class NegativeSuitChoiceEvent : CardChoiceEvent
{
    private Queue<Card.Suit> suits = new(new[]{Card.Suit.Diamonds, Card.Suit.Spades, Card.Suit.Clubs, Card.Suit.Hearts});
    
    private void Awake()
    {
        OptionCount = suits.Count;
    }

    protected override IEnumerator DealCardOption()
    {
        var suit = suits.Dequeue();
        var cardInstance = TableCards.DealCardInstance(new Card {rank = Card.Rank.Ace, suit = suit}, false);
        cardInstance.displayComponent.SetTextureAce1();
        yield return TableCards.PlaceCardAtIndex(1, cardInstance, cardsPosition, CardsOffset);
    }
    
    protected override void OnSelectCardOption(CardInstance cardInstance)
    {
        dartSelection.ThrowDart(SelectIndex, cardInstance.CardObject.transform.position);
        
        AudioManagerFMOD.Play(AudioEvent.sfxDeal);
        
        StartCoroutine(MakeSuitNegative(cardInstance));
    }

    private IEnumerator MakeSuitNegative(CardInstance cardInstance)
    {
        var negativeCard = Instantiate(cardInstance.CardObject,
            cardInstance.CardObject.transform.position, cardInstance.CardObject.transform.rotation);
        
        var pos = negativeCard.transform.localPosition;
        pos.y -= 0.0005f;
        negativeCard.transform.localPosition = pos;

        var display = negativeCard.GetComponent<CardDisplay>();
        display.SetNegativeVisual(true);
        
        yield return cardInstance.displayComponent.SetDissolvedVisual(1.5f, Color.aliceBlue, 1.2f);
        
        global::CardEffects.AddNegativeSuit(cardInstance.cardData.suit);
        TableCards.UpdateCardVFX();
        
        CardEffects.OnCardSelected();

        yield return SelectChoiceEndCoroutine();
        
        Destroy(negativeCard);
    }
}
