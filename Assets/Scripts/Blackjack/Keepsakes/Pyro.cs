using Managers;
using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Pyro", menuName = "Keepsakes/Pyro")]
public class Pyro : Keepsake
{
    [Header("Fire VFX")]
    [SerializeField] private ParticleSystem fireParticlePrefab;
    public float burnTime = 3f;
    public Color burnColor = Color.darkRed;
    public float burnBorder = 1.3f;
    
    public static bool isPyroActive;
    private bool isCardSelecting;
    
    private CardEffectActions cardEffect;
    private BlackjackGame game;
    private int usesThisRound = 0;
    private TableCards tableCards;

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
        tableCards = blackjackGame.TableCards;
        cardEffect = new CardEffectActions(
            game,
            CursorType.None,
            CardTrigger.Pyro
        );
    }

    #endregion

    #region Activate Pyro

    public override bool ActivateTableEffect()
    {
        if(usesThisRound >= 1) return false;
        
        return ActivatePyro();
    }
    
    private bool ActivatePyro()
    {
        if(!game.isRoundActive || game.isActionLocked || isPyroActive) return false;

        isPyroActive = true;
        isCardSelecting = true;
        
        cardEffect.SelectCard();
        cardEffect.AddItemCardEffectAction(OnBurnCard);
        usesThisRound++;

        return true;
    }
    
    private void OnBurnCard(CardInstance cardInstance)
    {
        AudioManager.instance.Play("Pyro");
        
        cardEffect.OnCardSelected();
        cardInstance.displayComponent.SetFaceColliderActive(false);
        game.StartCoroutine(BurnCard(cardInstance));
        
        isPyroActive = false;
        isCardSelecting = false;
    }
    
    private IEnumerator BurnCard(CardInstance cardInstance)
    {
        CardEffects.SetDissolvedVisual(cardInstance.displayComponent, burnTime, burnColor, burnBorder);
        SpawnBurnParticles(cardInstance.displayComponent.transform);
        
        yield return new WaitForSeconds(burnTime);
        
        tableCards.DestroyCard(cardInstance);
        game.EvaluateDoubleDownCondition();
        
        yield return null;
    }

    #endregion
    
    #region VFX
    
    private void SpawnBurnParticles(Transform cardTransform)
    {
        if (!fireParticlePrefab) return;

        var fx = Instantiate(fireParticlePrefab, cardTransform.position,
            Quaternion.Inverse(cardTransform.rotation), cardTransform);
        fx.Play();
    }
    
    #endregion

    #region Cancel Pyro
    
    public override bool OnCancel()
    {
        if (cardEffect == null || !isCardSelecting) return false;

        usesThisRound--;
        cardEffect.OnCancelSelect();
        isCardSelecting = false;
        isPyroActive = false;

        return true;
    }

    #endregion
    
}