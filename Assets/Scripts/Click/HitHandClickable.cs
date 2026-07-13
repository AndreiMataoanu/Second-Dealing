using UnityEngine;

public class HitHandClickable : Clickable
{
    [SerializeField] private BlackjackGame blackjackManager;
    [SerializeField] [TextArea] private string standardHitTooltip;
    [SerializeField] [TextArea] private string doubleDownTooltip;
    private string activeTooltipString = "";

    protected override void OnMouseEnter()
    {
        if(!IsActive) return;

        UpdateTooltip();
        ApplyOutline();
    }

    private void OnMouseOver()
    {
        if(!IsActive || !blackjackManager.canDoubleDown) return;

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
            blackjackManager.OnHit();

            UpdateTooltip();
        }
        else if(mouseButton == 1 && blackjackManager.canDoubleDown)
        {
            blackjackManager.OnDoubleDown();

            OnRemoveOutline();
        }
    }

    private void UpdateTooltip()
    {
        string content = blackjackManager.canDoubleDown ? doubleDownTooltip : standardHitTooltip;

        if(activeTooltipString != content)
        {
            activeTooltipString = content;

            TooltipManager.instance.ShowTooltip(activeTooltipString, tooltipHeader);
        }
    }
}