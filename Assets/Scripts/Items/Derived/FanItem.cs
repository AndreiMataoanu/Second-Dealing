using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanItem : Item
{
    private TableCards tableCards;

    #region Override

    public override void SetMembers()
    {
        delayDestroy = true;
        tableCards = blackjackGame.TableCards;
    }

    public override bool Activate()
    {
        return ActivateFan();
    }
    
    #endregion

    #region Activate Fan

    private bool ActivateFan()
    {
        if(!blackjackGame.isRoundActive || blackjackGame.CheckItemAfterStand()) return false;

        AudioManager.instance.Play("Fan(Clone)");

        blackjackGame.StartCoroutine(FanCoroutine());

        return true;
    }
    
    private IEnumerator FanCoroutine()
    {
        blackjackGame.isActionLocked = true;
        blackjackGame.isRoundActive = false;

        yield return blackjackGame.StopDealerTurn();
        
        yield return StartCoroutine(AnimateCardsOffScreen());
    
        tableCards.ClearTable();
        tableCards.ResetCards();
        blackjackGame.ResetToSingleBet();
    
        blackjackGame.OnStartGame();
    }

    #endregion

    #region Animate Cards

    private IEnumerator AnimateCardsOffScreen()
    {
        float animDuration = 2f;
    
        List<Coroutine> moveCoroutines = new List<Coroutine>();
    
        foreach(GameObject card in tableCards.ActiveCardObjects)
        {
            Vector3 randomWindDirection = new Vector3(Random.Range(-25f, -15f), Random.Range(5f, 15f), Random.Range(-10f, 10f));
            Vector3 offScreenPos = card.transform.position + randomWindDirection;
            Vector3 randomSpin = new Vector3(Random.Range(-500f, 500f), Random.Range(-500f, 500f), Random.Range(-500f, 500f));
    
            moveCoroutines.Add(StartCoroutine(BlowCardAwayCoroutine(card.transform, offScreenPos, randomSpin, animDuration)));
        }
    
        foreach(Coroutine c in moveCoroutines)
        {
            yield return c;
        }
    }
    
    //Helps with spinning cards away when the fan is used.
    private IEnumerator BlowCardAwayCoroutine(Transform cardTransform, Vector3 targetPosition, Vector3 spinSpeed, float duration)
    {
        Vector3 startPosition = cardTransform.position;
    
        float time = 0;
    
        while(time < duration)
        {
            time += Time.deltaTime;
    
            float t = time / duration;
            float moveT = t * t * (3f - 2f * t);
    
            cardTransform.position = Vector3.Lerp(startPosition, targetPosition, moveT);
            cardTransform.Rotate(spinSpeed * Time.deltaTime, Space.World);
    
            yield return null;
        }
    }

    #endregion
}
