using System.Collections;
using UnityEngine;

public class AddCardsEventAction : MonoBehaviour
{
    [SerializeField] private BlackjackGame blackjackGame;
    [SerializeField] private Vector3 cardsOffset = new(0.2f, 0.0f, 0.0f);

    private EventManager eventManager;
    private const int OptionCount = 3;
    private TableCards tableCards;

    private void Awake()
    {
        eventManager = blackjackGame.EventManager;
        tableCards = blackjackGame.TableCards;
    }

    public void DealOptions()
    {
        StartCoroutine(DealAllOptionsCoroutine());
    }

    private IEnumerator DealAllOptionsCoroutine()
    {
        blackjackGame.GameCamera.ChangeToCamera(CameraType.Playing);
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < OptionCount; i++)
        {
            StartCoroutine(DealCardOption(i));
            yield return new WaitForSeconds(1f);
        }

        eventManager.AddClickableCardOptions();
        blackjackGame.SelectCursorHand(true);
    }
    
    private IEnumerator DealCardOption(int optionIndex)
    {
        var cardsPosition = blackjackGame.CardOptionPosition;
        var card = tableCards.DealCard();
        var cardInstance = tableCards.DealCardInstance(card, false);
        yield return tableCards.PlaceCardAtIndex(optionIndex, cardInstance, cardsPosition, cardsOffset);
    }

    public void DestroyCards()
    {
        var cardsPosition = blackjackGame.CardOptionPosition;

        foreach(Transform card in cardsPosition.transform)
            Destroy(card.gameObject);
    }
}
