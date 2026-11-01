using System.Collections;
using UnityEngine;

public class AddCardChoiceEvent : CardChoiceEvent
{
    [Header("Card copies")]
    [SerializeField] protected TMPro.TextMeshProUGUI cardText;
    
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
        
        AudioManager.instance.Play("CardHit"); //DELETE LATER
        AudioManagerFMOD.Play(AudioEvent.sfxDartEventHit);
        TableCards.GameDeck.AddCardCopies(cardInstance.cardData, currentCopyCount);
        PlayMultipleAddSound(currentCopyCount, (float)0.2);
        UpdateCopyCount();

        CardEffects.OnCardSelected();
        StartCoroutine(ChangeDart());
    }
    
    protected override IEnumerator SelectChoiceEndCoroutine()
    {
        yield return new WaitForSeconds(0.7f);
        blackjackGame.DialogueSystem.ShowCopyChoiceTaunt();
        
        yield return base.SelectChoiceEndCoroutine();
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
    
    public void PlayMultipleAddSound(int count, float delay)
    {
        StartCoroutine(PlayRoutineCardAddSound(count, delay));
    }

    private IEnumerator PlayRoutineCardAddSound(int count, float delay)
    {
        yield return new WaitForSeconds((float)0.3); 
        for (int i = 0; i < count; i++)
        {
            AudioManagerFMOD.Play(AudioEvent.sfxCardAddToDeck);
            yield return new WaitForSeconds(delay);
        }
    }
    
    #endregion
}
