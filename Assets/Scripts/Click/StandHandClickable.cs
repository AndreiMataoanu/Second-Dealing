using UnityEngine;

public class StandHandClickable : Clickable
{
    [SerializeField] private BlackjackGame blackjackManager;
    [SerializeField] [TextArea] private string standardStandTooltip;
    [SerializeField] [TextArea] private string splitTooltip;
    private string activeTooltipString = "";

    protected override void OnMouseEnter()
    {
        if(!IsActive) return;

        UpdateTooltip();
        ApplyOutline();
    }

    private void OnMouseOver()
    {
        if(!IsActive) return;

        UpdateTooltip();
    }

    protected override void OnMouseExit()
    {
        activeTooltipString = "";

        base.OnMouseExit();
    }

    public override void OnClick(int mouseButton)
    {
        if(!IsActive) return;

        if(mouseButton == 0)
        {
            blackjackManager.OnStand();

            UpdateTooltip();
        }
        else if(mouseButton == 1 && blackjackManager.CanSplit())
        {
            blackjackManager.OnSplit();

            OnRemoveOutline();
        }
    }

    private void UpdateTooltip()
    {
        string content = blackjackManager.CanSplit() ? splitTooltip : standardStandTooltip;

        if(activeTooltipString != content)
        {
            activeTooltipString = content;

            TooltipManager.instance.ShowTooltip(activeTooltipString, tooltipHeader);
        }
    }
}