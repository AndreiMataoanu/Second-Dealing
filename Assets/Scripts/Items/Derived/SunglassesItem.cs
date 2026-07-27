public class SunglassesItem : Item
{
    private bool ActivateSunglasses()
    {
        if(!blackjackGame.isRoundActive || blackjackGame.CheckItemAfterStand()) return false;

        return blackjackGame.TableCards.RevealNextCard();
    }

    public override bool Activate()
    {
        return ActivateSunglasses();
    }
}
