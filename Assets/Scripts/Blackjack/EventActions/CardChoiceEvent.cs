using System.Collections;
using Managers;
using UnityEngine;

public abstract class CardChoiceEvent : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] protected BlackjackGame blackjackGame;
    
    [Header("Cards")]
    [SerializeField] protected Transform cardsPosition;
    [SerializeField] protected float radius = 0.2f;
    [SerializeField] protected TMPro.TextMeshProUGUI cardText;

    [Header("Darts")]
    [SerializeField] protected DartSelection dartSelection;
    [SerializeField] [Range(1, 3)] protected int dartNumber;

    [HideInInspector] public bool isChoosing = false;
    protected int OptionCount;
    protected int SelectIndex;

    protected TableCards TableCards;
    protected CardEffectActions CardEffects;
    
    private Vector3 cardsOffset;
    
    private void Start()
    {
        TableCards = blackjackGame.TableCards;
        CardEffects = new CardEffectActions(
            blackjackGame,
            CursorType.Dart,
            CardTrigger.AddCardsEvent
        );
    }
    
    #region Deal Options

    public void DealOptions()
    {
        StartCoroutine(DealAllOptionsCoroutine());
    }

    protected virtual IEnumerator DealAllOptionsCoroutine()
    {
        isChoosing = true;
        yield return new WaitForSeconds(1f);

        Quaternion slice = Quaternion.Euler(0.0f, 0.0f, 360.0f / OptionCount);
        cardsOffset = Vector3.up * radius;
        
        for (int i = 0; i < OptionCount; i++)
        {
            StartCoroutine(DealCardOption());
            cardsOffset = slice * cardsOffset;
            
            yield return new WaitForSeconds(0.5f);
        }

        UseDart();
        CardEffects.AddEventCardEffectAction(OnSelectCardOption, cardsPosition);
    }
    
    private IEnumerator DealCardOption()
    {
        var card = TableCards.DealCard();
        var cardInstance = TableCards.DealCardInstance(card, false);
        yield return TableCards.PlaceCardAtIndex(1, cardInstance, cardsPosition, cardsOffset);
    }

    #endregion

    #region Select Option

    // has to set isChoosing to false at the end (used in event manager)
    protected abstract void OnSelectCardOption(CardInstance cardInstance);

    protected virtual IEnumerator SelectCardCopyEndCoroutine()
    {
        yield return new WaitForSeconds(1.5f);
        
        DestroyCards();
        dartSelection.ResetDarts();
        isChoosing = false;
    }

    #endregion
    
    #region Helpers

    public void DestroyCards()
    {
        foreach(Transform card in cardsPosition.transform)
            Destroy(card.gameObject);
    }

    #endregion

    #region Using Darts

    private void UseDart()
    {
        dartSelection.DeactivateDartAtIndex(SelectIndex);
        
        CardEffects.SelectCard(dartSelection.GetDartPositionAtIndex(SelectIndex));
    }
    
    protected IEnumerator ChangeDart()
    {
        SelectIndex++;

        yield return new WaitForSeconds(0.5f);

        if (SelectIndex < dartNumber)
        {
            UseDart();
            yield break;
        }

        yield return SelectCardCopyEndCoroutine();
    }
    
    public void SetDartsActive(bool active)
    {
        dartSelection.SetDartSelectionActive(active);

        var activeDarts = active ? dartNumber : 0;
        dartSelection.SetActiveDartCount(activeDarts);
    }

    #endregion
}
