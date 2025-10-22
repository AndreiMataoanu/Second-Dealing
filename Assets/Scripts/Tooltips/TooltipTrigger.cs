using UnityEngine;

public class TooltipTrigger : MonoBehaviour
{
    [Tooltip("The message to display in the tooltip.")]
    [TextArea(3, 10)]
    [SerializeField] private string tooltipMessage;

    private void OnMouseEnter()
    {
        TooltipManager.instance.ShowTooltip(tooltipMessage);
    }

    private void OnMouseExit()
    {
        TooltipManager.instance.HideTooltip();
    }
}