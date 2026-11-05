public class CoinItem : Item
{
    private bool ActivateCoin()
    {
        if (!blackjackGame.isRoundActive || blackjackGame.CheckItemAfterStand()) return false;

        AudioManager.instance.Play("Coin(Clone)"); //DELETE LATER
        AudioManagerFMOD.Play(AudioEvent.sfxCoinItem);

        var isLucky = blackjackGame.ShopManager.FlipCoin();

        if (isLucky)
        {
            AudioManagerFMOD.SetGlobalParameter("parCoinFlipResult", 0);
            blackjackGame.DialogueSystem.ShowLuckyCoinFlip();
        }
        else 
        {
            AudioManagerFMOD.SetGlobalParameter("parCoinFlipResult", 0);
            blackjackGame.DialogueSystem.ShowUnluckyCoinFlip();
            return true;
        }

        return false;
    }
    

    public override bool Activate()
    {
        return ActivateCoin();
    }
}
