using UnityEngine;

[CreateAssetMenu(fileName = "SecondDealing", menuName = "Keepsakes/Second Dealing")]
public class SecondDealing : Keepsake
{
    private CardSelectorManager cardSelector;
    private BlackjackGame gameManager;
    private bool isCharged = false;

    private void OnEnable()
    {
        isActive = true;
        isCharged = true;
    }

    public override void SetMembers(BlackjackGame blackjackGame)
    {
        gameManager = blackjackGame;
        cardSelector = FindFirstObjectByType<CardSelectorManager>(FindObjectsInactive.Include);
    }

    public override bool ActivateTableEffect()
    {
        if(!gameManager.isRoundActive || gameManager.isActionLocked || !isCharged) return false;

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

    public void UseCharge()
    {
        isCharged = false;
    }

    public void Recharge()
    {
        isCharged = true;
    }
}