using UnityEngine;

public class StandHandClickable : Clickable
{
    [SerializeField] private BlackjackGame gameReference;
    [SerializeField] [TextArea] private string standardStandTooltip;
    [SerializeField] [TextArea] private string splitTooltip;

    protected override void OnMouseEnter()
    {
        if(!IsActive) return;

        string activeContent = gameReference.CanSplit() ? splitTooltip : standardStandTooltip;

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

            OnRemoveOutline();
        }
    }
}