using UnityEngine;

[CreateAssetMenu(fileName = "Printer", menuName = "Keepsakes/Printer")]
public class Printer : Keepsake
{
    private CardSelectorManager activeMachine;
    private BlackjackGame gameManager;

    private void OnEnable()
    {
        isActive = true;
    }

    public override void SetMembers(BlackjackGame blackjackGame)
    {
        gameManager = blackjackGame;

        activeMachine = FindFirstObjectByType<CardSelectorManager>();
    }

    public override bool ActivateTableEffect(BlackjackGame game)
    {
        if(!gameManager.isRoundActive || gameManager.isActionLocked) return false;

        if(activeMachine != null)
        {
            activeMachine.OpenMachine();
            return true;
        }

        return false;
    }

    public override void OnRoundStart()
    {
        if(activeMachine != null)
        {
            activeMachine.OnRoundStart();
        }
    }
}