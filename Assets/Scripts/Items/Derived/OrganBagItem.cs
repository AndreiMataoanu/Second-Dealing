public class OrganBagItem : Item
{
    public static bool isOrganActive;
    public static int organRoundsLeft;
    public static bool isInShop = false;

    public override bool Activate()
    {
        return false;
    }

    public override void OnRoundEnd()
    {
        if(isOrganActive && organRoundsLeft > 0)
        {
            organRoundsLeft--;

            if(organRoundsLeft == 0)
            {
                isOrganActive = false;

                AudioManager.instance.Play("OrganExpire");
                
                blackjackGame.ItemManager.AddItemToRemove(this);
            }
        }
    }

    public override void ActivatePassive()
    {
        isOrganActive = true;
        organRoundsLeft = 2;
    }

    public override void DeactivatePassive()
    {
        isOrganActive = false;
        organRoundsLeft = 0;
    }

    protected override string GetTooltipContent()
    {
        if (organRoundsLeft == 0)
            return base.GetTooltipContent();
        if (organRoundsLeft == 1)
            return base.GetTooltipContent() + "\nExpires in 1 round";

        return base.GetTooltipContent() + $"\nExpires in {organRoundsLeft} rounds";
    }
}
