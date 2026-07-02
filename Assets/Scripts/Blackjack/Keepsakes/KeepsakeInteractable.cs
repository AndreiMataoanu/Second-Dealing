using UnityEngine;

public class KeepsakeInteractable : Clickable
{
    [SerializeField] private Keepsake keepsake;
    [SerializeField] private BlackjackGame blackjackGame;

    public override void OnClick(int mouseButton = 0)
    {
        if(!IsActive) return;

        base.OnClick(mouseButton);

        if(keepsake != null && KeepsakeManager.instance != null)
        {
            KeepsakeManager.instance.equippedKeepsake = keepsake;
            AudioManager.instance.Play("ItemBuy");
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