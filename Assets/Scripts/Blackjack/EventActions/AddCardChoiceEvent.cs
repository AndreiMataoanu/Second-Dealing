using System.Collections;
using UnityEngine;

public class AddCardChoiceEvent : CardChoiceEvent
{
    private AddCardsEvent addCardsEvent;

    public void SetAddCardsEvent(AddCardsEvent addCards) => addCardsEvent = addCards;
    
    private void Awake()
    {
        OptionCount = 3;
    }

    #region Select Option

    protected override void OnSelectCardOption(CardInstance cardInstance)
    {
        AudioManager.instance.Play("CardHit");
        TableCards.GameDeck.AddCardCopies(cardInstance.cardData, addCardsEvent.CopyCount);

        Destroy(cardInstance.CardObject);
        CardEffects.OnCardSelected();
        
        StartCoroutine(SelectCardCopyEndCoroutine());
    }
    
    private IEnumerator SelectCardCopyEndCoroutine()
    {
        yield return new WaitForSeconds(0.7f);
        blackjackGame.DialogueSystem.ShowCopyChoiceTaunt();
        
        yield return new WaitForSeconds(1.5f);
        
        DestroyCards();
        isChoosing = false;
    }

    #endregion

}
