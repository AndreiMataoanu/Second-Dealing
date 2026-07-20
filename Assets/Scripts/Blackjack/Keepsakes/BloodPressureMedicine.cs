using UnityEngine;

[CreateAssetMenu(fileName = "BloodPressureMedicine", menuName = "Keepsakes/BloodPressureMedicine")]
public class BloodPressureMedicine : Keepsake
{
    private BlackjackGame blackjackGame;
    
    public override bool AllowPostStandItem(BlackjackGame game)
    {
        blackjackGame = game;
        return game.ActivateBpMedicine();
    }

    public override void Deactivate() => blackjackGame.DeactivateBpMedicine();
}