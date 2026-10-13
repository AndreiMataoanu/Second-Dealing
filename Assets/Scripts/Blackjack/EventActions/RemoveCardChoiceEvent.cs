using System;
using System.Collections;
using System.Collections.Generic;

public class RemoveCardChoiceEvent : CardChoiceEvent
{
    private List<ClickableCard> clickableCards = new();
    private List<CardInstance> instanceCards = new();
    
    private void Awake()
    {
        OptionCount = 6;
    }

    protected override IEnumerator DealAllOptionsCoroutine()
    {
        yield return base.DealAllOptionsCoroutine();
        
        AddClickableCardSelection();
        AddHoverAction();
    }

    protected override void OnSelectCardOption(CardInstance cardInstance)
    {
        dartSelection.ThrowDart(SelectIndex, cardInstance.CardObject.transform.position);
        
        AudioManager.instance.Play("CardHit");
        RemoveCardGroup(cardInstance);
        
        CardEffects.OnCardSelected();

        StartCoroutine(SelectChoiceEndCoroutine());
    }

    private void AddClickableCardSelection()
    {
        for (var i = 0; i < cardsPosition.childCount; i++)
        {
            var clickCard = cardsPosition.GetChild(i).GetComponentInChildren<ClickableCard>();
            clickableCards.Add(clickCard);
            instanceCards.Add(cardsPosition.GetChild(i).GetComponent<CardDisplay>().GetCardInstance());
        }
    }

    private void AddHoverAction()
    {
        if (clickableCards.Count < 3) throw new Exception("Remove card choice has less than 3 options.");
        
        for (var i = 1; i < clickableCards.Count - 1; i++)
            AddHoverAction(clickableCards[i], clickableCards[i-1], clickableCards[i+1]);
        
        AddHoverAction(clickableCards[0], clickableCards[^1], clickableCards[1]);
        AddHoverAction(clickableCards[^1], clickableCards[^2], clickableCards[0]);
    }

    private void AddHoverAction(ClickableCard hovered, ClickableCard left, ClickableCard right)
    {
        hovered.AddMouseEnterAction(left.ApplyOutline);
        hovered.AddMouseEnterAction(right.ApplyOutline);
            
        hovered.AddMouseExitAction(left.RemoveOutline);
        hovered.AddMouseExitAction(right.RemoveOutline);
    }

    private void RemoveCardGroup(CardInstance selected)
    {
        var index = 0;
        
        for (var i = 0; i < instanceCards.Count; i++)
        {
            if (instanceCards[i] == selected) index = i;
            
            clickableCards[i].RemoveMouseEnterAction();
            clickableCards[i].RemoveMouseExitAction();
        }
        
        if (index == 0)
            RemoveCardGroup(instanceCards[0], instanceCards[^1], instanceCards[1]);
        else if (index == instanceCards.Count - 1)
            RemoveCardGroup(instanceCards[index], instanceCards[^2], instanceCards[0]);
        else
            RemoveCardGroup(instanceCards[index], instanceCards[index-1], instanceCards[index+1]);
    }

    private void RemoveCardGroup(CardInstance middle, CardInstance left, CardInstance right)
    {
        TableCards.GameDeck.AddRemovedCard(middle.cardData.rank, middle.cardData.suit);
        TableCards.GameDeck.AddRemovedCard(left.cardData.rank, left.cardData.suit);
        TableCards.GameDeck.AddRemovedCard(right.cardData.rank, right.cardData.suit);
    }
}
