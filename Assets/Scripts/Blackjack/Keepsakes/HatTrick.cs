using UnityEngine;

[CreateAssetMenu(fileName = "HatTrick", menuName = "Keepsakes/Hat Trick")]
public class HatTrick : Keepsake
{
    private int usesThisRound = 0;

    private void OnEnable()
    {
        usesThisRound = 0;
    }

    public override void OnRoundStart()
    {
        usesThisRound = 0;
    }

    public override bool ActivateTableEffect(BlackjackGame game)
    {
        if(usesThisRound >= 1) return false;

        bool success = game.ActivateHatTrick();

        if(success)
        {
            usesThisRound++;

            return true;
        }

        return false;
    }
}