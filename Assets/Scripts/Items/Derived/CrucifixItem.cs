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

    public static Card TryPrayForCard(Deck deck, int idealValue)
    {
        if (!isCrucifixActive) return deck.DealCard();

        isCrucifixActive = false;
        return deck.DealBestCard(idealValue);
    }
}
