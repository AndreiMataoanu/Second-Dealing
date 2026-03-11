using UnityEngine;

public class StandHandClickable : Clickable
{
    [SerializeField] private BlackjackGame gameReference;
    [SerializeField] private string standardHitTooltip = "Left-click: Stand";
    [SerializeField] private string splitTooltip = "Left-click: Stand\nRight-click: Split";

    protected override void OnMouseEnter()
    {
        if(!IsActive) return;

        string activeContent = gameReference.CanSplit() ? splitTooltip : standardHitTooltip;

        TooltipManager.instance.ShowTooltip(activeContent, tooltipHeader);

        ApplyOutline();
    }

    protected override void OnMouseExit()
    {
        base.OnMouseExit();
    }

    public override void OnClick(int mouseButton)
    {
        if(!IsActive) return;

        if(mouseButton == 1)
        {
            gameReference.OnSplit();
        }
        else
        {
            gameReference.OnStand();
        }
    }
}