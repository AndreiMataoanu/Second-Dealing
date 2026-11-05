using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveSuitChoiceEvent : CardChoiceEvent
{
    [Header("VFX")] 
    [SerializeField] private new ParticleSystem particleSystem;
    
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

        TableCards.GameDeck.AddRemovedSuit(cardInstance.cardData.suit);
        CardEffects.OnCardSelected();

        StartCoroutine(RemoveSuitCoroutine(cardInstance));
    }

    private IEnumerator RemoveSuitCoroutine(CardInstance cardInstance)
    {
        var particles = Instantiate(particleSystem, cardInstance.CardObject.transform.position, particleSystem.transform.rotation);
        particles.Play();
        Destroy(cardInstance.CardObject);
        dartSelection.ResetDarts();

        yield return new WaitUntil(() => !particles.isPlaying);
        
        Destroy(particles.gameObject);

        yield return base.SelectChoiceEndCoroutine();
    }
}
