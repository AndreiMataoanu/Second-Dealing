using UnityEngine;

[CreateAssetMenu(fileName = "Pyro", menuName = "Keepsakes/Pyro")]
public class Pyro : Keepsake
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

    public override bool TryActivateTableEffect(BlackjackGame game)
    {
        if(usesThisRound >= 1) return false;

        bool success = game.ActivatePyro();

        if(success)
        {
            usesThisRound++;

            return true;
        }

        return false;
    }
}