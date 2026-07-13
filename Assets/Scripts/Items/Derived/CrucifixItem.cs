public class CrucifixItem : Item
{
    public static bool isCrucifixActive;
    
    private bool ActivateCrucifix()
    {
        if(!blackjackGame.isRoundActive || blackjackGame.CheckItemAfterStand()) return false;

        isCrucifixActive = true;

        return true;
    }

    public override bool Activate()
    {
        return ActivateCrucifix();
    }
}
