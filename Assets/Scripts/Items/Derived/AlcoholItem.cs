public class AlcoholItem : Item
{
    // [SerializeField] private GameObject distortion;

    public static bool isAlcoholActive;
    
    private bool ActivateAlcohol()
    {
        if(!blackjackGame.isRoundActive || blackjackGame.CheckItemAfterStand() || isAlcoholActive) return false;
    
        isAlcoholActive = true;
    
        blackjackGame.StartCoroutine(blackjackGame.AlcoholCoroutine());
    
        return true;
    }

    public override bool Activate()
    {
        return ActivateAlcohol();
    }
    
}
