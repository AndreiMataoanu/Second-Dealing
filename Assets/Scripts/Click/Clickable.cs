using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
//using OutlineFx;

public class Clickable : MonoBehaviour
{
    [SerializeField] private UnityEvent clickEvent;
    //[SerializeField] private Material outline;
    [SerializeField] private Color defaultOutlineColor = Color.white;

    [Header("Tooltip Settings")]
    [SerializeField] public string tooltipHeader;
    [TextArea][SerializeField] private string tooltipContent;
    [SerializeField] protected bool showTooltip = true;

    private MeshCollider meshCollider;
    private Renderer meshRenderer;
    private List<Renderer> meshRenderers = new();
    private OutlineFx.OutlineFx outlineFx;
    private bool hasOutline;
    protected bool IsActive;

    public void SetActive(bool active) => IsActive = active;
    public void SetColliderActive(bool active) => meshCollider.enabled = active;
    public bool IsVisible => meshRenderer.enabled;

    public void SetVisibility(bool active)
    {
        meshRenderer.enabled = active;
        meshRenderers.ForEach(r => r.enabled = active);
    }

    public bool ShowTooltip
    {
        get => showTooltip;
        set => showTooltip = value;
    }

    protected virtual void Awake()
    {
        meshCollider = GetComponent<MeshCollider>();
        if (!meshCollider)
            meshCollider = gameObject.AddComponent<MeshCollider>();
        
        meshRenderer = GetComponent<Renderer>();
        meshRenderers = new List<Renderer>(gameObject.GetComponentsInChildren<MeshRenderer>());
        outlineFx = GetComponent<OutlineFx.OutlineFx>();

        if(!outlineFx)
        {
            outlineFx = gameObject.AddComponent<OutlineFx.OutlineFx>();
        }

        outlineFx.enabled = false;
    }

    protected virtual void OnMouseEnter()
    {
        if(!IsActive) return;

        ApplyOutline();

        if(!showTooltip) return;

        TooltipManager.instance.ShowTooltip(GetTooltipContent(), GetTooltipHeader());
    }

    protected virtual void OnMouseExit()
    {
        if(!IsActive) return;

        OnRemoveOutline();
    }

    public void OnRemoveOutline(bool hideTooltip = true)
    {
        if(hideTooltip) TooltipManager.instance.HideTooltip();

        if(!hasOutline) return;
        
        outlineFx.enabled = false;
        hasOutline = false;
    }

    public virtual void OnClick(int mouseButton = 0)
    {
        if (!IsActive) return;

        if(mouseButton == 0)
        {
            clickEvent?.Invoke();
        }

        OnRemoveOutline();
    }

    public virtual void ApplyOutline()
    {
        if(hasOutline) return;

        outlineFx.Color = GetOutlineColor();
        outlineFx.enabled = true;
        hasOutline = true;
    }

    protected virtual string GetTooltipHeader()
    {
        return tooltipHeader;
    }

    protected virtual string GetTooltipContent()
    {
        return $"\n{tooltipContent}";
    }

    //protected virtual Material GetOutlineMaterial()
    //{
    //    return outline;
    //}

    protected virtual Color GetOutlineColor()
    {
        return defaultOutlineColor;
    }
}
