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
        return organRoundsLeft switch
        {
            0 => base.GetTooltipContent(),
            1 => base.GetTooltipContent() + "\nExpires in 1 round",
            _ => base.GetTooltipContent() + $"\nExpires in {organRoundsLeft} rounds"
        };
    }

    public static void Expire(ShopManager shopManager)
    {
        AudioManager.instance.Play("OrganExpire");
        shopManager.RemoveFromInventory(ItemType.Organ);
        isOrganActive = false;
    }
}
