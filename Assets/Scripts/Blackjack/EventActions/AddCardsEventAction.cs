using System.Collections;
using UnityEngine;

public class AddCardsEventAction : MonoBehaviour
{
    [SerializeField] private BlackjackGame blackjackGame;
    [SerializeField] private Transform cardsPosition;
    [SerializeField] private float horizontalSpacing = 0.2f;

    private const int OptionCount = 3;
    private const float CardAnimationDuration = 0.25f;
    private readonly Vector3 cardScaleVector = Vector3.one * 0.05f;

    public void DealOptions()
    {
        StartCoroutine(DealAllOptionsCoroutine());
    }

    private IEnumerator DealAllOptionsCoroutine()
    {
        blackjackGame.ChangeToCamera(CameraType.Playing);
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < OptionCount; i++)
        {
            StartCoroutine(DealCardOption(i));
            yield return new WaitForSeconds(1f);
        }
        
        // TODO make cards interactive
    }
    
    private IEnumerator DealCardOption(int optionIndex)
    {
        var card = blackjackGame.DealCard();
        var cardInstance = blackjackGame.DealCardInstanceOption(card, false);
        AudioManager.instance.Play("CardHit");

        if (cardInstance != null)
        {
            int cardOrderIndex = optionIndex;
            float xOffset = cardOrderIndex * horizontalSpacing;

            Vector3 targetLocalPos = new Vector3(xOffset, 0, 0);
            Quaternion targetRotation = Quaternion.identity;

            cardInstance.displayComponent.transform.SetParent(cardsPosition.parent);

            yield return StartCoroutine(blackjackGame.CardAnimationCoroutine(
                cardInstance.displayComponent.transform,
                cardsPosition.TransformPoint(targetLocalPos),
                cardsPosition.rotation * targetRotation,
                cardScaleVector,
                CardAnimationDuration
            ));

            cardInstance.displayComponent.transform.SetParent(cardsPosition);
            cardInstance.displayComponent.transform.localPosition = targetLocalPos;
            cardInstance.displayComponent.transform.localRotation = targetRotation;
            cardInstance.displayComponent.transform.localScale = cardScaleVector;
        }
    }

}
