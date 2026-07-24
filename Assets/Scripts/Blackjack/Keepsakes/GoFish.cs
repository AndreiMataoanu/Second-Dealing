using UnityEngine;

[CreateAssetMenu(fileName = "GoFish", menuName = "Keepsakes/Go Fish")]
public class GoFish : Keepsake
{
    private CardSelectorManager cardSelector;
    private BlackjackGame gameManager;

    private void OnEnable()
    {
        isActive = true;
    }

    public override void SetMembers(BlackjackGame blackjackGame)
    {
        gameManager = blackjackGame;
        cardSelector = FindFirstObjectByType<CardSelectorManager>(FindObjectsInactive.Include);
    }

    public override bool ActivateTableEffect(BlackjackGame game)
    {
        if(!gameManager.isRoundActive || gameManager.isActionLocked) return false;

        if(cardSelector != null)
        {
            cardSelector.OpenCardSelector(this);

            return true;
        }

        return false;
    }

    public override void OnRoundStart()
    {
        cardSelector.ResetPrinting();
    }
}