using System.Collections;
using UnityEngine;

public class AlcoholItem : Item
{
    [SerializeField] private float drinkDuration = 1f;
    
    public static bool isAlcoholActive;
    private static readonly int Drink = Animator.StringToHash("Drink");
    private readonly Quaternion tiltDegree = Quaternion.Euler(-30f, 0f, 0f);

    public override bool Activate()
    {
        return ActivateAlcohol();
    }
    
    private bool ActivateAlcohol()
    {
        if(!blackjackGame.isRoundActive || blackjackGame.CheckItemAfterStand() || isAlcoholActive) return false;
    
        isAlcoholActive = true;
        SetVisibility(false);

        AudioManager.instance.Play("Drink");
        StartCoroutine(AlcoholCoroutine());
        
        return true;
    }

    #region Override Methods

    public override void SetMembers()
    {
        delayDestroy = true;
    }
    
    public override void OnRoundStart()
    {
        if (!isAlcoholActive) return;
        blackjackGame.GameCamera.UseClearVision();
    }

    public override void OnRoundEnd()
    {
        if (!isAlcoholActive) return;
        isAlcoholActive = false;
        blackjackGame.GameCamera.UseClearVision();
        blackjackGame.ItemManager.AddItemToRemove(this);
    }

    #endregion
    
    #region Drink from bottle
    
    private IEnumerator AlcoholCoroutine()
    {
        blackjackGame.isActionLocked = true;
        
        yield return DrinkFromBottle();
        yield return new WaitForSeconds(1f);

        blackjackGame.bottleAnimation.gameObject.SetActive(false);
        blackjackGame.GameCamera.UseDistortedVision();
        blackjackGame.UpdateAlcoholCards();
        blackjackGame.CalculateBust();
    }

    private Coroutine DrinkFromBottle()
    {
        UseBottleAnimation(Drink);
        return blackjackGame.GameCamera.TiltPlayerCameraUpDown(tiltDegree, drinkDuration);
    }

    private void UseBottleAnimation(int animationId)
    {
        blackjackGame.bottleAnimation.gameObject.SetActive(true);
        blackjackGame.bottleAnimation.SetTrigger(animationId);
    }
    
    #endregion
}
