using Managers;
using UnityEngine;
using System.Collections;
[CreateAssetMenu(fileName = "Pyro", menuName = "Keepsakes/Pyro")]
public class Pyro : Keepsake
{
    public static bool isPyroActive;
    public float burnTime = 3f;
    public Color burnColor = Color.darkRed;
    public float burnBorder = 1.3f;
    private CardEffectActions cardEffect;
    private BlackjackGame game;
    private int usesThisRound = 0;
    [Header("Fire VFX")]
    [SerializeField] private ParticleSystem fireParticlePrefab;
    #region Setup

    private void OnEnable()
    {
        usesThisRound = 0;
    }

    public override void OnRoundStart()
    {
        usesThisRound = 0;
    }
    
    public override void SetMembers(BlackjackGame blackjackGame)
    {
        game = blackjackGame;
        cardEffect = new CardEffectActions(
            game,
            game.CursorFollow,
            game.CursorDetection,
            CursorType.None,
            CardTrigger.Pyro
        );
    }

    #endregion

    #region Activate Pyro

    public override bool ActivateTableEffect(BlackjackGame blackjackGame)
    {
        if(usesThisRound >= 1) return false;
        
        return ActivatePyro();
    }
    
    private bool ActivatePyro()
    {
        if(!game.isRoundActive || game.isActionLocked || isPyroActive) return false;

        isPyroActive = true;
        cardEffect.SelectCard();
        cardEffect.AddCardEffectAction(OnBurnCard);
        usesThisRound++;

        return true;
    }
    
    private void OnBurnCard(CardInstance cardInstance)
    {
        AudioManager.instance.Play("ItemBuy");
        CardEffects.SetDissolvedVisual(cardInstance.displayComponent, burnTime, burnColor,burnBorder);
        cardEffect.OnCardSelected();
        SpawnBurnParticles(cardInstance.displayComponent.transform);
        game.StartCoroutine(DissolveCard(cardInstance));
        isPyroActive = false;
    }
    
    // TODO: revise, same as acid code
    private IEnumerator DissolveCard(CardInstance cardInstance)
    {
        yield return new WaitForSeconds(burnTime);
        
        var cardObject = cardInstance.displayComponent.gameObject;
         var fx = cardObject.GetComponentInChildren<ParticleSystem>();
        CardEffects.RemoveCutCard(cardInstance);
        CardEffects.RemoveAlcoholCard(cardInstance);
        game.activeCardObjects.Remove(cardObject);
        game.GameDeck.AddRemovedCard(cardInstance.cardData.rank, cardInstance.cardData.suit); // TODO: move to card effects

        if (game.dealerHand.Remove(cardInstance))
        {
            KeepsakeUnlockProgression.instance.AddStat(ChallengeType.AlterDealerHand);
            game.UpdateHandVisuals(game.dealerHand, false);
        }
        
        game.playerHands.ForEach(hand =>
        {
            hand.Remove(cardInstance);
            game.UpdateHandVisuals(hand, true);
        });

        if (cardInstance == game.peekCardInstance)
            game.peekCardInstance = null;
        
        Destroy(cardObject);
        game.UpdateUI();
        game.EvaluateDoubleDownCondition();
        
        yield return null;
    }

    #endregion
    private ParticleSystem SpawnBurnParticles(Transform cardTransform)
    {
        if (fireParticlePrefab == null) return null;

        var fx = Instantiate(fireParticlePrefab, cardTransform.position, Quaternion.Inverse(cardTransform.rotation), cardTransform);
        fx.Play();
        return fx;
    }
    
}