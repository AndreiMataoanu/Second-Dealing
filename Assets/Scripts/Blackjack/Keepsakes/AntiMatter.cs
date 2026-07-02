using UnityEngine;

[CreateAssetMenu(fileName = "AntiMatter", menuName = "Keepsakes/Anti Matter")]
public class AntiMatter : Keepsake
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

        bool success = game.ActivateAntiMatter();

        if(success)
        {
            usesThisRound++;
            return true;
        }

        return false;
    }
}