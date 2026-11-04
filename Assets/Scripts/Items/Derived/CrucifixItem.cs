public class CrucifixItem : Item
{
    public static bool isCrucifixActive;
    
    private bool ActivateCrucifix()
    {
        if(!blackjackGame.isRoundActive || blackjackGame.CheckItemAfterStand()) return false;

        isCrucifixActive = true;
        AudioManagerFMOD.Play(AudioEvent.sfxCrossItem);

        return true;
    }

    public override bool Activate()
    {
        return ActivateCrucifix();
    }

    public static Card TryPrayForCard(Deck deck, int idealValue)
    {
        if (!isCrucifixActive) return deck.DealCard();
        AudioManagerFMOD.Play(AudioEvent.sfxCrossItemEffect);

        isCrucifixActive = false;
        return deck.DealBestCard(idealValue);
    }
}
