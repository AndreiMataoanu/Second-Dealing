using System.Collections;
using Managers;
using UnityEngine;

public abstract class CardChoiceEvent : MonoBehaviour
{
    [SerializeField] protected BlackjackGame blackjackGame;
    [SerializeField] protected Transform cardsPosition;
    [SerializeField] protected Vector3 cardsOffset = new(0.2f, 0.0f, 0.0f);

    [HideInInspector] public bool isChoosing = false;
    protected int OptionCount;
    
    protected TableCards TableCards;
    protected CardEffectActions CardEffects;
    
    private void Start()
    {
        TableCards = blackjackGame.TableCards;
        CardEffects = new CardEffectActions(
            blackjackGame,
            blackjackGame.CursorFollow,
            blackjackGame.CursorDetection,
            CursorType.Flip,
            CardTrigger.AddCardsEvent
        );
    }
    
    #region Deal Options

    public void DealOptions()
    {
        StartCoroutine(DealAllOptionsCoroutine());
    }

    private IEnumerator DealAllOptionsCoroutine()
    {
        isChoosing = true;
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < OptionCount; i++)
        {
            StartCoroutine(DealCardOption(i));
            yield return new WaitForSeconds(1f);
        }

        CardEffects.SelectCard();
        CardEffects.AddEventCardEffectAction(OnSelectCardOption, cardsPosition);
    }
    
    private IEnumerator DealCardOption(int optionIndex)
    {
        var card = TableCards.DealCard();
        var cardInstance = TableCards.DealCardInstance(card, false);
        yield return TableCards.PlaceCardAtIndex(optionIndex, cardInstance, cardsPosition, cardsOffset);
    }

    #endregion

    #region Select Option

    // has to set isChoosing to false at the end (used in event manager)
    protected abstract void OnSelectCardOption(CardInstance cardInstance);

    #endregion
    
    #region Helpers

    public void DestroyCards()
    {
        Debug.Log("destroy cards");
        foreach(Transform card in cardsPosition.transform)
            Destroy(card.gameObject);
    }

    #endregion

}
