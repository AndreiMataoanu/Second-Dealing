using System.Collections;
using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager instance;

    [SerializeField] private Tooltip tooltip;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowTooltip(string content, string header = "")
    {
        this.tooltip.SetText(content, header);
        this.tooltip.gameObject.SetActive(true);
    }

    public void HideTooltip()
    {
        this.tooltip.gameObject.SetActive(false);
    }
}