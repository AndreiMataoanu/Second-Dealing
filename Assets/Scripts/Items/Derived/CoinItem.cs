public class CoinItem : Item
{
    private bool ActivateCoin()
    {
        if(!blackjackGame.isRoundActive || blackjackGame.CheckItemAfterStand()) return false;
        
        var isLucky = blackjackGame.ShopManager.FlipCoin();
        if (isLucky) blackjackGame.DialogueSystem.ShowLuckyCoinFlip();
        else blackjackGame.DialogueSystem.ShowUnluckyCoinFlip();
    
        return true;
    }

    public override bool Activate()
    {
        return ActivateCoin();
    }
}
