public class KnifeItem : Item
{
    public static bool isKnifeActive;
    
    private bool ActivateKnife()
    {
        if(!blackjackGame.isRoundActive || isKnifeActive || blackjackGame.CheckItemAfterStand()) return false;
    
        isKnifeActive = true;
    
        return true;
    }

    public override bool Activate()
    {
        return ActivateKnife();
    }
}
