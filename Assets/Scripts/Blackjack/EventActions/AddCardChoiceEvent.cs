using System.Collections;
using UnityEngine;

public class AddCardChoiceEvent : CardChoiceEvent
{
    private AddCardsEvent addCardsEvent;

    private int currentCopyCount;

    public void SetAddCardsEvent(AddCardsEvent addCards) => addCardsEvent = addCards;
    
    private void Awake()
    {
        OptionCount = 6;
    }

    #region Select Option

    protected override IEnumerator DealAllOptionsCoroutine()
    {
        UpdateCopyCount();

        yield return base.DealAllOptionsCoroutine();
    }

    protected override void OnSelectCardOption(CardInstance cardInstance)
    {
        dartSelection.ThrowDart(SelectIndex, cardInstance.CardObject.transform.position);
        
        AudioManager.instance.Play("CardHit");
        TableCards.GameDeck.AddCardCopies(cardInstance.cardData, currentCopyCount);
        UpdateCopyCount();

        CardEffects.OnCardSelected();
        StartCoroutine(ChangeDart());
    }
    
    protected override IEnumerator SelectCardCopyEndCoroutine()
    {
        yield return new WaitForSeconds(0.7f);
        blackjackGame.DialogueSystem.ShowCopyChoiceTaunt();
        
        yield return base.SelectCardCopyEndCoroutine();
    }

    #endregion

    #region Helpers

    private void UpdateCopyCount()
    {
        if (SelectIndex == dartNumber) return;
        
        currentCopyCount = addCardsEvent.GenerateCopyCount();
        cardText.text = currentCopyCount + " ";
        cardText.text += currentCopyCount > 0 ? "copies" : "copy";
    }
    
    #endregion
}
