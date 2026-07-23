using UnityEngine;

[CreateAssetMenu(fileName = "SecondDealing", menuName = "Keepsakes/Second Dealing")]
public class SecondDealing : Keepsake
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
            cardSelector.OpenCardSelector();

            return true;
        }

        return false;
    }

    public override void OnRoundStart()
    {
        cardSelector.ResetPrinting();
    }
}