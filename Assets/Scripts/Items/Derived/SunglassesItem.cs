public class SunglassesItem : Item
{
    private bool ActivateSunglasses()
    {
        if(!blackjackGame.isRoundActive || blackjackGame.CheckItemAfterStand()) return false;
        AudioManagerFMOD.Play(AudioEvent.sfxSunglassesItem);

        return blackjackGame.TableCards.RevealNextCard();
    }

    public override bool Activate()
    {
        return ActivateSunglasses();
    }
}
