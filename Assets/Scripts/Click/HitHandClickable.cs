using UnityEngine;

public class HitHandClickable : Clickable
{
    [SerializeField] private BlackjackGame gameReference;
    [SerializeField] [TextArea] private string standardHitTooltip;
    [SerializeField] [TextArea] private string doubleDownTooltip;

    protected override void OnMouseEnter()
    {
        if(!IsActive) return;

        string activeContent = gameReference.canDoubleDown ? doubleDownTooltip : standardHitTooltip;

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

        if(mouseButton == 0)
        {
            gameReference.OnHit();
        }
        else if(mouseButton == 1 && gameReference.canDoubleDown)
        {
            gameReference.OnDoubleDown();

            OnRemoveOutline();
        }
    }
}