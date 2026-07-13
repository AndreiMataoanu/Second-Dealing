using UnityEngine;

public class FanItem : Item
{
    private bool ActivateFan()
    {
        if(!blackjackGame.isRoundActive || blackjackGame.CheckItemAfterStand()) return false;
        
        blackjackGame.StartCoroutine(blackjackGame.FanCoroutine());

        return true;
    }
    
    public override bool Activate()
    {
        return ActivateFan();
    }
}
