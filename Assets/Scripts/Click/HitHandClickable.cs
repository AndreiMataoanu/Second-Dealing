using UnityEngine;

public class HitHandClickable : Clickable
{
    [SerializeField] private BlackjackGame gameReference;
    [SerializeField] private string standardHitTooltip = "Left-click: Hit";
    [SerializeField] private string doubleDownTooltip = "Left-click: Hit\nRight-click: Double Down";

    protected override void OnMouseEnter()
    {
        if(!IsActive) return;

        string activeContent = gameReference.CanPlayerDoubleDown ? doubleDownTooltip : standardHitTooltip;

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

        if(mouseButton == 1 && gameReference.CanPlayerDoubleDown)
        {
            gameReference.OnDoubleDown();
        }
        else
        {
            gameReference.OnHit();
        }
    }
}