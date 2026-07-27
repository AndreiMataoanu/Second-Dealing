using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Clickable : MonoBehaviour
{
    [SerializeField] private UnityEvent clickEvent;
    [SerializeField] private Material outline;

    [Header("Tooltip Settings")]
    [SerializeField] public string tooltipHeader;
    [TextArea][SerializeField] private string tooltipContent;
    [SerializeField] protected bool showTooltip = true;

    private MeshCollider meshCollider;
    private Renderer meshRenderer;
    private List<Renderer> childRenderers = new();
    private bool hasOutline;
    protected bool IsActive;

    public void SetActive(bool active) => IsActive = active;
    public void SetColliderActive(bool active) => meshCollider.enabled = active;
    public bool IsVisible => meshRenderer.enabled;

    public void SetVisibility(bool active)
    {
        meshRenderer.enabled = active;
        childRenderers.ForEach(r => r.enabled = active);
    }
    
    private void Awake()
    {
        meshCollider = GetComponent<MeshCollider>();
        if (!meshCollider)
            meshCollider = gameObject.AddComponent<MeshCollider>();
        
        meshRenderer = GetComponent<Renderer>();
        childRenderers = new List<Renderer>(gameObject.GetComponentsInChildren<MeshRenderer>());
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

        if(!outline || !hasOutline) return;
        
        var materials = new List<Material>(meshRenderer.materials);
        materials.RemoveAt(materials.Count - 1);
        meshRenderer.materials = materials.ToArray();
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
        if(!outline || hasOutline || meshRenderer == null) return;

        var materials = new List<Material>(meshRenderer.materials) { outline };

        meshRenderer.materials = materials.ToArray();
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
}
