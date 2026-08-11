using UnityEngine;

public class NftItem : Item
{
    [HideInInspector] public int nftRoundsLeft;
    private bool isNftActive;
    
    private bool ActivateNft()
    {
        if(blackjackGame.CheckItemAfterStand()) return false;
        
        if(basePrice == 0)
        {
            AudioManager.instance.Play("ItemBuy");
            return true;
        }
        
        // TODO: make a separate method
        blackjackGame.AnimateBetGain(basePrice);
    
        return true;
    }
    
    public override bool Activate()
    {
        return ActivateNft();
    }

    public override void OnRoundStart()
    {
        if(nftRoundsLeft == 0)
            basePrice = 0;
        else
        {
            nftRoundsLeft--;
            basePrice = Random.Range(0, blackjackGame.GetPlayerMoney() * 2);
        }
    }

    public override void SetMembers()
    {
        nftRoundsLeft = Random.Range(2, 4);
        isNftActive = true;
    }

    protected override string GetTooltipContent()
    {
        return isNftActive ? base.GetTooltipContent() + "\nCurrent value: " + basePrice : base.GetTooltipContent();
    }
}
