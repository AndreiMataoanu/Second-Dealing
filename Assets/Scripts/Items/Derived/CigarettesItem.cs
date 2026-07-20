using UnityEngine;

public class CigarettesItem : Item
{
    public static bool isCigaretteActive;
    
    private bool ActivateCigarette()
    {
        Debug.Log("cig" +!blackjackGame.isRoundActive + " " + blackjackGame.CheckItemAfterStand() + " " + isCigaretteActive + " " + blackjackGame.isSplitting);

        if(!blackjackGame.isRoundActive || blackjackGame.CheckItemAfterStand() 
                                        || isCigaretteActive 
                                        || blackjackGame.isSplitting) return false;

        isCigaretteActive = true;

        blackjackGame.StartCoroutine(blackjackGame.CigaretteCoroutine());

        return true;
    }

    public override bool Activate()
    {
        return ActivateCigarette();
    }
}
