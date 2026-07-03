using UnityEngine;

public class KeepsakeInteractable : Clickable
{
    [SerializeField] private Keepsake keepsake;
    [SerializeField] private BlackjackGame blackjackGame;

    public override void OnClick(int mouseButton = 0)
    {
        if(!IsActive) return;

        base.OnClick(mouseButton);

        if(KeepsakeManager.instance.equippedKeepsakes.Contains(keepsake))
        {
            KeepsakeManager.instance.UnequipKeepsake(keepsake);
            AudioManager.instance.Play("ItemBuy");
        }
        else
        {
            bool equipped = KeepsakeManager.instance.EquipKeepsake(keepsake);

            if(equipped)
            {
                AudioManager.instance.Play("ItemBuy");
            }
            else
            {
                AudioManager.instance.Play("ItemDeny");
            }
        }
    }

    protected override string GetTooltipHeader()
    {
        return keepsake.keepsakeName;
    }

    protected override string GetTooltipContent()
    {
        return keepsake.description;
    }
}